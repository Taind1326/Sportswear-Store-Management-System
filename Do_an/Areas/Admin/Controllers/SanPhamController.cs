using Do_an.Models;
using Do_an.Services;
using Do_an.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Diagnostics;
using System.IO;
using System.Web;

namespace Do_an.Areas.Admin.Controllers
{
    public class SanPhamController : Controller
    {
        private readonly SPORTIXEntities db = new SPORTIXEntities();

        [HttpGet]
        public ActionResult Index(string tuKhoa, int trang = 1)
        {
            const int kichThuocTrang = 10;

            tuKhoa = (tuKhoa ?? "").Trim();

            IQueryable<SanPham> query = db.SanPhams.AsNoTracking();

            if (!string.IsNullOrEmpty(tuKhoa))
            {
                query = query.Where(x =>
                    x.TenSanPham.Contains(tuKhoa) ||
                    x.BienTheSanPhams.Any(bt =>
                        bt.SKU.Contains(tuKhoa)));
            }

            int tongSanPham = query.Count();

            int tongTrang = Math.Max(
                1,
                (int)Math.Ceiling(
                    tongSanPham / (double)kichThuocTrang));

            trang = Math.Max(1, Math.Min(trang, tongTrang));

            var danhSach = query
                .Include(x => x.LoaiHang)
                .Include(x => x.ThuongHieu)
                .Include(x => x.BienTheSanPhams)
                .Include(x => x.HinhAnhSanPhams)
                .OrderByDescending(x => x.NgayTao)
                .ThenByDescending(x => x.MaSanPham)
                .Skip((trang - 1) * kichThuocTrang)
                .Take(kichThuocTrang)
                .ToList();

            ViewBag.TuKhoa = tuKhoa;
            ViewBag.Trang = trang;
            ViewBag.TongTrang = tongTrang;
            ViewBag.TongSanPham = tongSanPham;

            return View(danhSach);
        }

        [HttpGet]
        public ActionResult ChiTiet(int? id)
        {
            if (!id.HasValue || id.Value <= 0)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest,
                    "Mã sản phẩm không hợp lệ.");
            }

            var sanPham = db.SanPhams
                .AsNoTracking()
                .Include(x => x.LoaiHang)
                .Include(x => x.ThuongHieu)
                .Include(x => x.HinhAnhSanPhams)
                .Include(x => x.BienTheSanPhams)
                .SingleOrDefault(x => x.MaSanPham == id.Value);

            if (sanPham == null)
            {
                return HttpNotFound("Không tìm thấy sản phẩm.");
            }

