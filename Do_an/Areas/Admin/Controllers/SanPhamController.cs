using Do_an.Models;
using Do_an.Services;
using Do_an.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web.Mvc;

namespace Do_an.Areas.Admin.Controllers
{
    public class SanPhamController : Controller
    {
        private readonly SPORTIXEntities db = new SPORTIXEntities();

        public ActionResult Index(string tuKhoa, int trang = 1)
        {
            const int kichThuocTrang = 10;

            tuKhoa = (tuKhoa ?? "").Trim();

            IQueryable<SanPham> query = db.SanPhams.AsNoTracking();

            if (!string.IsNullOrEmpty(tuKhoa))
            {
                query = query.Where(sp =>
                    sp.TenSanPham.Contains(tuKhoa) ||
                    sp.BienTheSanPhams.Any(bt =>
                        bt.SKU.Contains(tuKhoa)));
            }

            int tongSanPham = query.Count();

            int tongTrang = Math.Max(
                1,
                (int)Math.Ceiling(
                    tongSanPham / (double)kichThuocTrang
                )
            );

            trang = Math.Max(1, Math.Min(trang, tongTrang));

            var danhSach = query
                .Include(sp => sp.LoaiHang)
                .Include(sp => sp.ThuongHieu)
                .Include(sp => sp.BienTheSanPhams)
                .Include(sp => sp.HinhAnhSanPhams)
                .OrderByDescending(sp => sp.NgayTao)
                .ThenByDescending(sp => sp.MaSanPham)
                .Skip((trang - 1) * kichThuocTrang)
                .Take(kichThuocTrang)
                .ToList();

            ViewBag.TuKhoa = tuKhoa;
            ViewBag.Trang = trang;
            ViewBag.TongTrang = tongTrang;
            ViewBag.TongSanPham = tongSanPham;

            return View(danhSach);
        }

        public ActionResult ChiTiet(int? id)
        {
            if (!id.HasValue)
            {
                return new HttpStatusCodeResult(
                    System.Net.HttpStatusCode.BadRequest
                );
            }

            var sanPham = db.SanPhams
                .AsNoTracking()
                .Include(sp => sp.LoaiHang)
                .Include(sp => sp.ThuongHieu)
                .Include(sp => sp.HinhAnhSanPhams)
                .Include(sp => sp.BienTheSanPhams)
                .SingleOrDefault(sp => sp.MaSanPham == id.Value);

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

            model.PhanLoai.Add(new PhanLoaiSanPhamInput
            {
                SoLuongTon = 0
            });

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
                ModelState.AddModelError(
                    "TenSanPham",
                    "Nhập tên sản phẩm."
                );
            }

            if (!db.LoaiHangs.Any(x =>
                x.MaLoaiHang == model.MaLoaiHang && x.HoatDong))
            {
                ModelState.AddModelError(
                    "MaLoaiHang",
                    "Loại hàng không hợp lệ."
                );
            }

            if (!db.ThuongHieux.Any(x =>
                x.MaThuongHieu == model.MaThuongHieu && x.HoatDong))
            {
                ModelState.AddModelError(
                    "MaThuongHieu",
                    "Thương hiệu không hợp lệ."
                );
            }

            model.PhanLoai = model.PhanLoai
                ?? new List<PhanLoaiSanPhamInput>();

            if (model.PhanLoai.Count == 0 ||
                model.PhanLoai.Count > 100)
            {
                ModelState.AddModelError(
                    "",
                    "Sản phẩm phải có từ 1 đến 100 dòng phân loại."
                );
            }

            var toHopDaCo = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase
            );

            for (int i = 0; i < model.PhanLoai.Count; i++)
            {
                var dong = model.PhanLoai[i];

                if (dong == null)
                {
                    ModelState.AddModelError(
                        "",
                        "Dòng phân loại không hợp lệ."
                    );

                    continue;
                }

                dong.MauSac = QuyTacSanPham.ChuanHoa(
                    dong.MauSac
                );

                dong.KichThuoc = QuyTacSanPham.ChuanHoa(
                    dong.KichThuoc
                );

                string khoa = QuyTacSanPham.Khoa(
                    dong.MauSac,
                    dong.KichThuoc
                );

                if (!toHopDaCo.Add(khoa))
                {
                    ModelState.AddModelError(
                        "",
                        "Dòng " + (i + 1)
                        + " bị trùng màu và kích cỡ với dòng trước."
                    );
                }
            }

