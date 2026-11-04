using Do_an.Models;
using System;
using System.Data;
using System.Data.SqlClient;

namespace Do_an.Services
{
    public class TonKhoService
    {
        public bool BoSungHang(
            int maSanPham,
            int maBienThe,
            int soLuongThem,
            int maTaiKhoan,
            Guid yeuCauId,
            string lyDo)
        {
            if (maSanPham <= 0 || maBienThe <= 0)
            {
                throw new ArgumentException(
                    "Sản phẩm hoặc phân loại không hợp lệ.");
            }

            if (soLuongThem <= 0)
            {
                throw new ArgumentException(
                    "Số lượng bổ sung phải lớn hơn 0.");
            }

            if (maTaiKhoan <= 0)
            {
                throw new ArgumentException(
                    "Không xác định được tài khoản thực hiện.");
            }

            if (yeuCauId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Yêu cầu bổ sung hàng không hợp lệ. Vui lòng tải lại trang.");
            }

            lyDo = (lyDo ?? "").Trim();

            if (lyDo.Length > 500)
            {
                throw new ArgumentException(
                    "Ghi chú không được vượt quá 500 ký tự.");
            }

            using (var db = new SPORTIXEntities())
            {
                var ketNoi = db.Database.Connection as SqlConnection;

                if (ketNoi == null)
                {
                    throw new InvalidOperationException(
                        "Không kết nối được cơ sở dữ liệu SQL Server.");
                }

                ketNoi.Open();

                using (var giaoDich = ketNoi.BeginTransaction(
                    IsolationLevel.Serializable))
                using (var lenh = ketNoi.CreateCommand())
                {
                    lenh.Transaction = giaoDich;

                    lenh.CommandText = @"
SET NOCOUNT ON;

IF NOT EXISTS (
    SELECT 1
    FROM dbo.TaiKhoan
    WHERE MaTaiKhoan = @MaTaiKhoan
      AND VaiTro = 'Admin'
      AND HoatDong = 1
)
BEGIN
    SELECT -1;
    RETURN;
END;

IF EXISTS (
    SELECT 1
    FROM dbo.LichSuTonKho WITH (UPDLOCK, HOLDLOCK)
    WHERE YeuCauId = @YeuCauId
)
BEGIN
    IF EXISTS (
        SELECT 1
        FROM dbo.LichSuTonKho AS ls
        INNER JOIN dbo.BienTheSanPham AS bt
            ON bt.MaBienThe = ls.MaBienThe
        WHERE ls.YeuCauId = @YeuCauId
          AND ls.MaBienThe = @MaBienThe
          AND bt.MaSanPham = @MaSanPham
          AND ls.MaTaiKhoan = @MaTaiKhoan
          AND ls.SoLuongThem = @SoLuongThem
          AND ISNULL(ls.LyDo, N'') = @LyDo
    )
        SELECT 0;
    ELSE
        SELECT -4;

    RETURN;
END;

DECLARE @TonTruoc int;

SELECT @TonTruoc = SoLuongTon
FROM dbo.BienTheSanPham WITH (UPDLOCK, HOLDLOCK)
WHERE MaBienThe = @MaBienThe
  AND MaSanPham = @MaSanPham;

IF @TonTruoc IS NULL
BEGIN
    SELECT -2;
    RETURN;
END;

IF @TonTruoc > 2147483647 - @SoLuongThem
BEGIN
    SELECT -3;
    RETURN;
END;

DECLARE @TonSau int = @TonTruoc + @SoLuongThem;
DECLARE @NgayThucHien datetime = GETDATE();

UPDATE dbo.BienTheSanPham
SET SoLuongTon = @TonSau
WHERE MaBienThe = @MaBienThe
  AND MaSanPham = @MaSanPham;

INSERT INTO dbo.LichSuTonKho (
    YeuCauId,
    MaBienThe,
    MaTaiKhoan,
    SoLuongThem,
    TonTruoc,
    TonSau,
    LyDo,
    NgayTao
)
VALUES (
    @YeuCauId,
    @MaBienThe,
    @MaTaiKhoan,
    @SoLuongThem,
    @TonTruoc,
    @TonSau,
    @LyDo,
    @NgayThucHien
);

UPDATE dbo.SanPham
SET NgayCapNhat = @NgayThucHien
WHERE MaSanPham = @MaSanPham;

SELECT 1;
";

                    lenh.Parameters.Add(
                        "@MaSanPham",
                        SqlDbType.Int).Value = maSanPham;

                    lenh.Parameters.Add(
                        "@MaBienThe",
                        SqlDbType.Int).Value = maBienThe;

                    lenh.Parameters.Add(
                        "@SoLuongThem",
                        SqlDbType.Int).Value = soLuongThem;

                    lenh.Parameters.Add(
                        "@MaTaiKhoan",
                        SqlDbType.Int).Value = maTaiKhoan;

                    lenh.Parameters.Add(
                        "@YeuCauId",
                        SqlDbType.UniqueIdentifier).Value = yeuCauId;

                    lenh.Parameters.Add(
                        "@LyDo",
                        SqlDbType.NVarChar,
                        500).Value = lyDo;

                    int ketQua = Convert.ToInt32(
                        lenh.ExecuteScalar());

                    if (ketQua == 1 || ketQua == 0)
                    {
                        giaoDich.Commit();

                        return ketQua == 1;
                    }

                    giaoDich.Rollback();

                    switch (ketQua)
                    {
                        case -1:
                            throw new InvalidOperationException(
                                "Tài khoản không có quyền bổ sung hàng.");

                        case -2:
                            throw new InvalidOperationException(
                                "Phân loại không tồn tại hoặc không thuộc sản phẩm này.");

                        case -3:
                            throw new InvalidOperationException(
                                "Số lượng bổ sung làm tồn kho vượt giới hạn cho phép.");

                        case -4:
                            throw new InvalidOperationException(
                                "Mã yêu cầu đã được sử dụng cho lần bổ sung khác. Vui lòng mở lại form.");

                        default:
                            throw new InvalidOperationException(
                                "Chưa bổ sung được hàng. Vui lòng thử lại.");
                    }
                }
            }
        }
    }
}