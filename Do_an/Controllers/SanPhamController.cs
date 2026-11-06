using Do_an.Models;
using Do_an.Services;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Do_an.Controllers
{
    public class SanPhamController : Controller
    {
        [HttpGet]
        public ActionResult Index(
            string tuKhoa,
            int? loaiHang,
            int? thuongHieu,
            long? giaTu,
            long? giaDen,
            string sapXep,
            int trang = 1)
        {
            const int kichThuocTrang = 12;
            tuKhoa = (tuKhoa ?? "").Trim();

            using (var db = new SPORTIXEntities())
            {
                var thoiDiem = DateTime.Now;
                var mocSanPhamMoi = thoiDiem.AddDays(-30);

                ViewBag.SanPhamMoi = db.SanPhams.AsNoTracking()
                    .Include(x => x.LoaiHang)
                    .Include(x => x.HinhAnhSanPhams)
                    .Include(x => x.BienTheSanPhams)
                    .Where(x =>
                        x.DangBan &&
                        x.NgayTao > mocSanPhamMoi &&
                        x.NgayTao <= thoiDiem &&
                        x.BienTheSanPhams.Any(bt => bt.DangBan))
                    .OrderByDescending(x => x.NgayTao)
                    .ThenByDescending(x => x.MaSanPham)
                    .Take(8)
                    .ToList();

                var query = db.SanPhams.AsNoTracking()
                    .Where(x =>
                        x.DangBan &&
                        x.BienTheSanPhams.Any(bt => bt.DangBan));

                var giaCaoNhat = query.Select(x =>
                    (decimal?)Math.Floor(
                        x.BienTheSanPhams
                            .Where(bt => bt.DangBan)
                            .Min(bt => bt.GiaBan)
                        * (100 - x.PhanTramGiam) / 100m + 0.5m))
                    .Max() ?? 0m;

                ViewBag.GiaToiDa = Math.Max(
                    5000000L,
                    (long)giaCaoNhat);

                if (tuKhoa.Length > 0)
                {
                    query = query.Where(x =>
                        x.TenSanPham.Contains(tuKhoa));
                }

                if (loaiHang.HasValue)
                {
                    query = query.Where(x =>
                        x.MaLoaiHang == loaiHang.Value);
                }

                if (thuongHieu.HasValue)
                {
                    query = query.Where(x =>
                        x.MaThuongHieu == thuongHieu.Value);
                }

                var bangGia = query.Select(x => new
                {
                    x.MaSanPham,
                    x.NgayTao,
                    Gia = Math.Floor(
                        x.BienTheSanPhams
                            .Where(bt => bt.DangBan)
                            .Min(bt => bt.GiaBan)
                        * (100 - x.PhanTramGiam) / 100m + 0.5m)
                });

                if (giaTu.HasValue && giaTu.Value >= 0)
                {
                    bangGia = bangGia.Where(x =>
                        x.Gia >= giaTu.Value);
                }

                if (giaDen.HasValue && giaDen.Value >= 0)
                {
                    bangGia = bangGia.Where(x =>
                        x.Gia <= giaDen.Value);
                }

                if (giaTu.HasValue &&
                    giaDen.HasValue &&
                    giaTu.Value > giaDen.Value)
                {
                    ModelState.AddModelError(
                        "",
                        "Giá từ phải nhỏ hơn hoặc bằng giá đến.");
                }

                int tongSanPham = bangGia.Count();

                int tongTrang = Math.Max(
                    1,
                    (int)Math.Ceiling(
                        tongSanPham / (double)kichThuocTrang));

                trang = Math.Max(1, Math.Min(trang, tongTrang));

                var thuTu = sapXep == "gia-tang"
                    ? bangGia.OrderBy(x => x.Gia)
                    : sapXep == "gia-giam"
                        ? bangGia.OrderByDescending(x => x.Gia)
                        : bangGia.OrderByDescending(x => x.NgayTao);

                var maSanPhams = thuTu
                    .ThenByDescending(x => x.MaSanPham)
                    .Skip((trang - 1) * kichThuocTrang)
                    .Take(kichThuocTrang)
                    .Select(x => x.MaSanPham)
                    .ToList();

                var sanPhams = db.SanPhams.AsNoTracking()
                    .Include(x => x.LoaiHang)
                    .Include(x => x.HinhAnhSanPhams)
                    .Include(x => x.BienTheSanPhams)
                    .Where(x =>
                        x.DangBan &&
                        maSanPhams.Contains(x.MaSanPham))
                    .ToList()
                    .ToDictionary(x => x.MaSanPham);

                var loaiHangs = db.LoaiHangs
                    .Where(x =>
                        x.HoatDong ||
                        x.MaLoaiHang == loaiHang)
                    .OrderBy(x => x.TenLoaiHang)
                    .ToList();

                var thuongHieus = db.ThuongHieux
                    .Where(x =>
                        x.HoatDong ||
                        x.MaThuongHieu == thuongHieu)
                    .OrderBy(x => x.TenThuongHieu)
                    .ToList();

                ViewBag.LoaiHang = new SelectList(
                    loaiHangs,
                    "MaLoaiHang",
                    "TenLoaiHang",
                    loaiHang);

                ViewBag.ThuongHieu = new SelectList(
                    thuongHieus,
                    "MaThuongHieu",
                    "TenThuongHieu",
                    thuongHieu);

                ViewBag.SapXep = new SelectList(new[]
                {
                    new SelectListItem
                    {
                        Value = "",
                        Text = "Mới nhất"
                    },
                    new SelectListItem
                    {
                        Value = "gia-tang",
                        Text = "Giá thấp đến cao"
                    },
                    new SelectListItem
                    {
                        Value = "gia-giam",
                        Text = "Giá cao đến thấp"
                    }
                }, "Value", "Text", sapXep);

                ViewBag.TuKhoa = tuKhoa;
                ViewBag.GiaTu = giaTu;
                ViewBag.GiaDen = giaDen;
                ViewBag.Trang = trang;
                ViewBag.TongTrang = tongTrang;
                ViewBag.TongSanPham = tongSanPham;

                return View(maSanPhams
                    .Where(sanPhams.ContainsKey)
                    .Select(ma => sanPhams[ma])
                    .ToList());
            }
        }

        [HttpGet]
        public ActionResult BienThe(int id, int maBienThe)
        {
            using (var db = new SPORTIXEntities())
            {
                var bienThe = db.BienTheSanPhams.AsNoTracking()
                    .Where(x =>
                        x.MaSanPham == id &&
                        x.MaBienThe == maBienThe &&
                        x.DangBan &&
                        x.SanPham.DangBan)
                    .Select(x => new
                    {
                        x.MaBienThe,
                        x.MauSac,
                        x.KichThuoc,
                        x.GiaBan,
                        x.SoLuongTon,
                        x.SanPham.PhanTramGiam
                    })
                    .SingleOrDefault();

                if (bienThe == null)
                {
                    return HttpNotFound();
                }

                return Json(new
                {
                    bienThe.MaBienThe,
                    MauSac = QuyTacSanPham.ChuanHoa(
                        bienThe.MauSac),
                    KichThuoc = QuyTacSanPham.ChuanHoa(
                        bienThe.KichThuoc),
                    GiaGoc = bienThe.GiaBan,
                    GiaSauGiam = QuyTacSanPham.GiaSauGiam(
                        bienThe.GiaBan,
                        bienThe.PhanTramGiam),
                    bienThe.SoLuongTon,
                    ConHang = bienThe.SoLuongTon > 0
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult ChiTiet(int? id)
        {
            if (!id.HasValue)
            {
                return HttpNotFound("Thiếu mã sản phẩm.");
            }

            using (var db = new SPORTIXEntities())
            {
                var sanPham = db.SanPhams.AsNoTracking()
                    .Include(x => x.HinhAnhSanPhams)
                    .Include(x => x.BienTheSanPhams)
                    .Include(x => x.ThuongHieu)
                    .SingleOrDefault(x =>
                        x.MaSanPham == id.Value &&
                        x.DangBan);

                if (sanPham == null)
                {
                    return HttpNotFound(
                        "Không tìm thấy sản phẩm đang bán.");
                }

                return View(sanPham);
            }
        }
    }
}