using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Do_an.Models;

namespace Do_an.Services
{
    // Hàm dùng chung cho cả nhóm:
    //   XacThucService.LayMaTaiKhoanHienTai()      -> int? (null nếu chưa đăng nhập)
    //   XacThucService.LayTaiKhoanHienTai(db)      -> TaiKhoan hợp lệ hoặc null
    //   XacThucService.LaAdmin(db)                 -> bool
    public static class XacThucService
    {
        public const string KhoaMaTaiKhoan = "MaTaiKhoan"; // TV1 đang đọc key này
        public const string KhoaHoTen = "HoTen";
        public const string KhoaVaiTro = "VaiTro";
        public const string VaiTroAdmin = "Admin";
        public const string VaiTroUser = "User";

        public static int? LayMaTaiKhoanHienTai()
        {
            var session = HttpContext.Current == null
                ? null
                : HttpContext.Current.Session;
            if (session == null) return null;

            int ma;
            if (int.TryParse(Convert.ToString(session[KhoaMaTaiKhoan]), out ma) && ma > 0)
                return ma;

            return null;
        }

        // Trả về tài khoản đang đăng nhập NẾU còn tồn tại và chưa bị khóa.
        // Nếu bị khóa/xóa thì tự đăng xuất và biKhoa = true (trường hợp khóa).
        public static TaiKhoan LayTaiKhoanHienTai(SPORTIXEntities db, out bool biKhoa)
        {
            biKhoa = false;

            int? ma = LayMaTaiKhoanHienTai();
            if (ma == null) return null;

            int maTk = ma.Value;
            TaiKhoan tk = db.TaiKhoans
                .AsNoTracking()
                .FirstOrDefault(x => x.MaTaiKhoan == maTk);

            if (tk == null)
            {
                DangXuat();
                return null;
            }

            if (!tk.HoatDong)
            {
                biKhoa = true;
                DangXuat();
                return null;
            }

            return tk;
        }

        public static TaiKhoan LayTaiKhoanHienTai(SPORTIXEntities db)
        {
            bool biKhoa;
            return LayTaiKhoanHienTai(db, out biKhoa);
        }

        public static bool LaAdmin(SPORTIXEntities db)
        {
            TaiKhoan tk = LayTaiKhoanHienTai(db);
            return tk != null && tk.VaiTro == VaiTroAdmin;
        }

        public static void DangNhap(TaiKhoan tk)
        {
            var session = HttpContext.Current.Session;
            session.Clear();
            session[KhoaMaTaiKhoan] = tk.MaTaiKhoan;
            session[KhoaHoTen] = tk.HoTen;
            session[KhoaVaiTro] = tk.VaiTro;
        }

        public static void CapNhatHoTen(string hoTen)
        {
            if (HttpContext.Current != null && HttpContext.Current.Session != null)
                HttpContext.Current.Session[KhoaHoTen] = hoTen;
        }

        public static void DangXuat()
        {
            if (HttpContext.Current != null && HttpContext.Current.Session != null)
                HttpContext.Current.Session.Clear();
        }

        // Tạo tài khoản Admin demo nếu DB chưa có admin nào.
        public static void TaoAdminMacDinh()
        {
            try
            {
                using (var db = new SPORTIXEntities())
                {
                    if (db.TaiKhoans.Any(x => x.VaiTro == VaiTroAdmin)) return;

                    if (db.TaiKhoans.Any(x =>
                        x.TenDangNhap == "admin" || x.Email == "admin@sportix.vn"))
                        return;

                    db.TaiKhoans.Add(new TaiKhoan
                    {
                        TenDangNhap = "admin",
                        MatKhauHash = MatKhauHelper.BamMatKhau("Admin@123"),
                        HoTen = "Quản trị viên SPORTIX",
                        Email = "admin@sportix.vn",
                        SoDienThoai = null,
                        VaiTro = VaiTroAdmin,
                        HoatDong = true,
                        NgayTao = DateTime.Now
                    });
                    db.SaveChanges();
                }
            }
            catch (Exception)
            {
                // DB chưa sẵn sàng: bỏ qua, không làm sập ứng dụng khi khởi động.
            }
        }
    }

    // Giới hạn tần suất đơn giản trong bộ nhớ (cửa sổ trượt).
    public static class GioiHanTanSuat
    {
        private static readonly ConcurrentDictionary<string, Queue<DateTime>> LichSu =
            new ConcurrentDictionary<string, Queue<DateTime>>();

        // true = cho phép (và ghi nhận lượt này); false = vượt giới hạn.
        public static bool ChoPhep(string khoa, int toiDa, TimeSpan cuaSo)
        {
            DateTime bayGio = DateTime.UtcNow;

            if (LichSu.Count > 5000) DonDep(bayGio, cuaSo);

            Queue<DateTime> hangDoi = LichSu.GetOrAdd(khoa, k => new Queue<DateTime>());
            lock (hangDoi)
            {
                while (hangDoi.Count > 0 && bayGio - hangDoi.Peek() > cuaSo)
                    hangDoi.Dequeue();

                if (hangDoi.Count >= toiDa) return false;

                hangDoi.Enqueue(bayGio);
                return true;
            }
        }

        private static void DonDep(DateTime bayGio, TimeSpan cuaSo)
        {
            foreach (var cap in LichSu.ToArray())
            {
                bool het;
                lock (cap.Value)
                {
                    het = cap.Value.Count == 0
                        || bayGio - cap.Value.Peek() > cuaSo;
                }
                if (het)
                {
                    Queue<DateTime> bo;
                    LichSu.TryRemove(cap.Key, out bo);
                }
            }
        }
    }
}