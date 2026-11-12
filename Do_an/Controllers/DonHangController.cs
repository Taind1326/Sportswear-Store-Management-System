
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Do_an.Models;
using Do_an.Services;
using Do_an.ViewModels;

namespace Do_an.Controllers
{
    [YeuCauDangNhap]
    public partial class DonHangController : Controller
    {
        private readonly DonHangService dichVu = new DonHangService();

        private const decimal TV3PhiVanChuyen = 0m;

        // Mở kết nối trước khi tạo transaction EF5.
        private DbTransaction TV3TaoGiaoDichSerializable(SPORTIXEntities db)
        {
            var ketNoi = ((IObjectContextAdapter)db)
                .ObjectContext.Connection;

            if (ketNoi.State != ConnectionState.Open)
                ketNoi.Open();

            return ketNoi.BeginTransaction(IsolationLevel.Serializable);
        }

        // =====================================================
        // TV4: DANH SÁCH VÀ LỊCH SỬ ĐƠN HÀNG
        // =====================================================

        [HttpGet]
        public ActionResult Index(string trangThai, int trang = 1)
        {
            return LichSu(trangThai, trang);
        }

        [HttpGet]
        public ActionResult LichSu(string trangThai, int trang = 1)
        {
            int? maTaiKhoan = XacThucService.LayMaTaiKhoanHienTai();

            if (!maTaiKhoan.HasValue)
                return RedirectToAction("DangNhap", "TaiKhoan");

            using (var db = new SPORTIXEntities())
            {
                var q = dichVu.TruyVan(
                    db,
                    maTaiKhoan.Value,
                    trangThai,
                    null
                );

                int tong = q.Count();

                int tongTrang = Math.Max(
                    1,
                    (int)Math.Ceiling(
                        tong / (double)DonHangService.KichThuocTrang
                    )
                );

                trang = Math.Max(1, Math.Min(trang, tongTrang));

                var ds = q
                    .OrderByDescending(x => x.NgayDat)
                    .ThenByDescending(x => x.MaDonHang)
                    .Skip(
                        (trang - 1) * DonHangService.KichThuocTrang
                    )
                    .Take(DonHangService.KichThuocTrang)
                    .ToList();

                return View("LichSu", new DanhSachDonHangViewModel
                {
                    DonHangs = ds,
                    TrangThai = trangThai,
                    Trang = trang,
                    TongTrang = tongTrang,
                    TongSo = tong
                });
            }
        }

        // =====================================================
        // TV4: CHI TIẾT ĐƠN HÀNG
        // =====================================================

        [HttpGet]
        public ActionResult ChiTiet(int id)
        {
            int? maTaiKhoan = XacThucService.LayMaTaiKhoanHienTai();

            if (!maTaiKhoan.HasValue)
                return RedirectToAction("DangNhap", "TaiKhoan");

            using (var db = new SPORTIXEntities())
            {
                DonHang don = db.DonHangs
                    .AsNoTracking()
                    .FirstOrDefault(x =>
                        x.MaDonHang == id &&
                        x.MaTaiKhoan == maTaiKhoan.Value
                    );

                if (don == null)
                    return HttpNotFound();

                return View(new ChiTietDonHangViewModel
                {
                    Don = don,

                    Dong = db.ChiTietDonHangs
                        .AsNoTracking()
                        .Where(x => x.MaDonHang == id)
                        .OrderBy(x => x.MaChiTietDonHang)
                        .ToList(),

                    ThanhToans = db.ThanhToans
                        .AsNoTracking()
                        .Where(x => x.MaDonHang == id)
                        .ToList()
                });
            }
        }

