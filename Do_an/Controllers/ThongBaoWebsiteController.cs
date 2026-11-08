using System.Linq;
using System.Web.Mvc;
using Do_an.Models;

namespace Do_an.Controllers
{
    // Khách xem các thông báo website đang bật (không cần đăng nhập).
    public class ThongBaoWebsiteController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            using (var db = new SPORTIXEntities())
            {
                return View(db.ThongBaoWebsites.AsNoTracking()
                    .Where(x => x.HienThi)
                    .OrderByDescending(x => x.NgayTao)
                    .ToList());
            }
        }

        // Partial nhỏ để TV nào cần có thể nhúng lên trang chủ: @Html.Action("GanDay", "ThongBaoWebsite")
        [ChildActionOnly]
        public ActionResult GanDay()
        {
            using (var db = new SPORTIXEntities())
            {
                return PartialView(db.ThongBaoWebsites.AsNoTracking()
                    .Where(x => x.HienThi)
                    .OrderByDescending(x => x.NgayTao)
                    .Take(3)
                    .ToList());
            }
        }
    }
}
