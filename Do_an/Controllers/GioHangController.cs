
using System;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web.Mvc;
using Do_an.Models;
using Do_an.Services;
using Do_an.ViewModels;

namespace Do_an.Controllers
{
    public class GioHangController : Controller
    {
        private int? LayMaTaiKhoanHienTai()
        {
            object giaTri = Session["MaTaiKhoan"];

            int maTaiKhoan;

            if (giaTri == null ||
                !int.TryParse(Convert.ToString(giaTri), out maTaiKhoan))
            {
                return null;
            }

            return maTaiKhoan;
        }

        private ActionResult ChuyenDenDangNhap()
        {
            return RedirectToAction(
                "DangNhap",
                "TaiKhoan",
                new { returnUrl = Request.RawUrl }
            );
        }

        // Mở kết nối trước khi tạo transaction EF5.
        private DbTransaction TaoGiaoDichSerializable(
            SPORTIXEntities db)
        {
            var ketNoi = ((IObjectContextAdapter)db)
                .ObjectContext.Connection;

            if (ketNoi.State != ConnectionState.Open)
            {
                ketNoi.Open();
            }

            return ketNoi.BeginTransaction(
                IsolationLevel.Serializable
            );
        }

        // GET: GioHang
        public ActionResult Index()
        {
            int? maTaiKhoan = LayMaTaiKhoanHienTai();

            if (!maTaiKhoan.HasValue)
                return ChuyenDenDangNhap();

            using (var db = new SPORTIXEntities())
            {
                var model = new GioHangViewModel();

                var gioHang = db.GioHangs.FirstOrDefault(
                    x => x.MaTaiKhoan == maTaiKhoan.Value
                );

                if (gioHang == null)
                    return View(model);

                var danhSachChiTiet = db.ChiTietGioHangs
                    .Include("BienTheSanPham.SanPham.HinhAnhSanPhams")
                    .Where(x => x.MaGioHang == gioHang.MaGioHang)
                    .ToList();

                foreach (var chiTiet in danhSachChiTiet)
                {
                    var bienThe = chiTiet.BienTheSanPham;

                    if (bienThe == null || bienThe.SanPham == null)
                        continue;

                    var sanPham = bienThe.SanPham;

                    int phanTramGiam = sanPham.PhanTramGiam;

                    decimal giaGoc = bienThe.GiaBan;

                    decimal donGia = QuyTacSanPham.GiaSauGiam(
                        giaGoc,
                        phanTramGiam
                    );

                    var hinhAnh = sanPham.HinhAnhSanPhams
                        .OrderByDescending(x => x.AnhDaiDien)
                        .ThenBy(x => x.ThuTu)
                        .FirstOrDefault();

                    bool conTheMua =
                        bienThe.DangBan &&
                        sanPham.DangBan &&
                        bienThe.SoLuongTon >= chiTiet.SoLuong;

                    var dongHang = new GioHangDongViewModel
                    {
                        MaChiTietGioHang = chiTiet.MaChiTietGioHang,
                        MaBienThe = bienThe.MaBienThe,
                        TenSanPham = sanPham.TenSanPham,
                        SKU = bienThe.SKU,
                        MauSac = bienThe.MauSac,
                        KichThuoc = bienThe.KichThuoc,

                        DuongDanAnh = hinhAnh == null
                            ? null
                            : hinhAnh.DuongDanAnh,

                        GiaGoc = giaGoc,
                        DonGia = donGia,
                        PhanTramGiam = phanTramGiam,
                        SoLuong = chiTiet.SoLuong,
                        SoLuongTon = bienThe.SoLuongTon,
                        ConTheMua = conTheMua
                    };

                    model.DongHang.Add(dongHang);

                    model.TongSoLuong += chiTiet.SoLuong;

                    model.TienHang += dongHang.ThanhTien;
                }

                return View(model);
            }
        }