        // =====================================================
        // TV4: HỦY ĐƠN
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Huy(int id, string lyDo)
        {
            int? maTaiKhoan = XacThucService.LayMaTaiKhoanHienTai();

            if (!maTaiKhoan.HasValue)
                return RedirectToAction("DangNhap", "TaiKhoan");

            KetQuaHuyDon kq = dichVu.HuyDon(
                id,
                lyDo,
                maTaiKhoan.Value,
                maTaiKhoan.Value
            );

            switch (kq)
            {
                case KetQuaHuyDon.ThanhCong:
                    TempData["ThanhCong"] = "Đã hủy đơn #" + id + ".";
                    break;

                case KetQuaHuyDon.DaHuyTruocDo:
                    TempData["Loi"] = "Đơn hàng này đã được hủy trước đó.";
                    break;

                case KetQuaHuyDon.KhongTheHuy:
                    TempData["Loi"] =
                        "Chỉ hủy được đơn đang ở trạng thái Chờ xác nhận.";
                    break;

                default:
                    return HttpNotFound();
            }

            return RedirectToAction("ChiTiet", new { id = id });
        }

        // =====================================================
        // TV3: KIỂM TRA MÃ GIẢM GIÁ
        // Giá trị tiền hàng gửi từ trình duyệt không được tin cậy.
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult KiemTraMa(string maCode, decimal tienHang)
        {
            int? maTaiKhoan = XacThucService.LayMaTaiKhoanHienTai();

            if (!maTaiKhoan.HasValue)
            {
                return Json(new
                {
                    hopLe = false,
                    loi = "Vui lòng đăng nhập lại.",
                    tienGiam = 0,
                    tienSauGiam = 0
                });
            }

            using (var db = new SPORTIXEntities())
            {
                decimal tienHangThucTe = TV3TinhTienHang(
                    db,
                    maTaiKhoan.Value
                );

                KetQuaMaGiamGia kq =
                    KhuyenMaiService.KiemTraVaTinh(
                        db,
                        maCode,
                        tienHangThucTe
                    );

                return Json(new
                {
                    hopLe = kq.HopLe,
                    loi = kq.LoiHienThi,
                    tienGiam = kq.TienGiam,
                    tienSauGiam = kq.TienSauGiam
                });
            }
        }

        // =====================================================
        // TV3: TRANG ĐẶT HÀNG GET
        // =====================================================

        [HttpGet]
        public ActionResult DatHang()
        {
            int? maTaiKhoan = XacThucService.LayMaTaiKhoanHienTai();

            if (!maTaiKhoan.HasValue)
                return RedirectToAction("DangNhap", "TaiKhoan");

            using (var db = new SPORTIXEntities())
            {
                var model = TV3NapLaiDanhSach(
                    db,
                    maTaiKhoan.Value,
                    new DatHangViewModel()
                );

                if (model.DongHang.Count == 0)
                {
                    TempData["ThongBaoLoi"] = "Giỏ hàng đang trống.";
                    return RedirectToAction("Index", "GioHang");
                }

                model.PhuongThucThanhToan = "COD";

                var diaChi = db.DiaChis
                    .Where(x => x.MaTaiKhoan == maTaiKhoan.Value)
                    .OrderByDescending(x => x.MacDinh)
                    .ThenByDescending(x => x.MaDiaChi)
                    .FirstOrDefault();

                if (diaChi != null)
                {
                    model.HoTenNguoiNhan = diaChi.HoTenNguoiNhan;
                    model.SoDienThoai = diaChi.SoDienThoai;
                    model.TinhThanh = diaChi.TinhThanh;
                    model.PhuongXa = diaChi.PhuongXa;
                    model.DiaChiCuThe = diaChi.DiaChiCuThe;
                }

                return View("DatHang", model);
            }
        }

