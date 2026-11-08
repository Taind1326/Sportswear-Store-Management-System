using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Do_an.Models;

namespace Do_an.Services
{
    public enum KetQuaHuyDon
    {
        ThanhCong,
        KhongTimThay,
        KhongCoQuyen,
        KhongTheHuy,
        DaHuyTruocDo
    }

    public class DonHangService
    {
        public const int KichThuocTrang = 10;

        // ---------- Danh sách / chi tiết ----------

        // maTaiKhoan != null: chỉ lấy đơn của tài khoản đó (phía khách).
        public IQueryable<DonHang> TruyVan(
            SPORTIXEntities db, int? maTaiKhoan, string trangThai, string tuKhoa)
        {
            IQueryable<DonHang> q = db.DonHangs.AsNoTracking();

            if (maTaiKhoan.HasValue)
                q = q.Where(x => x.MaTaiKhoan == maTaiKhoan.Value);

            if (!string.IsNullOrEmpty(trangThai) &&
                TrangThaiDonHang.TatCa.Contains(trangThai))
            {
                q = q.Where(x => x.TrangThai == trangThai);
            }

            tuKhoa = (tuKhoa ?? "").Trim().TrimStart('#');

            int ma;
            if (tuKhoa.Length > 0 && int.TryParse(tuKhoa, out ma))
                q = q.Where(x => x.MaDonHang == ma);
            else if (tuKhoa.Length > 0)
                q = q.Where(x => false); // mã đơn là số; chuỗi khác không khớp đơn nào

            return q;
        }

        // ---------- Cập nhật trạng thái (Admin) ----------

        // Chỉ cho đi tới đúng bước kế tiếp: Chờ xác nhận -> Đã xác nhận -> Đang giao -> Đã giao.
        // Dùng UPDATE có điều kiện trạng thái cũ để hai Admin bấm cùng lúc không nhảy bước.
        public bool ChuyenBuocKeTiep(int maDonHang, string trangThaiHienTai, out string thongBao)
        {
            string tiepTheo = TrangThaiDonHang.TiepTheo(trangThaiHienTai);

            if (tiepTheo == null)
            {
                thongBao = "Đơn hàng không thể chuyển sang bước tiếp theo.";
                return false;
            }

            using (var db = new SPORTIXEntities())
            {
                int dong = db.Database.ExecuteSqlCommand(
                    @"UPDATE dbo.DonHang
                      SET TrangThai = @p0, NgayCapNhat = SYSDATETIME()
                      WHERE MaDonHang = @p1 AND TrangThai = @p2",
                    tiepTheo, maDonHang, trangThaiHienTai);

                if (dong == 0)
                {
                    thongBao = "Trạng thái đơn đã thay đổi. Vui lòng tải lại trang.";
                    return false;
                }
            }

            thongBao = "Đã chuyển đơn #" + maDonHang + " sang \"" +
                       TrangThaiDonHang.Ten(tiepTheo) + "\".";
            return true;
        }

        // Xác nhận đã thu tiền COD (đơn đã giao, khoản thanh toán còn chờ).
        public bool XacNhanThuTienCod(int maDonHang, int maAdmin, out string thongBao)
        {
            using (var db = new SPORTIXEntities())
            {
                DonHang don = db.DonHangs.FirstOrDefault(x => x.MaDonHang == maDonHang);

                if (don == null)
                {
                    thongBao = "Không tìm thấy đơn hàng.";
                    return false;
                }

                if (don.TrangThai != TrangThaiDonHang.DaGiao)
                {
                    thongBao = "Chỉ xác nhận thu tiền COD khi đơn đã giao.";
                    return false;
                }

                ThanhToan tt = db.ThanhToans.FirstOrDefault(x =>
                    x.MaDonHang == maDonHang &&
                    x.PhuongThuc == TrangThaiThanhToan.PhuongThucCod &&
                    x.TrangThai == TrangThaiThanhToan.ChuaThanhToan);

                if (tt == null)
                {
                    thongBao = "Đơn này không có khoản COD nào đang chờ thu tiền.";
                    return false;
                }

                tt.TrangThai = TrangThaiThanhToan.DaThanhToan;
                tt.NgayThanhToan = DateTime.Now;
                tt.MaNguoiXacNhan = maAdmin;
                db.SaveChanges();
            }

            thongBao = "Đã xác nhận thu tiền COD cho đơn #" + maDonHang + ".";
            return true;
        }