        // POST: GioHang/Them
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Them(int maBienThe, int soLuong = 1)
        {
            int? maTaiKhoan = LayMaTaiKhoanHienTai();

            if (!maTaiKhoan.HasValue)
                return ChuyenDenDangNhap();

            if (maBienThe <= 0 || soLuong <= 0)
            {
                TempData["ThongBaoLoi"] =
                    "Thông tin sản phẩm không hợp lệ.";

                return RedirectToAction("Index");
            }

            using (var db = new SPORTIXEntities())
            using (var giaoDich = TaoGiaoDichSerializable(db))
            {
                var bienThe = db.BienTheSanPhams
                    .Include("SanPham")
                    .FirstOrDefault(x => x.MaBienThe == maBienThe);

                if (bienThe == null || bienThe.SanPham == null)
                {
                    TempData["ThongBaoLoi"] =
                        "Không tìm thấy sản phẩm.";

                    return RedirectToAction("Index");
                }

                if (!bienThe.DangBan || !bienThe.SanPham.DangBan)
                {
                    TempData["ThongBaoLoi"] =
                        "Sản phẩm hiện không được kinh doanh.";

                    return RedirectToAction("Index");
                }

                var gioHang = db.GioHangs.FirstOrDefault(
                    x => x.MaTaiKhoan == maTaiKhoan.Value
                );

                if (gioHang == null)
                {
                    gioHang = new GioHang
                    {
                        MaTaiKhoan = maTaiKhoan.Value,
                        NgayTao = DateTime.Now
                    };

                    db.GioHangs.Add(gioHang);
                    db.SaveChanges();
                }

                var chiTiet = db.ChiTietGioHangs.FirstOrDefault(
                    x => x.MaGioHang == gioHang.MaGioHang &&
                         x.MaBienThe == maBienThe
                );

                int soLuongHienTai = chiTiet == null
                    ? 0
                    : chiTiet.SoLuong;

                // Kiểm tra trước khi cộng để tránh tràn số nguyên.
                if (soLuongHienTai > bienThe.SoLuongTon ||
                    soLuong > bienThe.SoLuongTon - soLuongHienTai)
                {
                    TempData["ThongBaoLoi"] =
                        "Số lượng trong kho không đủ. Hiện chỉ còn " +
                        bienThe.SoLuongTon + " sản phẩm.";

                    return RedirectToAction("Index");
                }

                int tongSoLuong = soLuongHienTai + soLuong;

                if (chiTiet == null)
                {
                    chiTiet = new ChiTietGioHang
                    {
                        MaGioHang = gioHang.MaGioHang,
                        MaBienThe = maBienThe,
                        SoLuong = tongSoLuong
                    };

                    db.ChiTietGioHangs.Add(chiTiet);
                }
                else
                {
                    chiTiet.SoLuong = tongSoLuong;
                }

                db.SaveChanges();

                giaoDich.Commit();

                TempData["ThongBao"] =
                    "Đã thêm sản phẩm vào giỏ hàng.";

                return RedirectToAction("Index");
            }
        }

        // POST: GioHang/CapNhat
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CapNhat(int id, int soLuong)
        {
            int? maTaiKhoan = LayMaTaiKhoanHienTai();

            if (!maTaiKhoan.HasValue)
                return ChuyenDenDangNhap();

            if (id <= 0 || soLuong <= 0)
            {
                TempData["ThongBaoLoi"] =
                    "Số lượng cập nhật không hợp lệ.";

                return RedirectToAction("Index");
            }

            using (var db = new SPORTIXEntities())
            {
                var gioHang = db.GioHangs.FirstOrDefault(
                    x => x.MaTaiKhoan == maTaiKhoan.Value
                );

                if (gioHang == null)
                    return RedirectToAction("Index");

                var chiTiet = db.ChiTietGioHangs
                    .Include("BienTheSanPham.SanPham")
                    .FirstOrDefault(
                        x => x.MaChiTietGioHang == id &&
                             x.MaGioHang == gioHang.MaGioHang
                    );

                if (chiTiet == null ||
                    chiTiet.BienTheSanPham == null)
                {
                    TempData["ThongBaoLoi"] =
                        "Không tìm thấy sản phẩm trong giỏ.";

                    return RedirectToAction("Index");
                }

                var bienThe = chiTiet.BienTheSanPham;

                if (bienThe.SanPham == null ||
                    !bienThe.DangBan ||
                    !bienThe.SanPham.DangBan)
                {
                    TempData["ThongBaoLoi"] =
                        "Sản phẩm hiện không thể mua.";

                    return RedirectToAction("Index");
                }

                if (soLuong > bienThe.SoLuongTon)
                {
                    TempData["ThongBaoLoi"] =
                        "Số lượng vượt quá tồn kho. Hiện chỉ còn " +
                        bienThe.SoLuongTon + " sản phẩm.";

                    return RedirectToAction("Index");
                }

                chiTiet.SoLuong = soLuong;

                db.SaveChanges();

                TempData["ThongBao"] =
                    "Đã cập nhật giỏ hàng.";

                return RedirectToAction("Index");
            }
        }

        // POST: GioHang/Xoa
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Xoa(int id)
        {
            int? maTaiKhoan = LayMaTaiKhoanHienTai();

            if (!maTaiKhoan.HasValue)
                return ChuyenDenDangNhap();

            if (id <= 0)
                return RedirectToAction("Index");

            using (var db = new SPORTIXEntities())
            {
                var gioHang = db.GioHangs.FirstOrDefault(
                    x => x.MaTaiKhoan == maTaiKhoan.Value
                );

                if (gioHang == null)
                    return RedirectToAction("Index");

                var chiTiet = db.ChiTietGioHangs.FirstOrDefault(
                    x => x.MaChiTietGioHang == id &&
                         x.MaGioHang == gioHang.MaGioHang
                );

                if (chiTiet == null)
                {
                    TempData["ThongBaoLoi"] =
                        "Không tìm thấy sản phẩm cần xóa.";

                    return RedirectToAction("Index");
                }

                db.ChiTietGioHangs.Remove(chiTiet);

                db.SaveChanges();

                TempData["ThongBao"] =
                    "Đã xóa sản phẩm khỏi giỏ hàng.";

                return RedirectToAction("Index");
            }
        }
    }
}