        // =====================================================
        // TV3: XỬ LÝ ĐẶT HÀNG COD / CHUYỂN KHOẢN
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DatHang(DatHangViewModel model)
        {
            int? maTaiKhoan = XacThucService.LayMaTaiKhoanHienTai();

            if (!maTaiKhoan.HasValue)
                return RedirectToAction("DangNhap", "TaiKhoan");

            if (model == null)
                model = new DatHangViewModel();

            string phuongThuc = (model.PhuongThucThanhToan ?? "")
                .Trim();

            if (phuongThuc != "COD" && phuongThuc != "ChuyenKhoan")
            {
                ModelState.AddModelError(
                    "PhuongThucThanhToan",
                    "Phương thức thanh toán không hợp lệ."
                );
            }

            if (!ModelState.IsValid)
            {
                using (var db = new SPORTIXEntities())
                {
                    TV3NapLaiDanhSach(db, maTaiKhoan.Value, model);
                    return View("DatHang", model);
                }
            }

            int maDonHangMoi = 0;

            try
            {
                using (var db = new SPORTIXEntities())
                using (var giaoDich = TV3TaoGiaoDichSerializable(db))
                {
                    var gioHang = db.GioHangs.FirstOrDefault(
                        x => x.MaTaiKhoan == maTaiKhoan.Value
                    );

                    if (gioHang == null)
                        throw new InvalidOperationException(
                            "Không tìm thấy giỏ hàng."
                        );

                    var danhSachGioHang = db.ChiTietGioHangs
                        .Include("BienTheSanPham.SanPham")
                        .Where(x => x.MaGioHang == gioHang.MaGioHang)
                        .ToList();

                    if (danhSachGioHang.Count == 0)
                        throw new InvalidOperationException(
                            "Giỏ hàng đang trống."
                        );

                    decimal tienHang = 0m;

                    // 1. Kiểm tra sản phẩm, trạng thái, số lượng.
                    foreach (var dong in danhSachGioHang)
                    {
                        var bienThe = dong.BienTheSanPham;

                        if (bienThe == null || bienThe.SanPham == null)
                            throw new InvalidOperationException(
                                "Có sản phẩm trong giỏ không còn tồn tại."
                            );

                        var sanPham = bienThe.SanPham;

                        if (!bienThe.DangBan || !sanPham.DangBan)
                            throw new InvalidOperationException(
                                "Sản phẩm " + sanPham.TenSanPham +
                                " hiện không còn được kinh doanh."
                            );

                        if (dong.SoLuong <= 0 ||
                            dong.SoLuong > bienThe.SoLuongTon)
                        {
                            throw new InvalidOperationException(
                                "Sản phẩm " + sanPham.TenSanPham +
                                " không đủ số lượng tồn kho."
                            );
                        }

                        decimal donGia = QuyTacSanPham.GiaSauGiam(
                            bienThe.GiaBan,
                            sanPham.PhanTramGiam
                        );

                        tienHang += donGia * dong.SoLuong;
                    }

                    // 2. Kiểm tra mã giảm giá bằng service dùng chung.
                    decimal tienGiam = 0m;
                    int? maGiamGia = null;
                    string maCodeDaDung = null;

                    string maCode = (model.MaCode ?? "")
                        .Trim()
                        .ToUpperInvariant();

                    if (maCode.Length > 0)
                    {
                        KetQuaMaGiamGia kq =
                            KhuyenMaiService.KiemTraVaTinh(
                                db,
                                maCode,
                                tienHang
                            );

                        if (!kq.HopLe)
                            throw new InvalidOperationException(
                                kq.LoiHienThi
                            );

                        tienGiam = kq.TienGiam;
                        maGiamGia = kq.MaGiamGia;
                        maCodeDaDung = kq.MaCode;

                        // Tăng lượt dùng bằng UPDATE có điều kiện.
                        int soDongCapNhat = db.Database.ExecuteSqlCommand(
                            @"UPDATE dbo.MaGiamGia
                              SET SoLuotDaDung = SoLuotDaDung + 1
                              WHERE MaGiamGia = @p0
                                AND HoatDong = 1
                                AND SoLuotDaDung < SoLuotToiDa
                                AND NgayBatDau <= @p1
                                AND NgayKetThuc >= @p1
                                AND DonToiThieu <= @p2",
                            maGiamGia.Value,
                            DateTime.Now,
                            tienHang
                        );

                        if (soDongCapNhat != 1)
                            throw new InvalidOperationException(
                                "Mã giảm giá vừa hết lượt hoặc không còn hiệu lực."
                            );
                    }

                    if (tienGiam < 0)
                        tienGiam = 0;

                    if (tienGiam > tienHang)
                        tienGiam = tienHang;

                    decimal phiVanChuyen = TV3PhiVanChuyen;

                    decimal tongTien =
                        tienHang - tienGiam + phiVanChuyen;

                    // 3. Tạo đơn hàng.
                    var donHang = new DonHang
                    {
                        MaTaiKhoan = maTaiKhoan.Value,
                        MaGiamGia = maGiamGia,
                        MaCodeDaDung = maCodeDaDung,
                        NgayDat = DateTime.Now,

                        HoTenNguoiNhan = model.HoTenNguoiNhan.Trim(),
                        SoDienThoai = model.SoDienThoai.Trim(),
                        TinhThanh = model.TinhThanh.Trim(),
                        PhuongXa = model.PhuongXa.Trim(),
                        DiaChiCuThe = model.DiaChiCuThe.Trim(),

                        TienHang = tienHang,
                        TienGiam = tienGiam,
                        PhiVanChuyen = phiVanChuyen,
                        TrangThai = "ChoXacNhan",

                        GhiChu = string.IsNullOrWhiteSpace(model.GhiChu)
                            ? null
                            : model.GhiChu.Trim()
                    };

                    db.DonHangs.Add(donHang);
                    db.SaveChanges();

                    // Mã chuyển khoản gắn duy nhất với đơn.
                    string maThanhToan = phuongThuc == "ChuyenKhoan"
                        ? "SPX" + donHang.MaDonHang.ToString("D8")
                        : null;

                    // 4. Tạo chi tiết đơn và trừ tồn kho.
                    foreach (var dong in danhSachGioHang)
                    {
                        var bienThe = dong.BienTheSanPham;
                        var sanPham = bienThe.SanPham;

                        decimal donGia = QuyTacSanPham.GiaSauGiam(
                            bienThe.GiaBan,
                            sanPham.PhanTramGiam
                        );

                        db.ChiTietDonHangs.Add(new ChiTietDonHang
                        {
                            MaDonHang = donHang.MaDonHang,
                            MaBienThe = bienThe.MaBienThe,
                            TenSanPham = sanPham.TenSanPham,
                            SKU = bienThe.SKU,
                            KichThuoc = bienThe.KichThuoc,
                            MauSac = bienThe.MauSac,
                            SoLuong = dong.SoLuong,
                            GiaGoc = bienThe.GiaBan,
                            PhanTramGiam = sanPham.PhanTramGiam,
                            DonGia = donGia
                        });

                        // UPDATE có điều kiện để chống bán vượt tồn kho.
                        int soDongCapNhat = db.Database.ExecuteSqlCommand(
                            @"UPDATE bt
                              SET bt.SoLuongTon = bt.SoLuongTon - @p0
                              FROM dbo.BienTheSanPham AS bt
                              INNER JOIN dbo.SanPham AS sp
                                  ON sp.MaSanPham = bt.MaSanPham
                              WHERE bt.MaBienThe = @p1
                                AND bt.SoLuongTon >= @p0
                                AND bt.DangBan = 1
                                AND sp.DangBan = 1",
                            dong.SoLuong,
                            bienThe.MaBienThe
                        );

                        if (soDongCapNhat != 1)
                            throw new InvalidOperationException(
                                "Tồn kho của sản phẩm " +
                                sanPham.TenSanPham +
                                " vừa thay đổi. Vui lòng thử lại."
                            );
                    }

                    // 5. Tạo bản ghi thanh toán.
                    db.ThanhToans.Add(new ThanhToan
                    {
                        MaDonHang = donHang.MaDonHang,
                        PhuongThuc = phuongThuc,
                        SoTien = tongTien,
                        TrangThai = "ChuaThanhToan",
                        MaThamChieu = maThanhToan,
                        NgayTao = DateTime.Now,
                        GhiChu = phuongThuc == "ChuyenKhoan"
                            ? "Chờ xác nhận chuyển khoản SePay."
                            : null
                    });

                    // 6. Xóa các dòng giỏ hàng.
                    foreach (var dong in danhSachGioHang)
                    {
                        db.ChiTietGioHangs.Remove(dong);
                    }

                    db.SaveChanges();

                    maDonHangMoi = donHang.MaDonHang;

                    giaoDich.Commit();
                }

                TempData["ThongBao"] =
                    "Đã tạo đơn hàng #" + maDonHangMoi + ".";

                if (phuongThuc == "ChuyenKhoan")
                {
                    return RedirectToAction(
                        "ThanhToanSePay",
                        new { id = maDonHangMoi }
                    );
                }

                return RedirectToAction(
                    "ThanhCong",
                    new { id = maDonHangMoi }
                );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);

                ModelState.AddModelError(
                    "",
                    "Không thể tạo đơn hàng. Vui lòng kiểm tra giỏ hàng, tồn kho và mã giảm giá."
                );
            }

