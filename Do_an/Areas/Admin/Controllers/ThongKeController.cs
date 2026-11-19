using System;
using System.Globalization;
using System.Web.Mvc;
using Do_an.Models;
using Do_an.Services;
using Do_an.ViewModels;

namespace Do_an.Areas.Admin.Controllers
{
    [YeuCauAdmin]
    public class ThongKeController : Controller
    {
        [HttpGet]
        public ActionResult Index(string tuNgay, string denNgay, string thang)
        {
            var homNay = DateTime.Today;
            var ngayDauThang = new DateTime(homNay.Year, homNay.Month, 1);
            var model = new ThongKeViewModel
            {
                TuNgay = tuNgay ?? ngayDauThang.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                DenNgay = denNgay ?? homNay.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                Thang = thang ?? homNay.ToString("yyyy-MM", CultureInfo.InvariantCulture)
            };

            DateTime batDau;
            DateTime ketThuc;
            if (thang != null)
            {
                if (!DateTime.TryParseExact(thang, "yyyy-MM", CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out batDau) ||
                    (batDau.Year == 9999 && batDau.Month == 12))
                {
                    ModelState.AddModelError("", "Vui lòng chọn tháng và năm hợp lệ.");
                    return View(model);
                }

                ketThuc = batDau.AddMonths(1).AddDays(-1);
                model.TuNgay = batDau.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                model.DenNgay = ketThuc.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            }
            else if (!DateTime.TryParseExact(model.TuNgay, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out batDau) ||
                !DateTime.TryParseExact(model.DenNgay, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out ketThuc) || ketThuc == DateTime.MaxValue.Date)
            {
                ModelState.AddModelError("", "Vui lòng nhập ngày bắt đầu và ngày kết thúc hợp lệ.");
                return View(model);
            }

            if (batDau > ketThuc || (ketThuc - batDau).Days >= 366)
            {
                ModelState.AddModelError("", "Chọn khoảng thời gian từ 1 đến 366 ngày, theo thứ tự từ ngày đến ngày.");
                return View(model);
            }

            using (var db = new SPORTIXEntities())
            {
                new ThongKeService().TongHop(db, batDau, ketThuc.AddDays(1), model);
            }

            return View(model);
        }
    }
}
