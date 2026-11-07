using System;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web.Mvc;
using Do_an.Models;
using Do_an.Services;
using Do_an.ViewModels;

namespace Do_an.Controllers
{
    public class TaiKhoanController : Controller
    {
        private const int ToiDaDiaChi = 10;

        private readonly SPORTIXEntities db = new SPORTIXEntities();

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }

        public ActionResult Index()
        {
            return RedirectToAction("HoSo");
        }

        // ===================== ĐĂNG KÝ =====================

        [HttpGet]
        public ActionResult DangKy(string returnUrl)
        {
            TaiKhoan daDangNhap = XacThucService.LayTaiKhoanHienTai(db);
            if (daDangNhap != null)
                return ChuyenSauDangNhap(returnUrl, daDangNhap.VaiTro);

            return View(new DangKyViewModel { ReturnUrl = LamSachReturnUrl(returnUrl) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DangKy(DangKyViewModel model)
        {
            model.ReturnUrl = LamSachReturnUrl(model.ReturnUrl);

            string tenDangNhap = (model.TenDangNhap ?? "").Trim();
            string email = (model.Email ?? "").Trim();

            if (ModelState.IsValid)
            {
                if (db.TaiKhoans.Any(x => x.TenDangNhap == tenDangNhap))
                    ModelState.AddModelError("TenDangNhap", "Tên đăng nhập đã được sử dụng.");

                if (db.TaiKhoans.Any(x => x.Email == email))
                    ModelState.AddModelError("Email", "Email đã được sử dụng.");
            }

            if (!ModelState.IsValid) return View(model);

            var tk = new TaiKhoan
            {
                TenDangNhap = tenDangNhap,
                MatKhauHash = MatKhauHelper.BamMatKhau(model.MatKhau),
                HoTen = model.HoTen.Trim(),
                Email = email,
                SoDienThoai = string.IsNullOrWhiteSpace(model.SoDienThoai)
                    ? null
                    : model.SoDienThoai.Trim(),
                VaiTro = XacThucService.VaiTroUser, // luôn là User, không nhận từ form
                HoatDong = true,
                NgayTao = DateTime.Now
            };

            db.TaiKhoans.Add(tk);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("",
                    "Không thể tạo tài khoản. Tên đăng nhập hoặc email có thể đã được sử dụng, vui lòng thử lại.");
                return View(model);
            }

            XacThucService.DangNhap(tk);
            TempData["ThanhCong"] = "Đăng ký thành công. Chào mừng " + tk.HoTen + " đến với SPORTIX!";

            return ChuyenSauDangNhap(model.ReturnUrl, tk.VaiTro);
        }

        // ===================== ĐĂNG NHẬP / ĐĂNG XUẤT =====================

        [HttpGet]
        public ActionResult DangNhap(string returnUrl)
        {
            TaiKhoan daDangNhap = XacThucService.LayTaiKhoanHienTai(db);
            if (daDangNhap != null)
                return ChuyenSauDangNhap(returnUrl, daDangNhap.VaiTro);

            return View(new DangNhapViewModel { ReturnUrl = LamSachReturnUrl(returnUrl) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DangNhap(DangNhapViewModel model)
        {
            model.ReturnUrl = LamSachReturnUrl(model.ReturnUrl);

            if (!ModelState.IsValid) return View(model);

            string dinhDanh = model.TenDangNhap.Trim();

            string khoa = "dangnhap:" + Request.UserHostAddress + ":" + dinhDanh.ToLowerInvariant();
            if (!GioiHanTanSuat.ChoPhep(khoa, 10, TimeSpan.FromMinutes(5)))
            {
                ModelState.AddModelError("",
                    "Bạn thử đăng nhập quá nhiều lần. Vui lòng thử lại sau vài phút.");
                return View(model);
            }

            TaiKhoan tk = db.TaiKhoans.FirstOrDefault(
                x => x.TenDangNhap == dinhDanh || x.Email == dinhDanh);

            if (tk == null || !MatKhauHelper.KiemTra(model.MatKhau, tk.MatKhauHash))
            {
                ModelState.AddModelError("", "Tên đăng nhập/email hoặc mật khẩu không đúng.");
                return View(model);
            }

            if (!tk.HoatDong)
            {
                ModelState.AddModelError("",
                    "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ cửa hàng.");
                return View(model);
            }

            XacThucService.DangNhap(tk);

            return ChuyenSauDangNhap(model.ReturnUrl, tk.VaiTro);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DangXuat()
        {
            XacThucService.DangXuat();
            return RedirectToAction("Index", "Home", new { area = "" });
        }

        // ===================== HỒ SƠ =====================

        [HttpGet]
        [YeuCauDangNhap]
        public ActionResult HoSo()
        {
            int ma = XacThucService.LayMaTaiKhoanHienTai().Value;

            TaiKhoan tk = db.TaiKhoans.AsNoTracking().First(x => x.MaTaiKhoan == ma);

            return View(new HoSoViewModel
            {
                TenDangNhap = tk.TenDangNhap,
                HoTen = tk.HoTen,
                Email = tk.Email,
                SoDienThoai = tk.SoDienThoai
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [YeuCauDangNhap]
        public ActionResult HoSo(HoSoViewModel model)
        {
            int ma = XacThucService.LayMaTaiKhoanHienTai().Value;

            TaiKhoan tk = db.TaiKhoans.First(x => x.MaTaiKhoan == ma);
            model.TenDangNhap = tk.TenDangNhap;

            string email = (model.Email ?? "").Trim();

            if (ModelState.IsValid &&
                db.TaiKhoans.Any(x => x.Email == email && x.MaTaiKhoan != ma))
            {
                ModelState.AddModelError("Email", "Email đã được sử dụng bởi tài khoản khác.");
            }

            if (!ModelState.IsValid) return View(model);

            tk.HoTen = model.HoTen.Trim();
            tk.Email = email;
            tk.SoDienThoai = string.IsNullOrWhiteSpace(model.SoDienThoai)
                ? null
                : model.SoDienThoai.Trim();

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("Email", "Không thể lưu. Email có thể đã được sử dụng.");
                return View(model);
            }

            XacThucService.CapNhatHoTen(tk.HoTen);
            TempData["ThanhCong"] = "Đã cập nhật hồ sơ.";

            return RedirectToAction("HoSo");
        }

        // ===================== ĐỔI MẬT KHẨU =====================

        [HttpGet]
        [YeuCauDangNhap]
        public ActionResult DoiMatKhau()
        {
            return View(new DoiMatKhauViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [YeuCauDangNhap]
        public ActionResult DoiMatKhau(DoiMatKhauViewModel model)
        {
            int ma = XacThucService.LayMaTaiKhoanHienTai().Value;

            if (!ModelState.IsValid) return View(model);

            if (!GioiHanTanSuat.ChoPhep("doimk:" + ma, 10, TimeSpan.FromMinutes(15)))
            {
                ModelState.AddModelError("",
                    "Bạn thao tác quá nhiều lần. Vui lòng thử lại sau ít phút.");
                return View(model);
            }

            TaiKhoan tk = db.TaiKhoans.First(x => x.MaTaiKhoan == ma);

            if (!MatKhauHelper.KiemTra(model.MatKhauCu, tk.MatKhauHash))
            {
                ModelState.AddModelError("MatKhauCu", "Mật khẩu hiện tại không đúng.");
                return View(model);
            }

            if (model.MatKhauCu == model.MatKhauMoi)
            {
                ModelState.AddModelError("MatKhauMoi", "Mật khẩu mới phải khác mật khẩu hiện tại.");
                return View(model);
            }

            tk.MatKhauHash = MatKhauHelper.BamMatKhau(model.MatKhauMoi);
            db.SaveChanges();

            TempData["ThanhCong"] = "Đổi mật khẩu thành công.";
            return RedirectToAction("HoSo");
        }

        // ===================== ĐỊA CHỈ =====================

        // Dùng ActionName để URL là /TaiKhoan/DiaChi nhưng tránh trùng tên với class DiaChi.
        [HttpGet]
        [ActionName("DiaChi")]
        [YeuCauDangNhap]
        public ActionResult DanhSachDiaChi(int? sua)
        {
            int ma = XacThucService.LayMaTaiKhoanHienTai().Value;

            var form = new DiaChiViewModel();

            if (sua.HasValue)
            {
                int maSua = sua.Value;
                DiaChi dc = db.DiaChis.AsNoTracking()
                    .FirstOrDefault(x => x.MaDiaChi == maSua && x.MaTaiKhoan == ma);

                if (dc == null) return HttpNotFound();

                form = new DiaChiViewModel
                {
                    MaDiaChi = dc.MaDiaChi,
                    HoTenNguoiNhan = dc.HoTenNguoiNhan,
                    SoDienThoai = dc.SoDienThoai,
                    TinhThanh = dc.TinhThanh,
                    PhuongXa = dc.PhuongXa,
                    DiaChiCuThe = dc.DiaChiCuThe,
                    MacDinh = dc.MacDinh
                };
            }

            NapDanhSachDiaChi(ma);
            return View("DiaChi", form);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [YeuCauDangNhap]
        public ActionResult LuuDiaChi(DiaChiViewModel model)
        {
            int ma = XacThucService.LayMaTaiKhoanHienTai().Value;

            if (!ModelState.IsValid)
            {
                NapDanhSachDiaChi(ma);
                return View("DiaChi", model);
            }

            DiaChi dc;
            bool laMoi = model.MaDiaChi == 0;

            if (laMoi)
            {
                int soDiaChi = db.DiaChis.Count(x => x.MaTaiKhoan == ma);

                if (soDiaChi >= ToiDaDiaChi)
                {
                    ModelState.AddModelError("",
                        "Bạn chỉ có thể lưu tối đa " + ToiDaDiaChi + " địa chỉ.");
                    NapDanhSachDiaChi(ma);
                    return View("DiaChi", model);
                }

                dc = new DiaChi { MaTaiKhoan = ma };

                if (soDiaChi == 0) model.MacDinh = true; // địa chỉ đầu tiên là mặc định
            }
            else
            {
                int maSua = model.MaDiaChi;
                dc = db.DiaChis.FirstOrDefault(x => x.MaDiaChi == maSua && x.MaTaiKhoan == ma);

                if (dc == null) return HttpNotFound(); // địa chỉ của người khác -> 404
            }

            dc.HoTenNguoiNhan = model.HoTenNguoiNhan.Trim();
            dc.SoDienThoai = model.SoDienThoai.Trim();
            dc.TinhThanh = model.TinhThanh.Trim();
            dc.PhuongXa = model.PhuongXa.Trim();
            dc.DiaChiCuThe = model.DiaChiCuThe.Trim();
            dc.MacDinh = model.MacDinh;

            if (model.MacDinh)
            {
                int maHienTai = dc.MaDiaChi;
                var khac = db.DiaChis
                    .Where(x => x.MaTaiKhoan == ma && x.MacDinh && x.MaDiaChi != maHienTai)
                    .ToList();

                foreach (var d in khac) d.MacDinh = false;
            }

            if (laMoi) db.DiaChis.Add(dc);

            db.SaveChanges(); // một lần SaveChanges = một transaction

            TempData["ThanhCong"] = laMoi ? "Đã thêm địa chỉ mới." : "Đã cập nhật địa chỉ.";
            return RedirectToAction("DiaChi");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [YeuCauDangNhap]
        public ActionResult XoaDiaChi(int id)
        {
            int ma = XacThucService.LayMaTaiKhoanHienTai().Value;

            DiaChi dc = db.DiaChis.FirstOrDefault(x => x.MaDiaChi == id && x.MaTaiKhoan == ma);
            if (dc == null) return HttpNotFound();

            db.DiaChis.Remove(dc);
            db.SaveChanges();

            TempData["ThanhCong"] = "Đã xóa địa chỉ.";
            return RedirectToAction("DiaChi");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [YeuCauDangNhap]
        public ActionResult DatMacDinh(int id)
        {
            int ma = XacThucService.LayMaTaiKhoanHienTai().Value;

            var cacDiaChi = db.DiaChis.Where(x => x.MaTaiKhoan == ma).ToList();
            DiaChi chon = cacDiaChi.FirstOrDefault(x => x.MaDiaChi == id);

            if (chon == null) return HttpNotFound();

            foreach (var d in cacDiaChi) d.MacDinh = (d.MaDiaChi == id);

            db.SaveChanges();

            TempData["ThanhCong"] = "Đã đặt làm địa chỉ mặc định.";
            return RedirectToAction("DiaChi");
        }

        // ===================== HÀM PHỤ =====================

        private void NapDanhSachDiaChi(int ma)
        {
            ViewBag.DanhSach = db.DiaChis.AsNoTracking()
                .Where(x => x.MaTaiKhoan == ma)
                .OrderByDescending(x => x.MacDinh)
                .ThenByDescending(x => x.MaDiaChi)
                .ToList();
        }

        // Chỉ chấp nhận đường dẫn nội bộ để tránh open redirect.
        private string LamSachReturnUrl(string returnUrl)
        {
            if (string.IsNullOrWhiteSpace(returnUrl) || returnUrl.Length > 2000)
                return null;

            return Url.IsLocalUrl(returnUrl) ? returnUrl : null;
        }

        private ActionResult ChuyenSauDangNhap(string returnUrl, string vaiTro)
        {
            string url = LamSachReturnUrl(returnUrl);
            if (url != null) return Redirect(url);

            if (vaiTro == XacThucService.VaiTroAdmin)
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });

            return RedirectToAction("Index", "Home", new { area = "" });
        }
    }
}