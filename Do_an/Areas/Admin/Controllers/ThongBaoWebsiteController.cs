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
                    .OrderByDescending(x => x.NgayTao)
                    .ThenByDescending(x => x.MaThongBaoWebsite)
                    .ToList());
            }
        }

        [HttpGet]
        public ActionResult Them()
        {
            return View("Form", new ThongBaoWebsiteFormViewModel
            {
                HienThi = true
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Them(ThongBaoWebsiteFormViewModel vm)
        {
            if (!ModelState.IsValid)
                return View("Form", vm);

            int? maTaiKhoan = XacThucService.LayMaTaiKhoanHienTai();

            if (!maTaiKhoan.HasValue)
                return new HttpStatusCodeResult(401);

            using (var db = new SPORTIXEntities())
            {
                var thongBao = new ThongBaoWebsite
                {
                    MaNguoiTao = maTaiKhoan.Value,
                    NgayTao = DateTime.Now
                };

                GanNoiDung(thongBao, vm);
                db.ThongBaoWebsites.Add(thongBao);
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
                var thongBao = db.ThongBaoWebsites.AsNoTracking()
                    .FirstOrDefault(x => x.MaThongBaoWebsite == id);

                if (thongBao == null)
                    return HttpNotFound();

                return View("Form", new ThongBaoWebsiteFormViewModel
                {
                    MaThongBaoWebsite = thongBao.MaThongBaoWebsite,
                    TieuDe = thongBao.TieuDe,
                    NoiDung = thongBao.NoiDung,
                    HienThi = thongBao.HienThi,
                    NhanTrai = thongBao.NhanTrai,
                    GiaTriTrai = thongBao.GiaTriTrai,
                    MoTaTrai = thongBao.MoTaTrai,
                    NhanPhai = thongBao.NhanPhai,
                    GiaTriPhai = thongBao.GiaTriPhai,
                    MoTaPhai = thongBao.MoTaPhai
                });
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Sua(int id, ThongBaoWebsiteFormViewModel vm)
        {
            vm.MaThongBaoWebsite = id;

            if (!ModelState.IsValid)
                return View("Form", vm);

            using (var db = new SPORTIXEntities())
            {
                var thongBao = db.ThongBaoWebsites
                    .FirstOrDefault(x => x.MaThongBaoWebsite == id);

                if (thongBao == null)
                    return HttpNotFound();

                GanNoiDung(thongBao, vm);
                thongBao.NgayCapNhat = DateTime.Now;
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
                var thongBao = db.ThongBaoWebsites
                    .FirstOrDefault(x => x.MaThongBaoWebsite == id);

                if (thongBao == null)
                    return HttpNotFound();

                thongBao.HienThi = !thongBao.HienThi;
                thongBao.NgayCapNhat = DateTime.Now;
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Xoa(int id)
        {
            using (var db = new SPORTIXEntities())
            {
                var thongBao = db.ThongBaoWebsites
                    .FirstOrDefault(x => x.MaThongBaoWebsite == id);

                if (thongBao == null)
                    return HttpNotFound();

                db.ThongBaoWebsites.Remove(thongBao);
                db.SaveChanges();
            }

            TempData["ThanhCong"] = "Đã xóa thông báo.";
            return RedirectToAction("Index");
        }

        private static void GanNoiDung(
            ThongBaoWebsite thongBao,
            ThongBaoWebsiteFormViewModel vm)
        {
            thongBao.TieuDe = vm.TieuDe.Trim();
            thongBao.NoiDung = vm.NoiDung.Trim();
            thongBao.HienThi = vm.HienThi;

            thongBao.NhanTrai = ChuanHoa(vm.NhanTrai);
            thongBao.GiaTriTrai = ChuanHoa(vm.GiaTriTrai);
            thongBao.MoTaTrai = ChuanHoa(vm.MoTaTrai);

            thongBao.NhanPhai = ChuanHoa(vm.NhanPhai);
            thongBao.GiaTriPhai = ChuanHoa(vm.GiaTriPhai);
            thongBao.MoTaPhai = ChuanHoa(vm.MoTaPhai);
        }

        private static string ChuanHoa(string noiDung)
        {
            return string.IsNullOrWhiteSpace(noiDung)
                ? null
                : noiDung.Trim();
        }
    }
}