using Do_an.Models;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Do_an.Controllers
{
    public class SanPhamController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            using (var db = new SPORTIXEntities())
            {
                var sanPhams = db.SanPhams
                    .AsNoTracking()
                    .Include(x => x.HinhAnhSanPhams)
                    .Where(x => x.DangBan)
                    .OrderByDescending(x => x.NgayTao)
                    .ThenByDescending(x => x.MaSanPham)
                    .ToList();

                return View(sanPhams);
            }
        }

        [HttpGet]
        public ActionResult ChiTiet(int? id)
        {
            if (!id.HasValue)
            {
                return HttpNotFound("Thiếu mã sản phẩm.");
            }

            using (var db = new SPORTIXEntities())
            {
                var sanPham = db.SanPhams
                    .AsNoTracking()
                    .Include(x => x.HinhAnhSanPhams)
                    .Include(x => x.BienTheSanPhams)
                    .SingleOrDefault(x => x.MaSanPham == id.Value && x.DangBan);

                if (sanPham == null)
                {
                    return HttpNotFound("Không tìm thấy sản phẩm đang bán.");
                }

                return View(sanPham);
            }
        }
    }
}