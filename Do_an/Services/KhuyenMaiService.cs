using System;
using System.Linq;
using Do_an.Models;

namespace Do_an.Services
{
    public class KetQuaMaGiamGia
    {
        public bool HopLe { get; set; }
        public string LoiHienThi { get; set; }
        public int MaGiamGia { get; set; }
        public string MaCode { get; set; }
        public decimal TienGiam { get; set; }
        public decimal TienSauGiam { get; set; }
    }

    // Hàm kiểm tra + tính tiền giảm DÙNG CHUNG toàn hệ thống.
    // TV3 gọi KhuyenMaiService.KiemTraVaTinh(maCode, tienHang) cả khi khách nhập mã
    // lẫn khi đặt hàng; không tự tính ở trình duyệt.
    public class KhuyenMaiService
    {
        public static KetQuaMaGiamGia KiemTraVaTinh(
            SPORTIXEntities db, string maCode, decimal tienHang)
        {
            maCode = (maCode ?? "").Trim().ToUpperInvariant();

            if (maCode.Length == 0)
                return Loi("Vui lòng nhập mã giảm giá.");

            if (tienHang <= 0)
                return Loi("Giỏ hàng chưa có sản phẩm để áp dụng mã.");

            MaGiamGia ma = db.MaGiamGias
                .AsNoTracking()
                .FirstOrDefault(x => x.MaCode == maCode);

            return KiemTra(ma, tienHang, DateTime.Now);
        }

        public static KetQuaMaGiamGia KiemTraVaTinh(string maCode, decimal tienHang)
        {
            using (var db = new SPORTIXEntities())
            {
                return KiemTraVaTinh(db, maCode, tienHang);
            }
        }

        // Tách riêng để TV3 dùng lại khi đã có entity (vd. trong transaction đặt hàng).
        public static KetQuaMaGiamGia KiemTra(
            MaGiamGia ma, decimal tienHang, DateTime bayGio)
        {
            if (ma == null)
                return Loi("Mã giảm giá không tồn tại.");

            if (!ma.HoatDong)
                return Loi("Mã giảm giá hiện đang tắt.");

            if (bayGio < ma.NgayBatDau)
                return Loi("Mã giảm giá chưa đến thời gian áp dụng.");

            if (bayGio > ma.NgayKetThuc)
                return Loi("Mã giảm giá đã hết hạn.");

            if (tienHang < ma.DonToiThieu)
                return Loi(string.Format(
                    "Đơn hàng cần tối thiểu {0:N0}đ để dùng mã này.",
                    ma.DonToiThieu));

            if (ma.SoLuotDaDung >= ma.SoLuotToiDa)
                return Loi("Mã giảm giá đã hết lượt sử dụng.");

            decimal giam;

            if (ma.LoaiGiam == LoaiGiamGia.PhanTram)
            {
                giam = Math.Round(
                    tienHang * ma.GiaTriGiam / 100m,
                    0,
                    MidpointRounding.AwayFromZero);

                if (ma.GiamToiDa.HasValue && giam > ma.GiamToiDa.Value)
                    giam = ma.GiamToiDa.Value;
            }
            else
            {
                giam = ma.GiaTriGiam;
            }

            // Không bao giờ giảm quá tiền hàng.
            if (giam > tienHang) giam = tienHang;
            if (giam < 0) giam = 0;

            return new KetQuaMaGiamGia
            {
                HopLe = true,
                MaGiamGia = ma.MaGiamGia1,
                MaCode = ma.MaCode,
                TienGiam = giam,
                TienSauGiam = tienHang - giam
            };
        }

        private static KetQuaMaGiamGia Loi(string thongBao)
        {
            return new KetQuaMaGiamGia { HopLe = false, LoiHienThi = thongBao };
        }
    }
}
