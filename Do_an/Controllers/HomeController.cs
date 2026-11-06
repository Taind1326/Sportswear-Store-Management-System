using Do_an.Models;
using Do_an.ViewModels;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Do_an.Controllers
{
    public class HomeController : Controller
    {
        private readonly SPORTIXEntities db = new SPORTIXEntities();

        private IQueryable<LoaiHang> DanhMucDangBan()
        {
            return db.LoaiHangs.AsNoTracking()
                .Where(x =>
                    x.HoatDong &&
                    x.SanPhams.Any(sp =>
                        sp.DangBan &&
                        sp.BienTheSanPhams.Any(bt => bt.DangBan)));
        }

        private TrangChuViewModel LayDanhMucTrangChu()
        {
            var maLoaiHangBongDa = db.LoaiHangs.AsNoTracking()
                .Where(x =>
                    x.HoatDong &&
                    x.TenLoaiHang.ToLower().Contains("bóng đá"))
                .OrderBy(x => x.MaLoaiHang)
                .Select(x => (int?)x.MaLoaiHang)
                .FirstOrDefault();

            return new TrangChuViewModel
            {
                MaLoaiHangBongDa = maLoaiHangBongDa,

                DanhMuc = DanhMucDangBan()
                    .Where(x =>
                        !x.TenLoaiHang.ToLower().Contains("bóng đá"))
                    .OrderBy(x => x.TenLoaiHang)
                    .Take(3)
                    .ToList()
            };
        }

        [HttpGet]
        public ActionResult Index()
        {
            IQueryable<SanPham> query = db.SanPhams.AsNoTracking()
                .Where(x =>
                    x.DangBan &&
                    x.BienTheSanPhams.Any(bt => bt.DangBan));

            var model = LayDanhMucTrangChu();

            model.TongSanPham = query.Count();
            model.TongLoaiHang = DanhMucDangBan().Count();

            model.TongThuongHieu = query
                .Select(x => x.MaThuongHieu)
                .Distinct()
                .Count();

            model.TongKhachHang = db.TaiKhoans.AsNoTracking()
                .Count(x => x.VaiTro == "User");

            model.MucGiamCaoNhat = query
                .Select(x => (int?)x.PhanTramGiam)
                .Max() ?? 0;

            model.SanPhamNoiBat = query
                .Include(x => x.LoaiHang)
                .Include(x => x.HinhAnhSanPhams)
                .Include(x => x.BienTheSanPhams)
                .OrderByDescending(x => x.NoiBat)
                .ThenByDescending(x => x.NgayTao)
                .ThenByDescending(x => x.MaSanPham)
                .Take(8)
                .ToList();

            return View(model);
        }

        [ChildActionOnly]
        public ActionResult DanhMucFooter()
        {
            return PartialView(
                "~/Views/Shared/_DanhMucFooter.cshtml",
                LayDanhMucTrangChu());
        }

        [HttpGet]
        public ActionResult GioiThieu()
        {
            return View();
        }

        [HttpGet]
        public ActionResult LienHe()
        {
            return View();
        }

        [HttpGet]
        public ActionResult ChinhSachBaoMat()
        {
            return View();
        }

        [HttpGet]
        public ActionResult DieuKhoanDichVu()
        {
            return View();
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