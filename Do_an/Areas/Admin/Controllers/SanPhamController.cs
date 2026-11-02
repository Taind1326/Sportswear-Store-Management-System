using Do_an.Models;
using System;
using System.Data.Entity;
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
                    sp.BienTheSanPhams.Any(bt => bt.SKU.Contains(tuKhoa)));
            }

            int tongSanPham = query.Count();
            int tongTrang = Math.Max(
                1,
                (int)Math.Ceiling(tongSanPham / (double)kichThuocTrang)
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