            if (!ModelState.IsValid)
            {
                model.PhanLoai = model.PhanLoai
                    .Where(x => x != null)
                    .ToList();

                NapDanhMuc(
                    model.MaLoaiHang,
                    model.MaThuongHieu
                );

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
                sanPham.BienTheSanPhams.Add(
                    new BienTheSanPham
                    {
                        SKU = "SX-" + Guid.NewGuid()
                            .ToString("N")
                            .ToUpperInvariant(),

                        MauSac = dong.MauSac,
                        KichThuoc = dong.KichThuoc,
                        GiaBan = dong.GiaBan.Value,
                        SoLuongTon = dong.SoLuongTon.Value,
                        DangBan = true
                    }
                );
            }

            db.SanPhams.Add(sanPham);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    "",
                    "Chưa lưu được sản phẩm. Kiểm tra danh mục "
                    + "và các dòng phân loại rồi thử lại."
                );

                NapDanhMuc(
                    model.MaLoaiHang,
                    model.MaThuongHieu
                );

                return View(model);
            }

            TempData["ThanhCong"] = "Thêm sản phẩm thành công.";

            return RedirectToAction(
                "ChiTiet",
                new { id = sanPham.MaSanPham }
            );
        }

        [HttpGet]
        public ActionResult ThemPhanLoai(int? id)
        {
            if (!id.HasValue)
            {
                return new HttpStatusCodeResult(
                    System.Net.HttpStatusCode.BadRequest
                );
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

            model.MauSac = QuyTacSanPham.ChuanHoa(
                model.MauSac
            );

            model.KichThuoc = QuyTacSanPham.ChuanHoa(
                model.KichThuoc
            );

            bool daTonTai = db.BienTheSanPhams.Any(x =>
                x.MaSanPham == model.MaSanPham &&
                x.MauSac == model.MauSac &&
                x.KichThuoc == model.KichThuoc);

            if (daTonTai)
            {
                ModelState.AddModelError(
                    "",
                    "Màu và kích cỡ này đã tồn tại. "
                    + "Không tạo lại cùng một loại bán hàng."
                );
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var phanLoai = new BienTheSanPham
            {
                MaSanPham = sanPham.MaSanPham,

                SKU = "SX-" + Guid.NewGuid()
                    .ToString("N")
                    .ToUpperInvariant(),

                MauSac = model.MauSac,
                KichThuoc = model.KichThuoc,
                GiaBan = model.GiaBan.Value,
                SoLuongTon = model.SoLuongTon.Value,
                DangBan = true
            };

            db.BienTheSanPhams.Add(phanLoai);

            sanPham.NgayCapNhat = DateTime.Now;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    "",
                    "Chưa lưu được màu / kích cỡ. "
                    + "Hãy kiểm tra dữ liệu hoặc tải lại trang rồi thử lại."
                );

                return View(model);
            }

            TempData["ThanhCong"] =
                "Thêm màu / kích cỡ thành công.";

            return RedirectToAction(
                "ChiTiet",
                new { id = sanPham.MaSanPham }
            );
        }

        private void NapDanhMuc(
            int? maLoaiHang = null,
            int? maThuongHieu = null)
        {
            ViewBag.MaLoaiHang = new SelectList(
                db.LoaiHangs
                    .AsNoTracking()
                    .Where(x => x.HoatDong)
                    .OrderBy(x => x.TenLoaiHang)
                    .ToList(),
                "MaLoaiHang",
                "TenLoaiHang",
                maLoaiHang
            );

            ViewBag.MaThuongHieu = new SelectList(
                db.ThuongHieux
                    .AsNoTracking()
                    .Where(x => x.HoatDong)
                    .OrderBy(x => x.TenThuongHieu)
                    .ToList(),
                "MaThuongHieu",
                "TenThuongHieu",
                maThuongHieu
            );
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