        // ---------- Hủy đơn ----------
        // Một transaction SERIALIZABLE duy nhất:
        //   1) khóa dòng đơn + kiểm tra quyền/trạng thái  2) đổi trạng thái + lý do
        //   3) cộng lại tồn kho đúng biến thể (ghi LichSuTonKho)  4) trả 1 lượt mã
        //   5) hoàn tiền mô phỏng nếu đã thanh toán.
        // Bất kỳ bước nào lỗi => rollback toàn bộ. Gọi lần 2 trả DaHuyTruocDo, không cộng lại.
        // maTaiKhoanSoHuu != null: phía khách, chỉ hủy được đơn của chính mình.
        public KetQuaHuyDon HuyDon(
            int maDonHang, string lyDo, int maNguoiThucHien, int? maTaiKhoanSoHuu)
        {
            lyDo = (lyDo ?? "").Trim();
            if (lyDo.Length == 0) lyDo = "Không nêu lý do";
            if (lyDo.Length > 500) lyDo = lyDo.Substring(0, 500);

            using (var db = new SPORTIXEntities())
            {
                var ketNoi = db.Database.Connection as SqlConnection;
                if (ketNoi == null)
                    throw new InvalidOperationException("Không kết nối được SQL Server.");

                ketNoi.Open();

                using (var gd = ketNoi.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        KetQuaHuyDon kq = ThucHienHuy(
                            ketNoi, gd, maDonHang, lyDo, maNguoiThucHien, maTaiKhoanSoHuu);

                        if (kq == KetQuaHuyDon.ThanhCong) gd.Commit();
                        else gd.Rollback();

                        return kq;
                    }
                    catch
                    {
                        gd.Rollback();
                        throw;
                    }
                }
            }
        }