            return View(sanPham);
        }

        [HttpGet]
        public ActionResult Them()
        {
            NapDanhMuc();

            var model = new ThemSanPhamViewModel();

            model.PhanLoai = new List<PhanLoaiSanPhamInput>
            {
                new PhanLoaiSanPhamInput
                {
                    SoLuongTon = 0
                }
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Them(ThemSanPhamViewModel model)
        {
            model.TenSanPham = (model.TenSanPham ?? "").Trim();
            model.MoTa = (model.MoTa ?? "").Trim();

            if (string.IsNullOrWhiteSpace(model.TenSanPham))
            {
                DatLoiTruong("TenSanPham", "Vui lòng nhập tên sản phẩm.");
            }

            if (!db.LoaiHangs.Any(x =>
                x.MaLoaiHang == model.MaLoaiHang && x.HoatDong))
            {
                DatLoiTruong("MaLoaiHang", "Vui lòng chọn loại hàng đang hoạt động.");
            }

            if (!db.ThuongHieux.Any(x =>
                x.MaThuongHieu == model.MaThuongHieu && x.HoatDong))
            {
                DatLoiTruong("MaThuongHieu", "Vui lòng chọn thương hiệu đang hoạt động.");
            }

            model.PhanLoai = model.PhanLoai ?? new List<PhanLoaiSanPhamInput>();

            if (model.PhanLoai.Count == 0 || model.PhanLoai.Count > 100)
            {
                ModelState.AddModelError(
                    "",
                    "Sản phẩm phải có từ 1 đến 100 dòng phân loại.");

                NapDanhMuc(model.MaLoaiHang, model.MaThuongHieu);
                return View(model);
            }

            var toHopDaCo = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var mauPhanLoai = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < model.PhanLoai.Count; i++)
            {
                var dong = model.PhanLoai[i];

                if (dong == null)
                {
                    ModelState.AddModelError(
                        "",
                        "Dòng phân loại " + (i + 1) + " không hợp lệ.");

                    continue;
                }

                if (!string.IsNullOrWhiteSpace(dong.MauSac))
                {
                    mauPhanLoai.Add(QuyTacSanPham.ChuanHoa(dong.MauSac));
                }

                dong.MauSac = QuyTacSanPham.ChuanHoa(dong.MauSac);
                dong.KichThuoc = QuyTacSanPham.ChuanHoa(dong.KichThuoc);

                string tenNhom = "PhanLoai[" + i + "]";
                string khoa = QuyTacSanPham.Khoa(dong.MauSac, dong.KichThuoc);

                if (!toHopDaCo.Add(khoa))
                {
                    ModelState.AddModelError(
                        tenNhom + ".KichThuoc",
                        "Màu và kích cỡ bị trùng với dòng trước.");
                }

                KiemTraGia(dong.GiaBan, tenNhom + ".GiaBan");

                KiemTraSoLuong(
                    dong.SoLuongTon,
                    tenNhom + ".SoLuongTon",
                    0,
                    "Vui lòng nhập số lượng tồn.",
                    "Số lượng tồn phải là số nguyên hợp lệ.",
                    "Số lượng tồn không được nhỏ hơn 0.");
            }

            var anhTheoMau = LayAnhTheoMau(model.AnhTheoMau, mauPhanLoai);

            if (!ModelState.IsValid)
            {
                NapDanhMuc(model.MaLoaiHang, model.MaThuongHieu);
                return View(model);
            }

            var sanPham = new SanPham
            {
                TenSanPham = model.TenSanPham,
                MaLoaiHang = model.MaLoaiHang,
                MaThuongHieu = model.MaThuongHieu,
                MoTa = model.MoTa,
                NoiBat = model.NoiBat,
                DangBan = true,
                PhanTramGiam = 0,
                NgayTao = DateTime.Now
            };

            foreach (var dong in model.PhanLoai)
            {
                sanPham.BienTheSanPhams.Add(new BienTheSanPham
                {
                    SKU = TaoSku(),
                    MauSac = dong.MauSac,
                    KichThuoc = dong.KichThuoc,
                    GiaBan = dong.GiaBan.Value,
                    SoLuongTon = dong.SoLuongTon.Value,
                    DangBan = true
                });
            }

            if (!LuuSanPhamKemAnh(sanPham, model.TepAnhDaiDien, anhTheoMau))
            {
                NapDanhMuc(model.MaLoaiHang, model.MaThuongHieu);
                return View(model);
            }

            TempData["ThanhCong"] = "Thêm sản phẩm thành công.";

            return RedirectToAction("ChiTiet", new { id = sanPham.MaSanPham });
        }

        private Dictionary<string, HttpPostedFileBase> LayAnhTheoMau(
    List<AnhTheoMauInput> danhSachAnh,
    HashSet<string> mauPhanLoai)
        {
            var ketQua = new Dictionary<string, HttpPostedFileBase>(
                StringComparer.OrdinalIgnoreCase);

            foreach (var anh in danhSachAnh ?? new List<AnhTheoMauInput>())
            {
                if (anh == null || anh.TepAnh == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(anh.MauSac))
                {
                    ModelState.AddModelError("", "Vui lòng chọn màu cho ảnh đã tải lên.");
                    continue;
                }

                string mau = QuyTacSanPham.ChuanHoa(anh.MauSac);

                if (!mauPhanLoai.Contains(mau))
                {
                    ModelState.AddModelError(
                        "",
                        "Màu " + mau + " chưa có trong bảng phân loại.");

                    continue;
                }

                if (ketQua.ContainsKey(mau))
                {
                    ModelState.AddModelError(
                        "",
                        "Màu " + mau + " chỉ được chọn một ảnh.");

                    continue;
                }

                ketQua.Add(mau, anh.TepAnh);
            }

            return ketQua;
        }

        private bool LuuSanPhamKemAnh(
            SanPham sanPham,
            HttpPostedFileBase tepAnhDaiDien,
            Dictionary<string, HttpPostedFileBase> anhTheoMau)
        {
            const string thuMucAnh = "~/Content/images/san-pham/";
            string thuMucVatLy = Server.MapPath(thuMucAnh);

            var dichVuAnh = new HinhAnhSanPhamService();
            var cacTepDaLuu = new List<string>();
            string nhanAnh = "Ảnh đại diện";
            bool daLuuDb = false;

            try
            {
                string tenTepDaiDien = dichVuAnh.LuuAnh(tepAnhDaiDien, thuMucVatLy);
                cacTepDaLuu.Add(tenTepDaiDien);

                sanPham.HinhAnhSanPhams.Add(new HinhAnhSanPham
                {
                    DuongDanAnh = thuMucAnh + tenTepDaiDien,
                    AnhDaiDien = true,
                    ThuTu = 0
                });

                int thuTu = 1;

                foreach (var anh in anhTheoMau)
                {
                    nhanAnh = "Ảnh màu " + anh.Key;
                    string tenTep = dichVuAnh.LuuAnh(anh.Value, thuMucVatLy);
                    cacTepDaLuu.Add(tenTep);

                    sanPham.HinhAnhSanPhams.Add(new HinhAnhSanPham
                    {
                        DuongDanAnh = thuMucAnh + tenTep,
                        MauSac = anh.Key,
                        AnhDaiDien = false,
                        ThuTu = thuTu++
                    });
                }

                db.SanPhams.Add(sanPham);
                db.SaveChanges();
                daLuuDb = true;
                return true;
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidDataException)
            {
                ModelState.AddModelError("", nhanAnh + ". " + ex.Message);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                Trace.TraceError("Lỗi ghi ảnh sản phẩm: {0}", ex);
                ModelState.AddModelError("", "Chưa ghi được ảnh sản phẩm. Vui lòng thử lại.");
            }
            catch (DbUpdateException ex)
            {
                Trace.TraceError("Lỗi lưu sản phẩm: {0}", ex);
                ModelState.AddModelError("", "Chưa lưu được sản phẩm. Vui lòng thử lại.");
            }
            finally
            {
                if (!daLuuDb)
                {
                    foreach (string tenTep in cacTepDaLuu)
                    {
                        try
                        {
                            System.IO.File.Delete(Path.Combine(thuMucVatLy, tenTep));
                        }
                        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                        {
                            Trace.TraceError("Lỗi dọn ảnh sản phẩm {0}: {1}", tenTep, ex);
                        }
                    }
                }
            }

            return false;
        }

        [HttpGet]
        public ActionResult Sua(int? id)
        {
            if (!id.HasValue || id.Value <= 0)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest,
                    "Mã sản phẩm không hợp lệ.");
            }

            var sanPham = db.SanPhams
                .AsNoTracking()
                .SingleOrDefault(x => x.MaSanPham == id.Value);

            if (sanPham == null)
            {
                return HttpNotFound("Không tìm thấy sản phẩm.");
            }

            var model = new SuaSanPhamViewModel
            {
                MaSanPham = sanPham.MaSanPham,
                TenSanPham = sanPham.TenSanPham,
                MaLoaiHang = sanPham.MaLoaiHang,
                MaThuongHieu = sanPham.MaThuongHieu,
                MoTa = sanPham.MoTa,
                PhanTramGiam = sanPham.PhanTramGiam,
                NoiBat = sanPham.NoiBat,
                DangBan = sanPham.DangBan,
                DuongDanAnhDaiDien = db.HinhAnhSanPhams
                    .Where(x => x.MaSanPham == sanPham.MaSanPham && x.AnhDaiDien)
                    .Select(x => x.DuongDanAnh)
                    .SingleOrDefault()
            };

            NapDanhMuc(
                model.MaLoaiHang,
                model.MaThuongHieu);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Sua(SuaSanPhamViewModel model)
        {
            var sanPham = db.SanPhams
                .SingleOrDefault(x =>
                    x.MaSanPham == model.MaSanPham);

            if (sanPham == null)
            {
                return HttpNotFound("Không tìm thấy sản phẩm.");
            }

            var anhDaiDien = db.HinhAnhSanPhams.SingleOrDefault(x =>
                x.MaSanPham == sanPham.MaSanPham && x.AnhDaiDien);

            model.DuongDanAnhDaiDien = anhDaiDien?.DuongDanAnh;
            model.TenSanPham = (model.TenSanPham ?? "").Trim();
            model.MoTa = (model.MoTa ?? "").Trim();

            if (string.IsNullOrWhiteSpace(model.TenSanPham))
            {
                DatLoiTruong(
                    "TenSanPham",
                    "Vui lòng nhập tên sản phẩm.");
            }

            if (!model.MaLoaiHang.HasValue)
            {
                DatLoiNhapSo(
                    "MaLoaiHang",
                    "Vui lòng chọn loại hàng.",
                    "Loại hàng không hợp lệ.");
            }
            else
            {
                int loaiHangCu = sanPham.MaLoaiHang;

                bool hopLe = db.LoaiHangs.Any(x =>
                    x.MaLoaiHang == model.MaLoaiHang.Value &&
                    (x.HoatDong || x.MaLoaiHang == loaiHangCu));

                if (!hopLe)
                {
                    DatLoiTruong(
                        "MaLoaiHang",
                        "Vui lòng chọn loại hàng đang hoạt động.");
                }
            }

            if (!model.MaThuongHieu.HasValue)
            {
                DatLoiNhapSo(
                    "MaThuongHieu",
                    "Vui lòng chọn thương hiệu.",
                    "Thương hiệu không hợp lệ.");
            }
            else
            {
                int thuongHieuCu = sanPham.MaThuongHieu;

                bool hopLe = db.ThuongHieux.Any(x =>
                    x.MaThuongHieu == model.MaThuongHieu.Value &&
                    (x.HoatDong || x.MaThuongHieu == thuongHieuCu));

                if (!hopLe)
                {
                    DatLoiTruong(
                        "MaThuongHieu",
                        "Vui lòng chọn thương hiệu đang hoạt động.");
                }
            }

            if (!model.PhanTramGiam.HasValue)
            {
                DatLoiNhapSo(
                    "PhanTramGiam",
                    "Vui lòng nhập phần trăm giảm.",
                    "Phần trăm giảm phải là số nguyên hợp lệ.");
            }
            else if (model.PhanTramGiam.Value < 0 ||
                     model.PhanTramGiam.Value > 100)
            {
                DatLoiTruong(
                    "PhanTramGiam",
                    "Phần trăm giảm phải từ 0 đến 100.");
            }

            if (!ModelState.IsValid)
            {
                NapDanhMuc(
                    model.MaLoaiHang,
                    model.MaThuongHieu);

                return View(model);
            }

            sanPham.TenSanPham = model.TenSanPham;
            sanPham.MaLoaiHang = model.MaLoaiHang.Value;
            sanPham.MaThuongHieu = model.MaThuongHieu.Value;
            sanPham.MoTa = model.MoTa;
            sanPham.PhanTramGiam = model.PhanTramGiam.Value;
            sanPham.NoiBat = model.NoiBat;
            sanPham.DangBan = model.DangBan;
            sanPham.NgayCapNhat = DateTime.Now;

            const string thuMucAnh = "~/Content/images/san-pham/";
            string tepMoi = null;
            string thuMucVatLy = null;
            bool daLuu = false;

            try
            {
                if (model.TepAnhDaiDien != null)
                {
                    thuMucVatLy = Server.MapPath(thuMucAnh);
                    var dichVuAnh = new HinhAnhSanPhamService();
                    tepMoi = dichVuAnh.LuuAnh(model.TepAnhDaiDien, thuMucVatLy);

                    if (anhDaiDien == null)
                    {
                        anhDaiDien = new HinhAnhSanPham
                        {
                            MaSanPham = sanPham.MaSanPham,
                            AnhDaiDien = true,
                            ThuTu = 0
                        };

                        db.HinhAnhSanPhams.Add(anhDaiDien);
                    }

                    anhDaiDien.DuongDanAnh = thuMucAnh + tepMoi;
                }

                db.SaveChanges();
                daLuu = true;
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidDataException)
            {
                ModelState.AddModelError("TepAnhDaiDien", ex.Message);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                Trace.TraceError("Lỗi ghi ảnh đại diện: {0}", ex);
                ModelState.AddModelError(
                    "TepAnhDaiDien", "Chưa ghi được ảnh. Vui lòng thử lại.");
            }
            catch (DbUpdateException ex)
            {
                Trace.TraceError("Lỗi cập nhật sản phẩm: {0}", ex);
                ModelState.AddModelError(
                    "",
                    "Chưa lưu được thay đổi. Vui lòng kiểm tra dữ liệu rồi thử lại.");
            }
            finally
            {
                if (!daLuu && tepMoi != null)
                {
                    try
                    {
                        System.IO.File.Delete(Path.Combine(thuMucVatLy, tepMoi));
                    }
                    catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                    {
                        Trace.TraceError("Lỗi dọn ảnh đại diện: {0}", ex);
                    }
                }
            }

            if (!daLuu)
            {
                NapDanhMuc(model.MaLoaiHang, model.MaThuongHieu);
                return View(model);
            }

            TempData["ThanhCong"] =
                "Cập nhật sản phẩm thành công.";

            return RedirectToAction(
                "ChiTiet",
                new { id = sanPham.MaSanPham });
        }

        [HttpGet]
        public ActionResult ThemPhanLoai(int? id)
        {
            if (!id.HasValue || id.Value <= 0)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest,
                    "Mã sản phẩm không hợp lệ.");
            }

            var sanPham = db.SanPhams
                .AsNoTracking()
                .SingleOrDefault(x => x.MaSanPham == id.Value);

            if (sanPham == null)
            {
                return HttpNotFound("Không tìm thấy sản phẩm.");
            }

            var model = new ThemPhanLoaiViewModel
            {
                MaSanPham = sanPham.MaSanPham,
                TenSanPham = sanPham.TenSanPham,
                SoLuongTon = 0
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThemPhanLoai(
            ThemPhanLoaiViewModel model)
        {
            var sanPham = db.SanPhams
                .SingleOrDefault(x =>
                    x.MaSanPham == model.MaSanPham);

            if (sanPham == null)
            {
                return HttpNotFound("Không tìm thấy sản phẩm.");
            }

            model.TenSanPham = sanPham.TenSanPham;

            bool coAnhTheoMau = model.TepAnhTheoMau != null;

            if (coAnhTheoMau && string.IsNullOrWhiteSpace(model.MauSac))
            {
                ModelState.AddModelError("MauSac", "Vui lòng nhập màu cho ảnh đã chọn.");
            }

            model.MauSac = QuyTacSanPham.ChuanHoa(model.MauSac);
            model.KichThuoc = QuyTacSanPham.ChuanHoa(model.KichThuoc);

            KiemTraGia(model.GiaBan, "GiaBan");

            KiemTraSoLuong(
                model.SoLuongTon,
                "SoLuongTon",
                0,
                "Vui lòng nhập số lượng tồn.",
                "Số lượng tồn phải là số nguyên hợp lệ.",
                "Số lượng tồn không được nhỏ hơn 0.");

            bool daTonTai = db.BienTheSanPhams.Any(x =>
                x.MaSanPham == sanPham.MaSanPham &&
                x.MauSac == model.MauSac &&
                x.KichThuoc == model.KichThuoc);

            if (daTonTai)
            {
                ModelState.AddModelError(
                    "KichThuoc",
                    "Màu và kích cỡ này đã tồn tại. "
                    + "Vui lòng dùng Bổ sung hàng để nhập thêm tồn.");
            }

            if (coAnhTheoMau && db.HinhAnhSanPhams.Any(x =>
                x.MaSanPham == sanPham.MaSanPham && x.MauSac == model.MauSac))
            {
                ModelState.AddModelError(
                    "TepAnhTheoMau",
                    "Màu này đã có ảnh. Vui lòng để trống để dùng lại ảnh hiện có.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var phanLoai = new BienTheSanPham
            {
                MaSanPham = sanPham.MaSanPham,
                SKU = TaoSku(),
                MauSac = model.MauSac,
                KichThuoc = model.KichThuoc,
                GiaBan = model.GiaBan.Value,
                SoLuongTon = model.SoLuongTon.Value,
                DangBan = true
            };

            const string thuMucAnh = "~/Content/images/san-pham/";
            string thuMucVatLy = null;
            string tenTepDaLuu = null;
            bool daLuuDb = false;

            try
            {
                if (coAnhTheoMau)
                {
                    thuMucVatLy = Server.MapPath(thuMucAnh);
                    var dichVuAnh = new HinhAnhSanPhamService();
                    tenTepDaLuu = dichVuAnh.LuuAnh(model.TepAnhTheoMau, thuMucVatLy);

                    db.HinhAnhSanPhams.Add(new HinhAnhSanPham
                    {
                        MaSanPham = sanPham.MaSanPham,
                        DuongDanAnh = thuMucAnh + tenTepDaLuu,
                        MauSac = model.MauSac,
                        AnhDaiDien = false,
                        ThuTu = 1
                    });
                }

                db.BienTheSanPhams.Add(phanLoai);
                sanPham.NgayCapNhat = DateTime.Now;

                db.SaveChanges();
                daLuuDb = true;
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidDataException)
            {
                ModelState.AddModelError("TepAnhTheoMau", ex.Message);
                return View(model);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                Trace.TraceError("Lỗi ghi ảnh phân loại: {0}", ex);
                ModelState.AddModelError(
                    "TepAnhTheoMau", "Chưa ghi được ảnh. Vui lòng thử lại.");

                return View(model);
            }
            catch (DbUpdateException ex)
            {
                Trace.TraceError("Lỗi lưu phân loại: {0}", ex);
                ModelState.AddModelError(
                    "",
                    "Chưa lưu được phân loại. Vui lòng kiểm tra "
                    + "màu và kích cỡ hoặc tải lại trang rồi thử lại.");

                return View(model);
            }
            finally
            {
                if (!daLuuDb && tenTepDaLuu != null)
                {
                    try
                    {
                        System.IO.File.Delete(Path.Combine(thuMucVatLy, tenTepDaLuu));
                    }
                    catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                    {
                        Trace.TraceError("Lỗi dọn ảnh phân loại: {0}", ex);
                    }
                }
            }

            TempData["ThanhCong"] =
                "Thêm phân loại thành công.";

            return RedirectToAction(
                "ChiTiet",
                new { id = sanPham.MaSanPham });
        }

        [HttpGet]
        public ActionResult SuaPhanLoai(int? id)
        {
            if (!id.HasValue || id.Value <= 0)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest,
                    "Mã phân loại không hợp lệ.");
            }

            var phanLoai = db.BienTheSanPhams
                .AsNoTracking()
                .Include(x => x.SanPham)
                .SingleOrDefault(x => x.MaBienThe == id.Value);

            if (phanLoai == null)
            {
                return HttpNotFound(
                    "Không tìm thấy phân loại sản phẩm.");
            }

            var model = new SuaPhanLoaiViewModel
            {
                MaBienThe = phanLoai.MaBienThe,
                MaSanPham = phanLoai.MaSanPham,
                TenSanPham = phanLoai.SanPham.TenSanPham,
                SKU = phanLoai.SKU,
                MauSac = phanLoai.MauSac == "Mặc định"
                    ? ""
                    : phanLoai.MauSac,
                KichThuoc = phanLoai.KichThuoc == "Mặc định"
                    ? ""
                    : phanLoai.KichThuoc,
                GiaBan = phanLoai.GiaBan,
                DangBan = phanLoai.DangBan
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SuaPhanLoai(
            SuaPhanLoaiViewModel model)
        {
            var phanLoai = db.BienTheSanPhams
                .Include(x => x.SanPham)
                .SingleOrDefault(x =>
                    x.MaBienThe == model.MaBienThe);

            if (phanLoai == null)
            {
                return HttpNotFound(
                    "Không tìm thấy phân loại sản phẩm.");
            }

            model.MaSanPham = phanLoai.MaSanPham;
            model.TenSanPham = phanLoai.SanPham.TenSanPham;
            model.SKU = phanLoai.SKU;

            model.MauSac = QuyTacSanPham.ChuanHoa(model.MauSac);
            model.KichThuoc = QuyTacSanPham.ChuanHoa(model.KichThuoc);

            KiemTraGia(model.GiaBan, "GiaBan");

            bool doiMau = !string.Equals(
                QuyTacSanPham.ChuanHoa(phanLoai.MauSac),
                model.MauSac,
                StringComparison.OrdinalIgnoreCase);

            bool doiKichThuoc = !string.Equals(
                QuyTacSanPham.ChuanHoa(phanLoai.KichThuoc),
                model.KichThuoc,
                StringComparison.OrdinalIgnoreCase);

            if ((doiMau || doiKichThuoc) &&
                phanLoai.ChiTietDonHangs.Any())
            {
                if (doiMau)
                {
                    ModelState.AddModelError(
                        "MauSac",
                        "Phân loại đã có đơn hàng nên không được đổi màu. "
                        + "Vui lòng tạo phân loại mới.");
                }

                if (doiKichThuoc)
                {
                    ModelState.AddModelError(
                        "KichThuoc",
                        "Phân loại đã có đơn hàng nên không được đổi kích cỡ. "
                        + "Vui lòng tạo phân loại mới.");
                }
            }

            bool biTrung = db.BienTheSanPhams.Any(x =>
                x.MaSanPham == phanLoai.MaSanPham &&
                x.MaBienThe != phanLoai.MaBienThe &&
                x.MauSac == model.MauSac &&
                x.KichThuoc == model.KichThuoc);

            if (biTrung)
            {
                ModelState.AddModelError(
                    "KichThuoc",
                    "Sản phẩm đã có phân loại với màu và kích cỡ này.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            phanLoai.MauSac = model.MauSac;
            phanLoai.KichThuoc = model.KichThuoc;
            phanLoai.GiaBan = model.GiaBan.Value;
            phanLoai.DangBan = model.DangBan;

            phanLoai.SanPham.NgayCapNhat = DateTime.Now;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    "",
                    "Chưa lưu được phân loại. Vui lòng kiểm tra "
                    + "màu và kích cỡ hoặc tải lại trang rồi thử lại.");

                return View(model);
            }

            TempData["ThanhCong"] =
                "Cập nhật phân loại thành công.";

            return RedirectToAction(
                "ChiTiet",
                new { id = phanLoai.MaSanPham });
        }

        [HttpGet]
        public ActionResult BoSungHang(int? id)
        {
            int maTaiKhoan;
            var loiQuyen = KiemTraQuyenBoSungHang(out maTaiKhoan);

            if (loiQuyen != null)
            {
                return loiQuyen;
            }

            if (!id.HasValue || id.Value <= 0)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest,
                    "Mã sản phẩm không hợp lệ.");
            }

            var sanPham = db.SanPhams
                .AsNoTracking()
                .SingleOrDefault(x => x.MaSanPham == id.Value);

            if (sanPham == null)
            {
                return HttpNotFound("Không tìm thấy sản phẩm.");
            }

            var model = new BoSungHangViewModel
            {
                MaSanPham = sanPham.MaSanPham,
                TenSanPham = sanPham.TenSanPham,
                YeuCauId = Guid.NewGuid()
            };

            NapPhanLoaiBoSungHang(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BoSungHang(
            BoSungHangViewModel model)
        {
            int maTaiKhoan;
            var loiQuyen = KiemTraQuyenBoSungHang(out maTaiKhoan);

            if (loiQuyen != null)
            {
                return loiQuyen;
            }

            var sanPham = db.SanPhams
                .AsNoTracking()
                .SingleOrDefault(x =>
                    x.MaSanPham == model.MaSanPham);

            if (sanPham == null)
            {
                return HttpNotFound("Không tìm thấy sản phẩm.");
            }

            model.TenSanPham = sanPham.TenSanPham;
            model.LyDo = (model.LyDo ?? "").Trim();

            if (!model.MaBienThe.HasValue)
            {
                DatLoiNhapSo(
                    "MaBienThe",
                    "Vui lòng chọn loại cần bổ sung.",
                    "Loại cần bổ sung không hợp lệ.");
            }
            else
            {
                bool hopLe = db.BienTheSanPhams.Any(x =>
                    x.MaBienThe == model.MaBienThe.Value &&
                    x.MaSanPham == sanPham.MaSanPham);

                if (!hopLe)
                {
                    DatLoiTruong(
                        "MaBienThe",
                        "Vui lòng chọn phân loại thuộc sản phẩm này.");
                }
            }

            KiemTraSoLuong(
                model.SoLuongThem,
                "SoLuongThem",
                1,
                "Vui lòng nhập số lượng cần bổ sung.",
                "Số lượng bổ sung phải là số nguyên hợp lệ.",
                "Số lượng bổ sung phải lớn hơn 0.");

            if (model.YeuCauId == Guid.Empty)
            {
                ModelState.Remove("YeuCauId");

                ModelState.AddModelError(
                    "",
                    "Yêu cầu bổ sung không hợp lệ. "
                    + "Vui lòng quay lại chi tiết sản phẩm và mở lại form.");
            }

            if (!ModelState.IsValid)
            {
                NapPhanLoaiBoSungHang(model);

                return View(model);
            }

            try
            {
                var service = new TonKhoService();

                bool daBoSung = service.BoSungHang(
                    sanPham.MaSanPham,
                    model.MaBienThe.Value,
                    model.SoLuongThem.Value,
                    maTaiKhoan,
                    model.YeuCauId,
                    model.LyDo);

                TempData["ThanhCong"] = daBoSung
                    ? "Bổ sung hàng thành công."
                    : "Yêu cầu này đã được xử lý trước đó. "
                        + "Hệ thống không cộng tồn kho lần nữa.";

                return RedirectToAction(
                    "ChiTiet",
                    new { id = sanPham.MaSanPham });
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            catch (SqlException)
            {
                ModelState.AddModelError(
                    "",
                    "Chưa bổ sung được hàng do lỗi cơ sở dữ liệu. "
                    + "Vui lòng thử lại với form hiện tại.");
            }

            NapPhanLoaiBoSungHang(model);

            return View(model);
        }

        private void NapDanhMuc(
            int? maLoaiHang = null,
            int? maThuongHieu = null)
        {
            int loaiDangChon = maLoaiHang.GetValueOrDefault();
            int thuongHieuDangChon = maThuongHieu.GetValueOrDefault();

            ViewBag.MaLoaiHang = new SelectList(
                db.LoaiHangs
                    .AsNoTracking()
                    .Where(x =>
                        x.HoatDong ||
                        x.MaLoaiHang == loaiDangChon)
                    .OrderBy(x => x.TenLoaiHang)
                    .ToList(),
                "MaLoaiHang",
                "TenLoaiHang",
                maLoaiHang);

            ViewBag.MaThuongHieu = new SelectList(
                db.ThuongHieux
                    .AsNoTracking()
                    .Where(x =>
                        x.HoatDong ||
                        x.MaThuongHieu == thuongHieuDangChon)
                    .OrderBy(x => x.TenThuongHieu)
                    .ToList(),
                "MaThuongHieu",
                "TenThuongHieu",
                maThuongHieu);
        }

        private void NapPhanLoaiBoSungHang(
            BoSungHangViewModel model)
        {
            var danhSach = db.BienTheSanPhams
                .AsNoTracking()
                .Where(x => x.MaSanPham == model.MaSanPham)
                .OrderBy(x => x.MauSac)
                .ThenBy(x => x.KichThuoc)
                .ToList();

            var vi = CultureInfo.GetCultureInfo("vi-VN");

            model.DanhSachPhanLoai = danhSach
                .Select(x => new SelectListItem
                {
                    Value = x.MaBienThe.ToString(),
                    Text = x.MauSac
                        + " / "
                        + x.KichThuoc
                        + " — đang còn "
                        + x.SoLuongTon.ToString("N0", vi)
                        + (x.DangBan ? "" : " (ngừng bán)"),
                    Selected = model.MaBienThe == x.MaBienThe
                })
                .ToList();
        }

        private ActionResult KiemTraQuyenBoSungHang(
            out int maTaiKhoan)
        {
            bool daDangNhap = int.TryParse(
                Convert.ToString(Session["MaTaiKhoan"]),
                out maTaiKhoan);

            if (!daDangNhap || maTaiKhoan <= 0)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.Unauthorized,
                    "Vui lòng đăng nhập để bổ sung hàng.");
            }

            int maDangNhap = maTaiKhoan;

            bool coQuyen = db.TaiKhoans
                .AsNoTracking()
                .Any(x =>
                    x.MaTaiKhoan == maDangNhap &&
                    x.VaiTro == "Admin" &&
                    x.HoatDong);

            if (!coQuyen)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.Forbidden,
                    "Bạn không có quyền bổ sung hàng.");
            }

            return null;
        }

        private void KiemTraGia(
            decimal? giaBan,
            string tenTruong)
        {
            if (!giaBan.HasValue)
            {
                DatLoiNhapSo(
                    tenTruong,
                    "Vui lòng nhập giá gốc.",
                    "Vui lòng nhập giá gốc bằng số hợp lệ.");

                return;
            }

            if (giaBan.Value < 0 ||
                giaBan.Value > 9999999999999999.99m)
            {
                DatLoiTruong(
                    tenTruong,
                    "Giá gốc phải từ 0 đến 9.999.999.999.999.999,99 đồng.");

                return;
            }

            if (decimal.Round(giaBan.Value, 2) != giaBan.Value)
            {
                DatLoiTruong(
                    tenTruong,
                    "Giá gốc chỉ được có tối đa 2 chữ số thập phân.");
            }
        }

        private void KiemTraSoLuong(
            int? soLuong,
            string tenTruong,
            int giaTriNhoNhat,
            string loiBoTrong,
            string loiSaiDinhDang,
            string loiNgoaiKhoang)
        {
            if (!soLuong.HasValue)
            {
                DatLoiNhapSo(
                    tenTruong,
                    loiBoTrong,
                    loiSaiDinhDang);

                return;
            }

            if (soLuong.Value < giaTriNhoNhat)
            {
                DatLoiTruong(
                    tenTruong,
                    loiNgoaiKhoang);
            }
        }

        private void DatLoiNhapSo(
            string tenTruong,
            string loiBoTrong,
            string loiSaiDinhDang)
        {
            var trangThai = ModelState[tenTruong];

            string giaTriDaNhap = trangThai != null &&
                trangThai.Value != null
                ? trangThai.Value.AttemptedValue
                : null;

            DatLoiTruong(
                tenTruong,
                string.IsNullOrWhiteSpace(giaTriDaNhap)
                    ? loiBoTrong
                    : loiSaiDinhDang);
        }

        private void DatLoiTruong(
            string tenTruong,
            string thongBao)
        {
            var trangThai = ModelState[tenTruong];

            if (trangThai != null)
            {
                trangThai.Errors.Clear();
            }

            ModelState.AddModelError(
                tenTruong,
                thongBao);
        }

        private static string TaoSku()
        {
            return "SX-" + Guid.NewGuid()
                .ToString("N")
                .ToUpperInvariant();
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