using Do_an.Models;
using System.Linq;
using System.Web.Mvc;

namespace Do_an.Controllers
{
    public class HomeController : Controller
    {
        private readonly SPORTIXEntities db = new SPORTIXEntities();

        public ActionResult Index()
        {
            var sanPham = db.SanPhams
                .Where(sp => sp.DangBan)
                .OrderByDescending(sp => sp.MaSanPham)
                .ToList();

            return View(sanPham);
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