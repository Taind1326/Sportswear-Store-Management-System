using System;
using System.Linq;
using System.Web.Mvc;
using Do_an.Models;
using Do_an.Services;
using Do_an.ViewModels;

namespace Do_an.Areas.Admin.Controllers
{
    [YeuCauAdmin]
    public class ThongBaoWebsiteController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            using (var db = new SPORTIXEntities())
            {
                return View(db.ThongBaoWebsites.AsNoTracking()
                    .OrderByDescending(x => x.NgayTao).ThenByDescending(x => x.MaThongBaoWebsite)
                    .ToList());
            }
        }

        [HttpGet]
        public ActionResult Them()
        {
            return View("Form", new ThongBaoWebsiteFormViewModel { HienThi = true });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Them(ThongBaoWebsiteFormViewModel vm)
        {
            if (!ModelState.IsValid) return View("Form", vm);

            using (var db = new SPORTIXEntities())
            {
                db.ThongBaoWebsites.Add(new ThongBaoWebsite
                {
                    TieuDe = vm.TieuDe.Trim(),
                    NoiDung = vm.NoiDung.Trim(),
                    HienThi = vm.HienThi,
                    MaNguoiTao = XacThucService.LayMaTaiKhoanHienTai().Value,
                    NgayTao = DateTime.Now
                });
                db.SaveChanges();
            }

            TempData["ThanhCong"] = "Đã tạo thông báo.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Sua(int id)
        {
            using (var db = new SPORTIXEntities())
            {
                var t = db.ThongBaoWebsites.AsNoTracking().FirstOrDefault(x => x.MaThongBaoWebsite == id);
                if (t == null) return HttpNotFound();

                return View("Form", new ThongBaoWebsiteFormViewModel
                {
                    MaThongBaoWebsite = t.MaThongBaoWebsite,
                    TieuDe = t.TieuDe,
                    NoiDung = t.NoiDung,
                    HienThi = t.HienThi
                });
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Sua(int id, ThongBaoWebsiteFormViewModel vm)
        {
            vm.MaThongBaoWebsite = id;
            if (!ModelState.IsValid) return View("Form", vm);

            using (var db = new SPORTIXEntities())
            {
                var t = db.ThongBaoWebsites.FirstOrDefault(x => x.MaThongBaoWebsite == id);
                if (t == null) return HttpNotFound();

                t.TieuDe = vm.TieuDe.Trim();
                t.NoiDung = vm.NoiDung.Trim();
                t.HienThi = vm.HienThi;
                t.NgayCapNhat = DateTime.Now;
                db.SaveChanges();
            }

            TempData["ThanhCong"] = "Đã cập nhật thông báo.";
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult BatTat(int id)
        {
            using (var db = new SPORTIXEntities())
            {
                var t = db.ThongBaoWebsites.FirstOrDefault(x => x.MaThongBaoWebsite == id);
                if (t == null) return HttpNotFound();

                t.HienThi = !t.HienThi;
                t.NgayCapNhat = DateTime.Now;
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Xoa(int id)
        {
            using (var db = new SPORTIXEntities())
            {
                var t = db.ThongBaoWebsites.FirstOrDefault(x => x.MaThongBaoWebsite == id);
                if (t != null)
                {
                    db.ThongBaoWebsites.Remove(t);
                    db.SaveChanges();
                    TempData["ThanhCong"] = "Đã xóa thông báo.";
                }
            }

            return RedirectToAction("Index");
        }
    }
}
