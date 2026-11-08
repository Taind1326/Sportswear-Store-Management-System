using System;
using System.Linq;
using System.Web.Mvc;
using Do_an.Models;
using Do_an.Services;
using Do_an.ViewModels;

namespace Do_an.Areas.Admin.Controllers
{
    [YeuCauAdmin]
    public class KhuyenMaiController : Controller
    {
        private const int KichThuocTrang = 10;

        [HttpGet]
        public ActionResult Index(string tuKhoa, int trang = 1)
        {
            tuKhoa = (tuKhoa ?? "").Trim();

            using (var db = new SPORTIXEntities())
            {
                IQueryable<MaGiamGia> q = db.MaGiamGias.AsNoTracking();

                if (tuKhoa.Length > 0)
                    q = q.Where(x => x.MaCode.Contains(tuKhoa) || x.TenKhuyenMai.Contains(tuKhoa));

                int tong = q.Count();
                int tongTrang = Math.Max(1, (int)Math.Ceiling(tong / (double)KichThuocTrang));
                trang = Math.Max(1, Math.Min(trang, tongTrang));

                ViewBag.TuKhoa = tuKhoa;
                ViewBag.Trang = trang;
                ViewBag.TongTrang = tongTrang;
                ViewBag.TongSo = tong;

                return View(q.OrderByDescending(x => x.NgayTao).ThenByDescending(x => x.MaGiamGia1)
                    .Skip((trang - 1) * KichThuocTrang).Take(KichThuocTrang).ToList());
            }
        }

        [HttpGet]
        public ActionResult Them()
        {
            return View("Form", new KhuyenMaiFormViewModel
            {
                LoaiGiam = LoaiGiamGia.PhanTram,
                NgayBatDau = DateTime.Today,
                NgayKetThuc = DateTime.Today.AddMonths(1),
                SoLuotToiDa = 100,
                HoatDong = true
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Them(KhuyenMaiFormViewModel vm)
        {
            Chuan(vm);

            using (var db = new SPORTIXEntities())
            {
                if (ModelState.IsValid && db.MaGiamGias.Any(x => x.MaCode == vm.MaCode))
                    ModelState.AddModelError("MaCode", "Mã giảm giá này đã tồn tại.");

                if (!ModelState.IsValid) return View("Form", vm);

                db.MaGiamGias.Add(new MaGiamGia
                {
                    MaCode = vm.MaCode,
                    TenKhuyenMai = vm.TenKhuyenMai.Trim(),
                    LoaiGiam = vm.LoaiGiam,
                    GiaTriGiam = vm.GiaTriGiam,
                    DonToiThieu = vm.DonToiThieu,
                    GiamToiDa = vm.GiamToiDa,
                    NgayBatDau = vm.NgayBatDau.Value,
                    NgayKetThuc = vm.NgayKetThuc.Value.Date.AddDays(1).AddSeconds(-1),
                    SoLuotToiDa = vm.SoLuotToiDa,
                    SoLuotDaDung = 0,
                    HoatDong = vm.HoatDong,
                    NgayTao = DateTime.Now
                });
                db.SaveChanges();
            }

            TempData["ThanhCong"] = "Đã tạo mã " + vm.MaCode + ".";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Sua(int id)
        {
            using (var db = new SPORTIXEntities())
            {
                MaGiamGia m = db.MaGiamGias.AsNoTracking().FirstOrDefault(x => x.MaGiamGia1 == id);
                if (m == null) return HttpNotFound();

                return View("Form", new KhuyenMaiFormViewModel
                {
                    MaGiamGia = m.MaGiamGia1,
                    MaCode = m.MaCode,
                    TenKhuyenMai = m.TenKhuyenMai,
                    LoaiGiam = m.LoaiGiam,
                    GiaTriGiam = m.GiaTriGiam,
                    DonToiThieu = m.DonToiThieu,
                    GiamToiDa = m.GiamToiDa,
                    NgayBatDau = m.NgayBatDau,
                    NgayKetThuc = m.NgayKetThuc,
                    SoLuotToiDa = m.SoLuotToiDa,
                    SoLuotDaDung = m.SoLuotDaDung,
                    HoatDong = m.HoatDong
                });
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Sua(int id, KhuyenMaiFormViewModel vm)
        {
            vm.MaGiamGia = id;
            Chuan(vm);

            using (var db = new SPORTIXEntities())
            {
                MaGiamGia m = db.MaGiamGias.FirstOrDefault(x => x.MaGiamGia1 == id);
                if (m == null) return HttpNotFound();

                // Mã code và lượt đã dùng không sửa được từ form (đơn cũ đang tham chiếu).
                vm.MaCode = m.MaCode;
                vm.SoLuotDaDung = m.SoLuotDaDung;
                ModelState.Remove("MaCode");
                ModelState.Remove("SoLuotDaDung");

                ModelState.Clear();
                TryValidateModel(vm);

                if (!ModelState.IsValid) return View("Form", vm);

                m.TenKhuyenMai = vm.TenKhuyenMai.Trim();
                m.LoaiGiam = vm.LoaiGiam;
                m.GiaTriGiam = vm.GiaTriGiam;
                m.DonToiThieu = vm.DonToiThieu;
                m.GiamToiDa = vm.GiamToiDa;
                m.NgayBatDau = vm.NgayBatDau.Value;
                m.NgayKetThuc = vm.NgayKetThuc.Value.TimeOfDay == TimeSpan.Zero
                    ? vm.NgayKetThuc.Value.Date.AddDays(1).AddSeconds(-1)
                    : vm.NgayKetThuc.Value;
                m.SoLuotToiDa = vm.SoLuotToiDa;
                m.HoatDong = vm.HoatDong;
                db.SaveChanges();
            }

            TempData["ThanhCong"] = "Đã cập nhật mã " + vm.MaCode + ".";
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult BatTat(int id)
        {
            using (var db = new SPORTIXEntities())
            {
                MaGiamGia m = db.MaGiamGias.FirstOrDefault(x => x.MaGiamGia1 == id);
                if (m == null) return HttpNotFound();

                m.HoatDong = !m.HoatDong;
                db.SaveChanges();

                TempData["ThanhCong"] = "Mã " + m.MaCode + (m.HoatDong ? " đã được bật." : " đã được tắt.");
            }

            return RedirectToAction("Index");
        }

        private static void Chuan(KhuyenMaiFormViewModel vm)
        {
            vm.MaCode = (vm.MaCode ?? "").Trim().ToUpperInvariant();
            if (vm.LoaiGiam == LoaiGiamGia.SoTien) vm.GiamToiDa = null;
        }
    }
}
