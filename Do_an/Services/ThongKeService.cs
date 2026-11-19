using System;
using System.Data.Entity;
using System.Linq;
using Do_an.Models;
using Do_an.ViewModels;

namespace Do_an.Services
{
    public class ThongKeService
    {
        public void TongHop(SPORTIXEntities db, DateTime batDau, DateTime denTruoc, ThongKeViewModel model)
        {
            model.TheoNgay = (denTruoc - batDau).Days <= 31;
            bool theoNgay = model.TheoNgay;
            var thanhToans = db.ThanhToans.AsNoTracking();

            // Giữ ngày thu của khoản đã hoàn. Ghi khoản hoàn vào ngày phát sinh hoàn tiền.
            var cacKhoanThu = thanhToans
                .Where(x => (x.TrangThai == TrangThaiThanhToan.DaThanhToan ||
                             x.TrangThai == TrangThaiThanhToan.DaHoanTien) &&
                            x.NgayThanhToan >= batDau && x.NgayThanhToan < denTruoc)
                .Select(x => new { Ngay = x.NgayThanhToan.Value, TienThu = x.SoTien, TienHoan = 0m });

            var cacKhoanHoan = thanhToans
                .Where(x => x.TrangThai == TrangThaiThanhToan.DaHoanTien &&
                            x.NgayHoanTien >= batDau && x.NgayHoanTien < denTruoc)
                .Select(x => new { Ngay = x.NgayHoanTien.Value, TienThu = 0m, TienHoan = x.SoTien });

            var cacKyCoGiaoDich = cacKhoanThu.Concat(cacKhoanHoan)
                .GroupBy(x => new { x.Ngay.Year, x.Ngay.Month, Ngay = theoNgay ? x.Ngay.Day : 1 })
                .Select(x => new
                {
                    x.Key.Year,
                    x.Key.Month,
                    x.Key.Ngay,
                    TienThu = x.Sum(khoan => khoan.TienThu),
                    TienHoan = x.Sum(khoan => khoan.TienHoan)
                })
                .ToList()
                .ToDictionary(x => new DateTime(x.Year, x.Month, x.Ngay));

            var ngayDauKy = theoNgay ? batDau : new DateTime(batDau.Year, batDau.Month, 1);
            var ngayCuoi = denTruoc.AddDays(-1);
            var ngayDauKyCuoi = theoNgay ? ngayCuoi : new DateTime(ngayCuoi.Year, ngayCuoi.Month, 1);
            for (var ky = ngayDauKy; ; ky = theoNgay ? ky.AddDays(1) : ky.AddMonths(1))
            {
                var giaoDich = cacKyCoGiaoDich.ContainsKey(ky) ? cacKyCoGiaoDich[ky] : null;
                model.CacKy.Add(new ThongKeKyViewModel
                {
                    BatDau = ky,
                    TienThu = giaoDich?.TienThu ?? 0m,
                    TienHoan = giaoDich?.TienHoan ?? 0m
                });

                if (ky == ngayDauKyCuoi)
                    break;
            }

            model.TrangThaiDon = db.DonHangs.AsNoTracking()
                .Where(x => x.NgayDat >= batDau && x.NgayDat < denTruoc)
                .GroupBy(x => x.TrangThai)
                .Select(x => new ThongKeTrangThaiViewModel { TrangThai = x.Key, SoDon = x.Count() })
                .ToList();

            model.SanPhamBanChay = db.ChiTietDonHangs.AsNoTracking()
                .Where(x => x.DonHang.NgayDat >= batDau && x.DonHang.NgayDat < denTruoc &&
                            x.DonHang.TrangThai == TrangThaiDonHang.DaGiao)
                .GroupBy(x => new
                {
                    x.BienTheSanPham.MaSanPham,
                    x.BienTheSanPham.SanPham.TenSanPham
                })
                .Select(x => new ThongKeSanPhamViewModel
                {
                    MaSanPham = x.Key.MaSanPham,
                    TenSanPham = x.Key.TenSanPham,
                    SoLuong = x.Sum(dong => (long)dong.SoLuong)
                })
                .OrderByDescending(x => x.SoLuong)
                .ThenBy(x => x.TenSanPham)
                .ThenBy(x => x.MaSanPham)
                .Take(5)
                .ToList();
        }
    }
}
