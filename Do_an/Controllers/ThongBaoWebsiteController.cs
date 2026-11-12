using System;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using Do_an.Models;
using Do_an.Services;
using Do_an.ViewModels;

namespace Do_an.Controllers
{
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
                    .ThenByDescending(x => x.MaThongBaoWebsite)
                    .ToList());
            }
        }

        [ChildActionOnly]
        public ActionResult GanDay()
        {
            using (var db = new SPORTIXEntities())
            {
                var thongBao = db.ThongBaoWebsites.AsNoTracking()
                    .Where(x => x.HienThi)
                    .OrderByDescending(x => x.NgayTao)
                    .ThenByDescending(x => x.MaThongBaoWebsite)
                    .FirstOrDefault();

                var vm = new ThongBaoWebsitePopupViewModel();

                if (thongBao == null)
                    return PartialView(vm);

                var taiKhoan = XacThucService.LayTaiKhoanHienTai(db);

                if (taiKhoan != null)
                {
                    int maTaiKhoan = taiKhoan.MaTaiKhoan;
                    int maThongBao = thongBao.MaThongBaoWebsite;

                    bool daXem = db.Set<LuotXemThongBaoWebsite>()
                        .Any(x => x.MaTaiKhoan == maTaiKhoan
                            && x.MaThongBaoWebsite == maThongBao);

                    if (daXem)
                        return PartialView(vm);

                    vm.MaTaiKhoan = maTaiKhoan;
                }

                vm.ThongBao = thongBao;
                return PartialView(vm);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult DaXem(int id)
        {
            using (var db = new SPORTIXEntities())
            {
                var taiKhoan = XacThucService.LayTaiKhoanHienTai(db);

                if (taiKhoan == null)
                    return new HttpStatusCodeResult(401);

                bool duocHienThi = db.ThongBaoWebsites
                    .Any(x => x.MaThongBaoWebsite == id && x.HienThi);

                if (!duocHienThi)
                    return HttpNotFound();

                int maTaiKhoan = taiKhoan.MaTaiKhoan;
                var luotXem = db.Set<LuotXemThongBaoWebsite>();

                bool daXem = luotXem.Any(
                    x => x.MaTaiKhoan == maTaiKhoan
                        && x.MaThongBaoWebsite == id);

                if (daXem)
                    return Json(new { thanhCong = true });

                luotXem.Add(new LuotXemThongBaoWebsite
                {
                    MaTaiKhoan = maTaiKhoan,
                    MaThongBaoWebsite = id,
                    NgayXem = DateTime.Now
                });

                try
                {
                    db.SaveChanges();
                }
                catch (DbUpdateException ex)
                {
                    // Hai tab có thể cùng ghi nhận một thông báo.
                    // Khóa chính bảo đảm không tạo lượt xem trùng.
                    var loiSql = ex.GetBaseException() as SqlException;

                    if (loiSql == null
                        || (loiSql.Number != 2601 && loiSql.Number != 2627))
                    {
                        throw;
                    }
                }

                return Json(new { thanhCong = true });
            }
        }
    }
}