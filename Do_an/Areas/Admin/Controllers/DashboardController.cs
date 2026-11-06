using Do_an.Models;
using Do_an.Services;
using Do_an.ViewModels;
using System;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Do_an.Areas.Admin.Controllers
{
    public class DashboardController : Controller
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
                    HttpStatusCode.Unauthorized);

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
                    HttpStatusCode.Forbidden);

                return;
            }

            base.OnActionExecuting(filterContext);
        }

        [HttpGet]
        public ActionResult Index()
        {
            var canhBaos = db.CanhBaoTonKhoes
                .AsNoTracking()
                .Include(x => x.BienTheSanPham.SanPham)
                .Where(x =>
                    x.DangCanhBao &&
                    x.BienTheSanPham.DangBan &&
                    x.BienTheSanPham.SanPham.DangBan &&
                    x.BienTheSanPham.SoLuongTon
                        <= CanhBaoTonKhoService.NguongSapHet)
                .OrderBy(x => x.BienTheSanPham.SoLuongTon)
                .ThenBy(x =>
                    x.BienTheSanPham.SanPham.TenSanPham)
                .ThenBy(x => x.MaBienThe)
                .ToList();

            return View(canhBaos);
        }

        [HttpGet]
        public ActionResult MoBoSungHang(int id, int maBienThe)
        {
            if (id <= 0 || maBienThe <= 0)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest);
            }

            var phanLoai = db.BienTheSanPhams
                .AsNoTracking()
                .Include(x => x.SanPham)
                .SingleOrDefault(x =>
                    x.MaBienThe == maBienThe &&
                    x.MaSanPham == id);

            if (phanLoai == null)
            {
                return HttpNotFound(
                    "Không tìm thấy phân loại thuộc sản phẩm này.");
            }

            var danhSach = db.BienTheSanPhams
                .AsNoTracking()
                .Where(x => x.MaSanPham == id)
                .OrderBy(x => x.MauSac)
                .ThenBy(x => x.KichThuoc)
                .ToList();

            var vi = CultureInfo.GetCultureInfo("vi-VN");

            var model = new BoSungHangViewModel
            {
                MaSanPham = id,
                TenSanPham = phanLoai.SanPham.TenSanPham,
                MaBienThe = maBienThe,
                YeuCauId = Guid.NewGuid(),

                DanhSachPhanLoai = danhSach
                    .Select(x => new SelectListItem
                    {
                        Value = x.MaBienThe.ToString(),
                        Text = x.MauSac
                            + " / "
                            + x.KichThuoc
                            + " — đang còn "
                            + x.SoLuongTon.ToString("N0", vi)
                            + (x.DangBan ? "" : " (ngừng bán)"),
                        Selected = x.MaBienThe == maBienThe
                    })
                    .ToList()
            };

            return View(
                "~/Areas/Admin/Views/SanPham/BoSungHang.cshtml",
                model);
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