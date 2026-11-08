using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using Do_an.Hubs;
using Do_an.Models;
using Microsoft.AspNet.SignalR;

namespace Do_an.Services
{
    public class ThongBaoDto
    {
        public int MaThongBao { get; set; }
        public int? MaDonHang { get; set; }
        public string TieuDe { get; set; }
        public string NoiDung { get; set; }
        public bool DaDoc { get; set; }
        public string NgayTao { get; set; }
    }

    public class ThongBaoService
    {
        private static readonly MemoryCache boNhoToken = new MemoryCache("sportix-hub-token");

        // ---------- Token kết nối SignalR ----------

        public static string TaoToken(int maAdmin)
        {
            string token = Guid.NewGuid().ToString("N");

            boNhoToken.Set(
                token,
                maAdmin,
                new CacheItemPolicy { SlidingExpiration = TimeSpan.FromHours(12) });

            return token;
        }

        public static int? LayMaAdminTuToken(string token)
        {
            if (string.IsNullOrEmpty(token)) return null;

            object giaTri = boNhoToken.Get(token);

            return giaTri is int ? (int?)(int)giaTri : null;
        }

        // ---------- Gọi từ TV3 sau khi lưu đơn hàng thành công ----------
        // Ghi 1 dòng ThongBao cho MỖI admin đang hoạt động rồi đẩy qua SignalR.
        // Lỗi SignalR không được làm hỏng việc đặt hàng nên được nuốt lại.
        public void ThongBaoDonMoi(int maDonHang)
        {
            var danhSachDay = new List<Tuple<int, ThongBaoDto>>();

            using (var db = new SPORTIXEntities())
            {
                DonHang don = db.DonHangs
                    .AsNoTracking()
                    .FirstOrDefault(x => x.MaDonHang == maDonHang);

                if (don == null) return;

                int[] admins = db.TaiKhoans
                    .Where(x => x.VaiTro == XacThucService.VaiTroAdmin && x.HoatDong)
                    .Select(x => x.MaTaiKhoan)
                    .ToArray();

                string tieuDe = "Đơn hàng mới #" + don.MaDonHang;
                string noiDung = string.Format(
                    "{0} vừa đặt đơn trị giá {1:N0}đ.",
                    don.HoTenNguoiNhan,
                    don.TongTien ?? (don.TienHang - don.TienGiam + don.PhiVanChuyen));

                DateTime bayGio = DateTime.Now;

                var cacDong = admins.Select(ma => new ThongBao
                {
                    MaTaiKhoan = ma,
                    MaDonHang = don.MaDonHang,
                    TieuDe = tieuDe,
                    NoiDung = noiDung,
                    DaDoc = false,
                    NgayTao = bayGio
                }).ToList();

                db.ThongBaos.AddRange(cacDong);
                db.SaveChanges();

                foreach (var dong in cacDong)
                {
                    danhSachDay.Add(Tuple.Create(dong.MaTaiKhoan, Dto(dong)));
                }
            }

            try
            {
                var hub = GlobalHost.ConnectionManager.GetHubContext<ThongBaoHub>();

                foreach (var item in danhSachDay)
                {
                    hub.Clients.Group(ThongBaoHub.TenNhom(item.Item1))
                        .nhanThongBao(item.Item2, DemChuaDoc(item.Item1));
                }
            }
            catch (Exception)
            {
                // Thông báo đã nằm trong DB; Admin vẫn thấy khi tải lại.
            }
        }

        // ---------- Phía Admin ----------

        public List<ThongBaoDto> LayDanhSach(int maAdmin, int soLuong)
        {
            using (var db = new SPORTIXEntities())
            {
                return db.ThongBaos
                    .AsNoTracking()
                    .Where(x => x.MaTaiKhoan == maAdmin)
                    .OrderByDescending(x => x.NgayTao)
                    .ThenByDescending(x => x.MaThongBao)
                    .Take(soLuong)
                    .ToList()
                    .Select(Dto)
                    .ToList();
            }
        }

        public static int DemChuaDoc(int maAdmin)
        {
            using (var db = new SPORTIXEntities())
            {
                return db.ThongBaos.Count(x => x.MaTaiKhoan == maAdmin && !x.DaDoc);
            }
        }

        // Chỉ đánh dấu được thông báo của chính mình.
        public bool DanhDauDaDoc(int maAdmin, int maThongBao)
        {
            using (var db = new SPORTIXEntities())
            {
                ThongBao tb = db.ThongBaos.FirstOrDefault(x =>
                    x.MaThongBao == maThongBao && x.MaTaiKhoan == maAdmin);

                if (tb == null) return false;

                if (!tb.DaDoc)
                {
                    tb.DaDoc = true;
                    db.SaveChanges();
                }

                return true;
            }
        }

        public void DanhDauTatCaDaDoc(int maAdmin)
        {
            using (var db = new SPORTIXEntities())
            {
                db.Database.ExecuteSqlCommand(
                    "UPDATE dbo.ThongBao SET DaDoc = 1 WHERE MaTaiKhoan = @p0 AND DaDoc = 0",
                    maAdmin);
            }
        }

        private static ThongBaoDto Dto(ThongBao x)
        {
            return new ThongBaoDto
            {
                MaThongBao = x.MaThongBao,
                MaDonHang = x.MaDonHang,
                TieuDe = x.TieuDe,
                NoiDung = x.NoiDung,
                DaDoc = x.DaDoc,
                NgayTao = x.NgayTao.ToString("dd/MM/yyyy HH:mm")
            };
        }
    }
}
