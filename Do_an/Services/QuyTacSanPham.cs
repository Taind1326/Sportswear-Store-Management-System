using System;

namespace Do_an.Services
{
    public static class QuyTacSanPham
    {
        public static string ChuanHoa(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "Mặc định";
            }

            return string.Join(
                " ",
                value.Trim().Split(
                    (char[])null,
                    StringSplitOptions.RemoveEmptyEntries
                )
            );
        }

        public static string Khoa(string mau, string kichThuoc)
        {
            string mauChuan = ChuanHoa(mau).ToUpperInvariant();

            string kichThuocChuan =
                ChuanHoa(kichThuoc).ToUpperInvariant();

            return mauChuan.Length
                + ":"
                + mauChuan
                + kichThuocChuan;
        }

        public static decimal GiaSauGiam(
            decimal giaGoc,
            int phanTramGiam)
        {
            if (giaGoc < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(giaGoc),
                    "Giá gốc không được âm."
                );
            }

            if (phanTramGiam < 0 || phanTramGiam > 100)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(phanTramGiam),
                    "Phần trăm giảm phải từ 0 đến 100."
                );
            }

            decimal giaSauGiam =
                giaGoc * (100 - phanTramGiam) / 100m;

            return Math.Round(
                giaSauGiam,
                0,
                MidpointRounding.AwayFromZero
            );
        }

        public static string Nhan(
            int phanTramGiam,
            DateTime ngayTao,
            DateTime thoiDiemHienTai)
        {
            if (phanTramGiam > 0)
            {
                return "Giảm giá";
            }

            bool laHangMoi =
                ngayTao <= thoiDiemHienTai
                && ngayTao > thoiDiemHienTai.AddDays(-30);

            return laHangMoi ? "Hàng mới" : "";
        }
    }
}