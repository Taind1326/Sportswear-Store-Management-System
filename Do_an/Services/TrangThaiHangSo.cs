using System.Collections.Generic;

namespace Do_an.Services
{
    // Hằng số trạng thái dùng chung cho đơn hàng, thanh toán, mã giảm giá.
    // Các cột TrangThai/LoaiGiam/PhuongThuc là varchar (không dấu), nên dùng mã ASCII.
    // Nếu script SQL của nhóm đặt giá trị khác, chỉ cần sửa tại file này.
    public static class TrangThaiDonHang
    {
        public const string ChoXacNhan = "ChoXacNhan";
        public const string DaXacNhan = "DaXacNhan";
        public const string DangGiao = "DangGiao";
        public const string DaGiao = "DaGiao";
        public const string DaHuy = "DaHuy";

        public static readonly string[] TatCa =
        {
            ChoXacNhan, DaXacNhan, DangGiao, DaGiao, DaHuy
        };

        private static readonly Dictionary<string, string> TenHienThi =
            new Dictionary<string, string>
            {
                { ChoXacNhan, "Chờ xác nhận" },
                { DaXacNhan, "Đã xác nhận" },
                { DangGiao, "Đang giao" },
                { DaGiao, "Đã giao" },
                { DaHuy, "Đã hủy" }
            };

        public static string Ten(string ma)
        {
            string ten;
            return ma != null && TenHienThi.TryGetValue(ma, out ten) ? ten : ma;
        }

        public static string LopBadge(string ma)
        {
            switch (ma)
            {
                case ChoXacNhan: return "bg-warning text-dark";
                case DaXacNhan: return "bg-info text-dark";
                case DangGiao: return "bg-primary";
                case DaGiao: return "bg-success";
                case DaHuy: return "bg-secondary";
                default: return "bg-light text-dark";
            }
        }

        // Trạng thái kế tiếp hợp lệ (không gồm Đã hủy, hủy đi qua HuyDon).
        public static string TiepTheo(string ma)
        {
            switch (ma)
            {
                case ChoXacNhan: return DaXacNhan;
                case DaXacNhan: return DangGiao;
                case DangGiao: return DaGiao;
                default: return null;
            }
        }

        public static bool ChoPhepHuy(string ma)
        {
            return ma == ChoXacNhan || ma == DaXacNhan;
        }
    }

    public static class TrangThaiThanhToan
    {
        public const string ChuaThanhToan = "ChuaThanhToan";
        public const string DaThanhToan = "DaThanhToan";
        public const string DaHoanTien = "DaHoanTien";

        public const string PhuongThucCod = "COD";
        public const string PhuongThucChuyenKhoan = "ChuyenKhoan";

        public static string Ten(string ma)
        {
            switch (ma)
            {
                case ChuaThanhToan: return "Chưa thanh toán";
                case DaThanhToan: return "Đã thanh toán";
                case DaHoanTien: return "Đã hoàn tiền";
                default: return ma;
            }
        }
    }

    public static class LoaiGiamGia
    {
        public const string PhanTram = "PhanTram";
        public const string SoTien = "SoTien";

        public static string Ten(string ma)
        {
            return ma == PhanTram ? "Giảm theo %" : "Giảm tiền cố định";
        }
    }
}
