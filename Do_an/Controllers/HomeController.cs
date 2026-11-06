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

            var model = new TrangChuViewModel
            {
                TongSanPham = query.Count(),

                TongLoaiHang = danhMuc.Count(),

                TongThuongHieu = query
                    .Select(x => x.MaThuongHieu)
                    .Distinct()
                    .Count(),

                MucGiamCaoNhat = query
                    .Select(x => (int?)x.PhanTramGiam)
                    .Max() ?? 0,

                DanhMuc = danhMuc
                    .OrderBy(x => x.TenLoaiHang)
                    .Take(4)
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