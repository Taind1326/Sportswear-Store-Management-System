using Do_an.Models;
using Do_an.ViewModels;
using System;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Do_an.Areas.Admin.Controllers
{
    public class LoaiHangController : Controller
    {
        private readonly SPORTIXEntities db = new SPORTIXEntities();

        protected override void OnActionExecuting(
            ActionExecutingContext filterContext)
        {
            int maTaiKhoan;

            bool daDangNhap = int.TryParse(
                Convert.ToString(Session["MaTaiKhoan"]),
                out maTaiKhoan);

            if (!daDangNhap || maTaiKhoan <= 0)
            {
                filterContext.Result = new HttpStatusCodeResult(
                    HttpStatusCode.Unauthorized,
                    "Vui lòng đăng nhập bằng tài khoản quản trị.");

                return;
            }

            bool coQuyen = db.TaiKhoans
                .AsNoTracking()
                .Any(x =>
                    x.MaTaiKhoan == maTaiKhoan &&
                    x.VaiTro == "Admin" &&
                    x.HoatDong);

            if (!coQuyen)
            {
                filterContext.Result = new HttpStatusCodeResult(
                    HttpStatusCode.Forbidden,
                    "Bạn không có quyền quản lý loại hàng.");

                return;
            }

            base.OnActionExecuting(filterContext);
        }

        [HttpGet]
        public ActionResult Index(string tuKhoa, int trang = 1)
        {
            const int kichThuocTrang = 20;

            tuKhoa = (tuKhoa ?? "").Trim();

            IQueryable<LoaiHang> query = db.LoaiHangs.AsNoTracking();

            if (tuKhoa.Length > 0)
            {
                query = query.Where(x =>
                    x.TenLoaiHang.Contains(tuKhoa));
            }

            int tongSo = query.Count();

            int tongTrang = Math.Max(
                1,
                (int)Math.Ceiling(
                    tongSo / (double)kichThuocTrang));

            trang = Math.Max(1, Math.Min(trang, tongTrang));

            var danhSach = query
                .OrderBy(x => x.TenLoaiHang)
                .ThenBy(x => x.MaLoaiHang)
                .Skip((trang - 1) * kichThuocTrang)
                .Take(kichThuocTrang)
                .ToList();

            ViewBag.TuKhoa = tuKhoa;
            ViewBag.Trang = trang;
            ViewBag.TongTrang = tongTrang;
            ViewBag.TongSo = tongSo;

            return View(danhSach);
        }

        [HttpGet]
        public ActionResult Them()
        {
            return View("Form", new LoaiHangViewModel
            {
                HoatDong = true
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Them(LoaiHangViewModel model)
        {
            // Mã loại hàng do database sinh.
            model.MaLoaiHang = 0;
            ModelState.Remove("MaLoaiHang");

            KiemTraThongTin(model);

            if (!ModelState.IsValid)
            {
                return View("Form", model);
            }

            var loaiHang = new LoaiHang
            {
                TenLoaiHang = model.TenLoaiHang,
                MoTa = model.MoTa,
                HoatDong = model.HoatDong
            };

            db.LoaiHangs.Add(loaiHang);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    "",
                    "Chưa thêm được loại hàng. Vui lòng thử lại.");

                return View("Form", model);
            }

            TempData["ThanhCong"] = "Thêm loại hàng thành công.";

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Sua(int? id)
        {
            if (!id.HasValue || id.Value <= 0)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest,
                    "Mã loại hàng không hợp lệ.");
            }

            var loaiHang = db.LoaiHangs
                .AsNoTracking()
                .SingleOrDefault(x => x.MaLoaiHang == id.Value);

            if (loaiHang == null)
            {
                return HttpNotFound("Không tìm thấy loại hàng.");
            }

            return View("Form", new LoaiHangViewModel
            {
                MaLoaiHang = loaiHang.MaLoaiHang,
                TenLoaiHang = loaiHang.TenLoaiHang,
                MoTa = loaiHang.MoTa,
                HoatDong = loaiHang.HoatDong
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Sua(LoaiHangViewModel model)
        {
            if (model.MaLoaiHang <= 0)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest,
                    "Mã loại hàng không hợp lệ.");
            }

            var loaiHang = db.LoaiHangs
                .SingleOrDefault(x =>
                    x.MaLoaiHang == model.MaLoaiHang);

            if (loaiHang == null)
            {
                return HttpNotFound("Không tìm thấy loại hàng.");
            }

            KiemTraThongTin(model);

            if (!ModelState.IsValid)
            {
                return View("Form", model);
            }

            loaiHang.TenLoaiHang = model.TenLoaiHang;
            loaiHang.MoTa = model.MoTa;
            loaiHang.HoatDong = model.HoatDong;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    "",
                    "Chưa lưu được loại hàng. Vui lòng thử lại.");

                return View("Form", model);
            }

            TempData["ThanhCong"] = "Cập nhật loại hàng thành công.";

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DoiTrangThai(int id, bool hoatDong)
        {
            if (id <= 0)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest,
                    "Mã loại hàng không hợp lệ.");
            }

            var loaiHang = db.LoaiHangs
                .SingleOrDefault(x => x.MaLoaiHang == id);

            if (loaiHang == null)
            {
                return HttpNotFound("Không tìm thấy loại hàng.");
            }

            loaiHang.HoatDong = hoatDong;

            try
            {
                db.SaveChanges();

                TempData["ThanhCong"] = hoatDong
                    ? "Đã mở sử dụng loại hàng."
                    : "Đã ngừng sử dụng loại hàng.";
            }
            catch (DbUpdateException)
            {
                TempData["Loi"] =
                    "Chưa cập nhật được trạng thái. Vui lòng thử lại.";
            }

            return RedirectToAction("Index");
        }

        private void KiemTraThongTin(LoaiHangViewModel model)
        {
            model.TenLoaiHang = string.Join(
                " ",
                (model.TenLoaiHang ?? "").Split(
                    (char[])null,
                    StringSplitOptions.RemoveEmptyEntries));

            model.MoTa = (model.MoTa ?? "").Trim();

            if (model.TenLoaiHang.Length == 0)
            {
                if (ModelState.IsValidField("TenLoaiHang"))
                {
                    ModelState.AddModelError(
                        "TenLoaiHang",
                        "Vui lòng nhập tên loại hàng.");
                }

                return;
            }

            string tenKiemTra = model.TenLoaiHang.ToUpper();

            bool trungTen = db.LoaiHangs.Any(x =>
                x.MaLoaiHang != model.MaLoaiHang &&
                x.TenLoaiHang.ToUpper() == tenKiemTra);

            if (trungTen)
            {
                ModelState.AddModelError(
                    "TenLoaiHang",
                    "Tên loại hàng đã tồn tại, kể cả loại đã ngừng sử dụng.");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}