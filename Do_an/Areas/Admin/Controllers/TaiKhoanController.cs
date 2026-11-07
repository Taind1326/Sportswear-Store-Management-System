using System;
using System.Linq;
using System.Web.Mvc;
using Do_an.Models;
using Do_an.Services;
using Do_an.ViewModels;

namespace Do_an.Areas.Admin.Controllers
{
    [YeuCauAdmin]
    public class TaiKhoanController : Controller
    {
        private const int SoDongMoiTrang = 10;

        private readonly SPORTIXEntities db = new SPORTIXEntities();

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }

        public ActionResult Index(string tuKhoa, string trangThai, int trang = 1)
        {
            tuKhoa = (tuKhoa ?? "").Trim();
            if (tuKhoa.Length > 100) tuKhoa = tuKhoa.Substring(0, 100);

            if (trangThai != "hoatdong" && trangThai != "khoa") trangThai = "";

            // Chỉ quản lý KHÁCH HÀNG (vai trò User); không động tới tài khoản Admin.
            IQueryable<TaiKhoan> truyVan = db.TaiKhoans
                .AsNoTracking()
                .Where(x => x.VaiTro == XacThucService.VaiTroUser);

            if (tuKhoa.Length > 0)
            {
                truyVan = truyVan.Where(x =>
                    x.HoTen.Contains(tuKhoa)
                    || x.TenDangNhap.Contains(tuKhoa)
                    || x.Email.Contains(tuKhoa)
                    || (x.SoDienThoai != null && x.SoDienThoai.Contains(tuKhoa)));
            }

            if (trangThai == "hoatdong") truyVan = truyVan.Where(x => x.HoatDong);
            else if (trangThai == "khoa") truyVan = truyVan.Where(x => !x.HoatDong);

            int tongSo = truyVan.Count();
            int tongTrang = Math.Max(1, (int)Math.Ceiling(tongSo / (double)SoDongMoiTrang));
            trang = Math.Min(Math.Max(trang, 1), tongTrang);

            var danhSach = truyVan
                .OrderByDescending(x => x.NgayTao)
                .ThenByDescending(x => x.MaTaiKhoan)
                .Skip((trang - 1) * SoDongMoiTrang)
                .Take(SoDongMoiTrang)
                .Select(x => new KhachHangItemViewModel
                {
                    MaTaiKhoan = x.MaTaiKhoan,
                    TenDangNhap = x.TenDangNhap,
                    HoTen = x.HoTen,
                    Email = x.Email,
                    SoDienThoai = x.SoDienThoai,
                    HoatDong = x.HoatDong,
                    NgayTao = x.NgayTao,
                    SoDonHang = x.DonHangs.Count()
                })
                .ToList();

            ViewBag.TuKhoa = tuKhoa;
            ViewBag.TrangThai = trangThai;
            ViewBag.Trang = trang;
            ViewBag.TongTrang = tongTrang;
            ViewBag.TongSo = tongSo;

            return View(danhSach);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DoiTrangThai(int id, string tuKhoa, string trangThai, int trang = 1)
        {
            TaiKhoan tk = db.TaiKhoans.FirstOrDefault(
                x => x.MaTaiKhoan == id && x.VaiTro == XacThucService.VaiTroUser);

            if (tk == null)
            {
                TempData["Loi"] = "Không tìm thấy khách hàng.";
            }
            else
            {
                tk.HoatDong = !tk.HoatDong;
                db.SaveChanges();

                TempData["ThanhCong"] = tk.HoatDong
                    ? "Đã mở khóa tài khoản " + tk.TenDangNhap + "."
                    : "Đã khóa tài khoản " + tk.TenDangNhap + ".";
            }

            return RedirectToAction("Index", new { tuKhoa, trangThai, trang });
        }
    }
}