            using (var dbTaiLai = new SPORTIXEntities())
            {
                TV3NapLaiDanhSach(
                    dbTaiLai,
                    maTaiKhoan.Value,
                    model
                );
            }

            return View("DatHang", model);
        }

        // =====================================================
        // TV3: TRANG THANH TOÁN SEPAY - SACOMBANK
        // =====================================================

        [HttpGet]
        public async Task<ActionResult> ThanhToanSePay(int id)
        {
            int? maTaiKhoan =
                XacThucService.LayMaTaiKhoanHienTai();

            if (!maTaiKhoan.HasValue)
                return RedirectToAction("DangNhap", "TaiKhoan");

            using (var db = new SPORTIXEntities())
            {
                var donHang = db.DonHangs.AsNoTracking()
                    .FirstOrDefault(x =>
                        x.MaDonHang == id &&
                        x.MaTaiKhoan == maTaiKhoan.Value
                    );

                if (donHang == null)
                    return HttpNotFound();

                // Không dùng AsNoTracking vì cần lưu metadata SePay.
                var thanhToan = db.ThanhToans
                    .FirstOrDefault(x =>
                        x.MaDonHang == id &&
                        x.PhuongThuc == "ChuyenKhoan"
                    );

                if (thanhToan == null)
                    return HttpNotFound();

                // Không tạo VA cho đơn đã hủy.
                if (donHang.TrangThai == "DaHuy")
                {
                    ViewBag.LoiSePay =
                        "Đơn hàng đã bị hủy, không thể tiếp tục thanh toán.";
                }
                // Chỉ tạo VA khi chưa có metadata SePay.
                else if (
                    thanhToan.TrangThai == "ChuaThanhToan" &&
                    !SePayApiService.CoMetadata(thanhToan.GhiChu))
                {
                    try
                    {
                        var sePayService = new SePayApiService();

                        SePayTaoDonHangResult ketQua =
                            await sePayService.TaoDonHangAsync(
                                thanhToan.MaThamChieu,
                                thanhToan.SoTien
                            );

                        thanhToan.GhiChu =
                            SePayApiService.MaHoaThongTinThanhToan(
                                ketQua
                            );

                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex);

                        ViewBag.LoiSePay =
                            "Không thể tạo mã thanh toán Sacombank. " +
                            "Vui lòng thử tải lại trang hoặc liên hệ quản trị viên.";
                    }
                }

                // Đọc metadata đã lưu.
                var metadata =
                    SePayApiService.DocThongTinThanhToan(
                        thanhToan.GhiChu
                    );

                // Chỉ hiển thị QR cho khoản thanh toán còn chờ.
                if (donHang.TrangThai != "DaHuy" &&
                    thanhToan.TrangThai == "ChuaThanhToan")
                {
                    if (metadata != null)
                    {
                        bool metadataHopLe =
                            string.Equals(
                                metadata.OrderCode,
                                thanhToan.MaThamChieu,
                                StringComparison.OrdinalIgnoreCase
                            ) &&
                            metadata.Amount > 0 &&
                            (decimal)metadata.Amount == thanhToan.SoTien &&
                            !string.IsNullOrWhiteSpace(metadata.QrCodeUrl) &&
                            !string.IsNullOrWhiteSpace(metadata.VaNumber);

                        if (metadataHopLe)
                        {
                            ViewBag.QRUrl = metadata.QrCodeUrl;
                            ViewBag.BankName = metadata.BankName;
                            ViewBag.VaNumber = metadata.VaNumber;
                            ViewBag.AccountNumber = metadata.AccountNumber;
                            ViewBag.AccountHolderName =
                                metadata.AccountHolderName;
                            ViewBag.ExpiredAt = metadata.ExpiredAt;
                        }
                        else
                        {
                            ViewBag.LoiSePay =
                                "Thông tin thanh toán SePay không khớp với đơn hàng.";
                        }
                    }
                    else if (ViewBag.LoiSePay == null)
                    {
                        ViewBag.LoiSePay =
                            "Chưa có thông tin VA thanh toán. Vui lòng thử tải lại trang.";
                    }
                }

                ViewBag.KiemTraUrl = Url.Action(
                    "KiemTraThanhToan",
                    "DonHang",
                    new { id = id }
                );

                ViewBag.ThanhCongUrl = Url.Action(
                    "ThanhCong",
                    "DonHang",
                    new { id = id }
                );

                return View("ThanhToanSePay", thanhToan);
            }
        }

        // =====================================================
        // TV3: KIỂM TRA TRẠNG THÁI THANH TOÁN
        // =====================================================

        [HttpGet]
        public ActionResult KiemTraThanhToan(int id)
        {
            int? maTaiKhoan = XacThucService.LayMaTaiKhoanHienTai();

            if (!maTaiKhoan.HasValue)
                return Json(new
                {
                    hopLe = false,
                    daThanhToan = false
                }, JsonRequestBehavior.AllowGet);

            using (var db = new SPORTIXEntities())
            {
                var donHang = db.DonHangs.AsNoTracking()
                    .FirstOrDefault(x =>
                        x.MaDonHang == id &&
                        x.MaTaiKhoan == maTaiKhoan.Value
                    );

                if (donHang == null)
                    return HttpNotFound();

                var thanhToan = db.ThanhToans.AsNoTracking()
                    .FirstOrDefault(x => x.MaDonHang == id);

                if (thanhToan == null)
                    return HttpNotFound();

                return Json(new
                {
                    hopLe = true,
                    daThanhToan = thanhToan.TrangThai == "DaThanhToan",
                    trangThai = thanhToan.TrangThai
                }, JsonRequestBehavior.AllowGet);
            }
        }

        // =====================================================
        // TV3: TRANG ĐẶT HÀNG THÀNH CÔNG
        // =====================================================

        [HttpGet]
        public ActionResult ThanhCong(int id)
        {
            int? maTaiKhoan = XacThucService.LayMaTaiKhoanHienTai();

            if (!maTaiKhoan.HasValue)
                return RedirectToAction("DangNhap", "TaiKhoan");

            using (var db = new SPORTIXEntities())
            {
                var donHang = db.DonHangs.AsNoTracking()
                    .FirstOrDefault(x =>
                        x.MaDonHang == id &&
                        x.MaTaiKhoan == maTaiKhoan.Value
                    );

                if (donHang == null)
                    return HttpNotFound();

                ViewBag.ThanhToan = db.ThanhToans
                    .AsNoTracking()
                    .FirstOrDefault(x => x.MaDonHang == id);

                return View("ThanhCong", donHang);
            }
        }

        // =====================================================
        // TV3: TÍNH TIỀN HÀNG TỪ GIỎ HÀNG
        // =====================================================

        private decimal TV3TinhTienHang(
            SPORTIXEntities db,
            int maTaiKhoan)
        {
            var gioHang = db.GioHangs.FirstOrDefault(
                x => x.MaTaiKhoan == maTaiKhoan
            );

            if (gioHang == null)
                return 0m;

            var danhSach = db.ChiTietGioHangs
                .Include("BienTheSanPham.SanPham")
                .Where(x => x.MaGioHang == gioHang.MaGioHang)
                .ToList();

            decimal tong = 0m;

            foreach (var dong in danhSach)
            {
                if (dong.BienTheSanPham == null ||
                    dong.BienTheSanPham.SanPham == null)
                    continue;

                decimal donGia = QuyTacSanPham.GiaSauGiam(
                    dong.BienTheSanPham.GiaBan,
                    dong.BienTheSanPham.SanPham.PhanTramGiam
                );

                tong += donGia * dong.SoLuong;
            }

            return tong;
        }

        // =====================================================
        // TV3: NẠP DỮ LIỆU GIỎ HÀNG VÀO VIEWMODEL
        // =====================================================

        private DatHangViewModel TV3NapLaiDanhSach(
            SPORTIXEntities db,
            int maTaiKhoan,
            DatHangViewModel model)
        {
            if (model == null)
                model = new DatHangViewModel();

            model.DongHang = new List<DatHangDongViewModel>();
            model.TongSoLuong = 0;
            model.TienHang = 0m;
            model.TienGiam = 0m;
            model.PhiVanChuyen = TV3PhiVanChuyen;
            model.TongTien = 0m;

            var gioHang = db.GioHangs.FirstOrDefault(
                x => x.MaTaiKhoan == maTaiKhoan
            );

            if (gioHang == null)
                return model;

            var danhSach = db.ChiTietGioHangs
                .Include("BienTheSanPham.SanPham.HinhAnhSanPhams")
                .Where(x => x.MaGioHang == gioHang.MaGioHang)
                .ToList();

            foreach (var dong in danhSach)
            {
                var bienThe = dong.BienTheSanPham;

                if (bienThe == null || bienThe.SanPham == null)
                    continue;

                var sanPham = bienThe.SanPham;

                var hinhAnh = sanPham.HinhAnhSanPhams
                    .OrderByDescending(x => x.AnhDaiDien)
                    .ThenBy(x => x.ThuTu)
                    .FirstOrDefault();

                decimal donGia = QuyTacSanPham.GiaSauGiam(
                    bienThe.GiaBan,
                    sanPham.PhanTramGiam
                );

                var item = new DatHangDongViewModel
                {
                    MaBienThe = bienThe.MaBienThe,
                    TenSanPham = sanPham.TenSanPham,
                    SKU = bienThe.SKU,
                    MauSac = bienThe.MauSac,
                    KichThuoc = bienThe.KichThuoc,

                    DuongDanAnh = hinhAnh == null
                        ? null
                        : hinhAnh.DuongDanAnh,

                    SoLuong = dong.SoLuong,
                    SoLuongTon = bienThe.SoLuongTon,
                    GiaGoc = bienThe.GiaBan,
                    DonGia = donGia,
                    PhanTramGiam = sanPham.PhanTramGiam
                };

                model.DongHang.Add(item);
                model.TongSoLuong += dong.SoLuong;
                model.TienHang += item.ThanhTien;
            }

            if (!string.IsNullOrWhiteSpace(model.MaCode) &&
                model.TienHang > 0)
            {
                KetQuaMaGiamGia kq =
                    KhuyenMaiService.KiemTraVaTinh(
                        db,
                        model.MaCode,
                        model.TienHang
                    );

                if (kq.HopLe)
                    model.TienGiam = kq.TienGiam;
            }

            model.TongTien =
                model.TienHang -
                model.TienGiam +
                model.PhiVanChuyen;

            return model;
        }
    }
}
