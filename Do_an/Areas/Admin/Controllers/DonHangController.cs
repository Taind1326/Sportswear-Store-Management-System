using System;
using System.Linq;
using System.Web.Mvc;
using Do_an.Models;
using Do_an.Services;
using Do_an.ViewModels;

namespace Do_an.Areas.Admin.Controllers
{
    [YeuCauAdmin]
    public class DonHangController : Controller
    {
        private readonly DonHangService dichVu = new DonHangService();

        [HttpGet]
        public ActionResult Index(string trangThai, string tuKhoa, int trang = 1)
        {
            using (var db = new SPORTIXEntities())
            {
                var q = dichVu.TruyVan(db, null, trangThai, tuKhoa);
                int tong = q.Count();
                int tongTrang = Math.Max(1, (int)Math.Ceiling(tong / (double)DonHangService.KichThuocTrang));
                trang = Math.Max(1, Math.Min(trang, tongTrang));

                var ds = q.OrderByDescending(x => x.NgayDat).ThenByDescending(x => x.MaDonHang)
                    .Skip((trang - 1) * DonHangService.KichThuocTrang)
                    .Take(DonHangService.KichThuocTrang)
                    .ToList();

                return View(new DanhSachDonHangViewModel
                {
                    DonHangs = ds,
                    TrangThai = trangThai,
                    TuKhoa = tuKhoa,
                    Trang = trang,
                    TongTrang = tongTrang,
                    TongSo = tong
                });
            }
        }

        [HttpGet]
        public ActionResult ChiTiet(int id)
        {
            using (var db = new SPORTIXEntities())
            {
                DonHang don = db.DonHangs.AsNoTracking().FirstOrDefault(x => x.MaDonHang == id);
                if (don == null) return HttpNotFound();

                return View(new ChiTietDonHangViewModel
                {
                    Don = don,
                    Dong = db.ChiTietDonHangs.AsNoTracking()
                        .Where(x => x.MaDonHang == id).OrderBy(x => x.MaChiTietDonHang).ToList(),
                    ThanhToans = db.ThanhToans.AsNoTracking()
                        .Where(x => x.MaDonHang == id).ToList()
                });
            }
        }

        // Xác nhận đơn COD / chuyển đang giao / đã giao: đi đúng từng bước.
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult CapNhatTrangThai(int id, string trangThaiHienTai)
        {
            string tb;
            bool ok = dichVu.ChuyenBuocKeTiep(id, trangThaiHienTai, out tb);
            TempData[ok ? "ThanhCong" : "Loi"] = tb;
            return RedirectToAction("ChiTiet", new { id });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult XacNhanThuTien(int id)
        {
            string tb;
            bool ok = dichVu.XacNhanThuTienCod(id, XacThucService.LayMaTaiKhoanHienTai().Value, out tb);
            TempData[ok ? "ThanhCong" : "Loi"] = tb;
            return RedirectToAction("ChiTiet", new { id });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Huy(int id, string lyDo)
        {
            if (string.IsNullOrWhiteSpace(lyDo))
            {
                TempData["Loi"] = "Vui lòng nhập lý do hủy đơn.";
                return RedirectToAction("ChiTiet", new { id });
            }

            int admin = XacThucService.LayMaTaiKhoanHienTai().Value;

            switch (dichVu.HuyDon(id, lyDo, admin, null))
            {
                case KetQuaHuyDon.ThanhCong:
                    TempData["ThanhCong"] = "Đã hủy đơn #" + id + ", hoàn tồn kho và lượt dùng mã.";
                    break;
                case KetQuaHuyDon.DaHuyTruocDo:
                    TempData["Loi"] = "Đơn hàng này đã được hủy trước đó.";
                    break;
                case KetQuaHuyDon.KhongTheHuy:
                    TempData["Loi"] = "Chỉ hủy được đơn Chờ xác nhận hoặc Đã xác nhận (trước khi giao).";
                    break;
                default:
                    return HttpNotFound();
            }

            return RedirectToAction("ChiTiet", new { id });
        }
    }
}
