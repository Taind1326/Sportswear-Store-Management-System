using System;
using System.Security.Cryptography;

namespace Do_an.Services
{
    // Băm mật khẩu bằng PBKDF2-SHA256 có salt ngẫu nhiên.
    // Định dạng lưu: PBKDF2$soVong$salt(base64)$hash(base64)  (< 500 ký tự)
    public static class MatKhauHelper
    {
        private const string TienTo = "PBKDF2";
        private const int SoVongLap = 100000;
        private const int DoDaiSalt = 16;
        private const int DoDaiHash = 32;

        public static string BamMatKhau(string matKhau)
        {
            if (matKhau == null) throw new ArgumentNullException(nameof(matKhau));

            byte[] salt = new byte[DoDaiSalt];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            byte[] hash = TinhHash(matKhau, salt, SoVongLap, DoDaiHash);

            return TienTo + "$" + SoVongLap + "$"
                + Convert.ToBase64String(salt) + "$"
                + Convert.ToBase64String(hash);
        }

        public static bool KiemTra(string matKhau, string chuoiHash)
        {
            if (string.IsNullOrEmpty(matKhau) || string.IsNullOrEmpty(chuoiHash))
                return false;

            string[] phan = chuoiHash.Split('$');
            if (phan.Length != 4 || phan[0] != TienTo) return false;

            int soVong;
            if (!int.TryParse(phan[1], out soVong) || soVong < 1000) return false;

            byte[] salt;
            byte[] hashDung;
            try
            {
                salt = Convert.FromBase64String(phan[2]);
                hashDung = Convert.FromBase64String(phan[3]);
            }
            catch (FormatException)
            {
                return false;
            }

            byte[] hashThu = TinhHash(matKhau, salt, soVong, hashDung.Length);
            return SoSanhHangSo(hashDung, hashThu);
        }

        private static byte[] TinhHash(string matKhau, byte[] salt, int soVong, int doDai)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(
                matKhau, salt, soVong, HashAlgorithmName.SHA256))
            {
                return pbkdf2.GetBytes(doDai);
            }
        }

        // So sánh thời gian không đổi để tránh timing attack.
        private static bool SoSanhHangSo(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            int khac = 0;
            for (int i = 0; i < a.Length; i++) khac |= a[i] ^ b[i];
            return khac == 0;
        }
    }
}