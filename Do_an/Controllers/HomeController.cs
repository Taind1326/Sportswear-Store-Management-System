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

        [HttpGet]
        public ActionResult Index()
        {
            IQueryable<SanPham> query = db.SanPhams
                .AsNoTracking()
                .Where(x =>
                    x.DangBan &&
                    x.BienTheSanPhams.Any(bt => bt.DangBan));

            IQueryable<LoaiHang> danhMuc = db.LoaiHangs
                .AsNoTracking()
                .Where(x =>
                    x.HoatDong &&
                    x.SanPhams.Any(sp =>
                        sp.DangBan &&
                        sp.BienTheSanPhams.Any(bt => bt.DangBan)));

            var maLoaiHangBongDa = db.LoaiHangs
                .AsNoTracking()
                .Where(x =>
                    x.HoatDong &&
                    x.TenLoaiHang.ToLower().Contains("bóng đá"))
                .OrderBy(x => x.MaLoaiHang)
                .Select(x => (int?)x.MaLoaiHang)
                .FirstOrDefault();

            var model = new TrangChuViewModel
            {
                TongSanPham = query.Count(),

                TongLoaiHang = danhMuc.Count(),

                TongThuongHieu = query
                    .Select(x => x.MaThuongHieu)
                    .Distinct()
                    .Count(),

                TongKhachHang = db.TaiKhoans
                    .AsNoTracking()
                    .Count(x => x.VaiTro == "User"),

                MucGiamCaoNhat = query
                    .Select(x => (int?)x.PhanTramGiam)
                    .Max() ?? 0,

                MaLoaiHangBongDa = maLoaiHangBongDa,

                DanhMuc = danhMuc
                    .Where(x =>
                        !x.TenLoaiHang.ToLower().Contains("bóng đá"))
                    .OrderBy(x => x.TenLoaiHang)
                    .Take(3)
                    .ToList(),

                SanPhamNoiBat = query
                    .Include(x => x.LoaiHang)
                    .Include(x => x.HinhAnhSanPhams)
                    .Include(x => x.BienTheSanPhams)
                    .OrderByDescending(x => x.NoiBat)
                    .ThenByDescending(x => x.NgayTao)
                    .ThenByDescending(x => x.MaSanPham)
                    .Take(8)
                    .ToList()
            };

            return View(model);
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