        private static KetQuaHuyDon ThucHienHuy(
            SqlConnection kn, SqlTransaction gd,
            int maDonHang, string lyDo, int maNguoiThucHien, int? maTaiKhoanSoHuu)
        {
            int maChuDon;
            string trangThai;
            int? maGiamGia;

            using (var lenh = TaoLenh(kn, gd,
                @"SELECT MaTaiKhoan, TrangThai, MaGiamGia
                  FROM dbo.DonHang WITH (UPDLOCK, HOLDLOCK)
                  WHERE MaDonHang = @ma"))
            {
                lenh.Parameters.Add("@ma", SqlDbType.Int).Value = maDonHang;

                using (var rd = lenh.ExecuteReader())
                {
                    if (!rd.Read()) return KetQuaHuyDon.KhongTimThay;

                    maChuDon = rd.GetInt32(0);
                    trangThai = rd.GetString(1);
                    maGiamGia = rd.IsDBNull(2) ? (int?)null : rd.GetInt32(2);
                }
            }

            if (maTaiKhoanSoHuu.HasValue && maTaiKhoanSoHuu.Value != maChuDon)
                return KetQuaHuyDon.KhongCoQuyen;

            if (trangThai == TrangThaiDonHang.DaHuy)
                return KetQuaHuyDon.DaHuyTruocDo;

            // Khách chỉ được hủy khi Chờ xác nhận; Admin hủy được cả Đã xác nhận.
            bool duocHuy = maTaiKhoanSoHuu.HasValue
                ? trangThai == TrangThaiDonHang.ChoXacNhan
                : TrangThaiDonHang.ChoPhepHuy(trangThai);

            if (!duocHuy) return KetQuaHuyDon.KhongTheHuy;

            // 1-2) đổi trạng thái + lý do
            using (var lenh = TaoLenh(kn, gd,
                @"UPDATE dbo.DonHang
                  SET TrangThai = @tt, LyDoHuy = @lydo, NgayCapNhat = SYSDATETIME()
                  WHERE MaDonHang = @ma"))
            {
                lenh.Parameters.Add("@tt", SqlDbType.VarChar, 20).Value = TrangThaiDonHang.DaHuy;
                lenh.Parameters.Add("@lydo", SqlDbType.NVarChar, 500).Value = lyDo;
                lenh.Parameters.Add("@ma", SqlDbType.Int).Value = maDonHang;
                lenh.ExecuteNonQuery();
            }

            // 3) hoàn tồn kho theo từng dòng (gộp theo biến thể), ghi lịch sử tồn kho
            var dong = new List<Tuple<int, int>>();

            using (var lenh = TaoLenh(kn, gd,
                @"SELECT MaBienThe, SUM(SoLuong)
                  FROM dbo.ChiTietDonHang WHERE MaDonHang = @ma
                  GROUP BY MaBienThe ORDER BY MaBienThe"))
            {
                lenh.Parameters.Add("@ma", SqlDbType.Int).Value = maDonHang;

                using (var rd = lenh.ExecuteReader())
                {
                    while (rd.Read())
                        dong.Add(Tuple.Create(rd.GetInt32(0), rd.GetInt32(1)));
                }
            }

            foreach (var d in dong)
            {
                int tonTruoc;

                using (var lenh = TaoLenh(kn, gd,
                    @"SELECT SoLuongTon FROM dbo.BienTheSanPham WITH (UPDLOCK, HOLDLOCK)
                      WHERE MaBienThe = @bt"))
                {
                    lenh.Parameters.Add("@bt", SqlDbType.Int).Value = d.Item1;
                    object o = lenh.ExecuteScalar();
                    if (o == null)
                        throw new InvalidOperationException(
                            "Không tìm thấy biến thể #" + d.Item1 + " để hoàn tồn kho.");
                    tonTruoc = Convert.ToInt32(o);
                }

                using (var lenh = TaoLenh(kn, gd,
                    @"UPDATE dbo.BienTheSanPham SET SoLuongTon = SoLuongTon + @sl
                      WHERE MaBienThe = @bt;
                      INSERT INTO dbo.LichSuTonKho
                        (YeuCauId, MaBienThe, MaTaiKhoan, SoLuongThem, TonTruoc, TonSau, LyDo, NgayTao)
                      VALUES (@yc, @bt, @tk, @sl, @truoc, @truoc + @sl, @lydo, GETDATE());"))
                {
                    lenh.Parameters.Add("@sl", SqlDbType.Int).Value = d.Item2;
                    lenh.Parameters.Add("@bt", SqlDbType.Int).Value = d.Item1;
                    lenh.Parameters.Add("@yc", SqlDbType.UniqueIdentifier).Value =
                        YeuCauHoanKho(maDonHang, d.Item1);
                    lenh.Parameters.Add("@tk", SqlDbType.Int).Value = maNguoiThucHien;
                    lenh.Parameters.Add("@truoc", SqlDbType.Int).Value = tonTruoc;
                    lenh.Parameters.Add("@lydo", SqlDbType.NVarChar, 500).Value =
                        "Hoàn tồn do hủy đơn #" + maDonHang;
                    lenh.ExecuteNonQuery();
                }
            }

            // 4) trả lại 1 lượt dùng mã (không để âm)
            if (maGiamGia.HasValue)
            {
                using (var lenh = TaoLenh(kn, gd,
                    @"UPDATE dbo.MaGiamGia SET SoLuotDaDung = SoLuotDaDung - 1
                      WHERE MaGiamGia = @mg AND SoLuotDaDung > 0"))
                {
                    lenh.Parameters.Add("@mg", SqlDbType.Int).Value = maGiamGia.Value;
                    lenh.ExecuteNonQuery();
                }
            }

            // 5) hoàn tiền mô phỏng cho khoản đã thanh toán
            using (var lenh = TaoLenh(kn, gd,
                @"UPDATE dbo.ThanhToan
                  SET TrangThai = @moi, NgayHoanTien = SYSDATETIME(),
                      GhiChu = LEFT(ISNULL(GhiChu + N' | ', N'') + @gc, 1000)
                  WHERE MaDonHang = @ma AND TrangThai = @cu"))
            {
                lenh.Parameters.Add("@moi", SqlDbType.VarChar, 20).Value = TrangThaiThanhToan.DaHoanTien;
                lenh.Parameters.Add("@cu", SqlDbType.VarChar, 20).Value = TrangThaiThanhToan.DaThanhToan;
                lenh.Parameters.Add("@gc", SqlDbType.NVarChar, 200).Value = "Hoàn tiền mô phỏng do hủy đơn";
                lenh.Parameters.Add("@ma", SqlDbType.Int).Value = maDonHang;
                lenh.ExecuteNonQuery();
            }

            return KetQuaHuyDon.ThanhCong;
        }

        private static SqlCommand TaoLenh(SqlConnection kn, SqlTransaction gd, string sql)
        {
            var lenh = kn.CreateCommand();
            lenh.Transaction = gd;
            lenh.CommandText = sql;
            return lenh;
        }

        // Guid xác định theo (đơn, biến thể): nếu vì lý do nào đó chạy lại thì khóa
        // LichSuTonKho.YeuCauId trùng sẽ làm transaction lỗi thay vì cộng tồn hai lần.
        private static Guid YeuCauHoanKho(int maDonHang, int maBienThe)
        {
            using (var md5 = MD5.Create())
            {
                byte[] h = md5.ComputeHash(
                    Encoding.UTF8.GetBytes("huy-don:" + maDonHang + ":" + maBienThe));
                return new Guid(h);
            }
        }
    }
}
