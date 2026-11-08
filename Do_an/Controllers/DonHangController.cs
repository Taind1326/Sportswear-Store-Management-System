using System;
using System.Linq;
using System.Web.Mvc;
using Do_an.Models;
using Do_an.Services;
using Do_an.ViewModels;

namespace Do_an.Controllers
{
    // Phía khách: lịch sử, chi tiết, hủy đơn. TV3 bổ sung DatHang/ThanhCong ở controller này.
    [YeuCauDangNhap]
    public partial class DonHangController : Controller
    {
        private readonly DonHangService dichVu = new DonHangService();

        // _TaiKhoanMenu đang trỏ tới DonHang/Index ("Đơn hàng của tôi").
        [HttpGet]
        public ActionResult Index(string trangThai, int trang = 1)
        {
            return LichSu(trangThai, trang);
        }

        [HttpGet]
        public ActionResult LichSu(string trangThai, int trang = 1)
        {
            int ma = XacThucService.LayMaTaiKhoanHienTai().Value;

            using (var db = new SPORTIXEntities())
            {
                var q = dichVu.TruyVan(db, ma, trangThai, null);
                int tong = q.Count();
                int tongTrang = Math.Max(1, (int)Math.Ceiling(tong / (double)DonHangService.KichThuocTrang));
                trang = Math.Max(1, Math.Min(trang, tongTrang));

                var ds = q.OrderByDescending(x => x.NgayDat).ThenByDescending(x => x.MaDonHang)
                    .Skip((trang - 1) * DonHangService.KichThuocTrang)
                    .Take(DonHangService.KichThuocTrang)
                    .ToList();

                return View("LichSu", new DanhSachDonHangViewModel
                {
                    DonHangs = ds,
                    TrangThai = trangThai,
                    Trang = trang,
                    TongTrang = tongTrang,
                    TongSo = tong
                });
            }
        }

        [HttpGet]
        public ActionResult ChiTiet(int id)
        {
            int ma = XacThucService.LayMaTaiKhoanHienTai().Value;

            using (var db = new SPORTIXEntities())
            {
                // Lọc theo MaTaiKhoan: đơn người khác trả 404, không lộ sự tồn tại.
                DonHang don = db.DonHangs.AsNoTracking()
                    .FirstOrDefault(x => x.MaDonHang == id && x.MaTaiKhoan == ma);

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

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Huy(int id, string lyDo)
        {
            int ma = XacThucService.LayMaTaiKhoanHienTai().Value;

            KetQuaHuyDon kq = dichVu.HuyDon(id, lyDo, ma, ma);

            switch (kq)
            {
                case KetQuaHuyDon.ThanhCong:
                    TempData["ThanhCong"] = "Đã hủy đơn #" + id + ".";
                    break;
                case KetQuaHuyDon.DaHuyTruocDo:
                    TempData["Loi"] = "Đơn hàng này đã được hủy trước đó.";
                    break;
                case KetQuaHuyDon.KhongTheHuy:
                    TempData["Loi"] = "Chỉ hủy được đơn đang ở trạng thái Chờ xác nhận.";
                    break;
                default:
                    return HttpNotFound();
            }

            return RedirectToAction("ChiTiet", new { id });
        }

        // API cho TV3: kiểm tra mã khi khách nhập. Chỉ trả về kết quả tính của server.
        // LƯU Ý: tienHang phải do TV3 tính lại từ giỏ hàng trên server khi đặt hàng
        // (gọi KhuyenMaiService.KiemTraVaTinh trực tiếp), không dùng số từ trình duyệt.
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult KiemTraMa(string maCode, decimal tienHang)
        {
            KetQuaMaGiamGia kq = KhuyenMaiService.KiemTraVaTinh(maCode, tienHang);

            return Json(new
            {
                hopLe = kq.HopLe,
                loi = kq.LoiHienThi,
                tienGiam = kq.TienGiam,
                tienSauGiam = kq.TienSauGiam
            });
        }
    }
}
