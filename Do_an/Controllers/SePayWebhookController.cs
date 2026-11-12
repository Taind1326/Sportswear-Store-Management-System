
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.EntityClient;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Do_an.Services;

namespace Do_an.Controllers
{
    // Endpoint nhận webhook SePay.
    // Không yêu cầu khách hàng đăng nhập.
    public class SePayWebhookController : Controller
    {
        [HttpPost]
        [AllowAnonymous]
        public ActionResult Index()
        {
            // =====================================================
            // 1. XÁC THỰC API KEY
            // =====================================================

            string expectedApiKey =
                ConfigurationManager.AppSettings[
                    "SePay.WebhookApiKey"
                ];

            if (string.IsNullOrWhiteSpace(expectedApiKey))
            {
                return TraLoi(
                    503,
                    false,
                    "Webhook chưa được cấu hình API Key."
                );
            }

            // Đọc Authorization header.
            string authorization =
                Request.Headers["Authorization"];

            // Dự phòng khi IIS chuyển Authorization
            // vào ServerVariables.
            if (string.IsNullOrWhiteSpace(authorization))
            {
                authorization =
                    Request.ServerVariables["HTTP_AUTHORIZATION"];
            }

            if (string.IsNullOrWhiteSpace(authorization))
            {
                authorization =
                    Request.ServerVariables[
                        "REDIRECT_HTTP_AUTHORIZATION"
                    ];
            }

            const string apiKeyPrefix = "Apikey ";

            if (string.IsNullOrWhiteSpace(authorization) ||
                !authorization.StartsWith(
                    apiKeyPrefix,
                    StringComparison.OrdinalIgnoreCase))
            {
                return TraLoi(
                    401,
                    false,
                    "Thiếu API Key xác thực webhook."
                );
            }

            string providedApiKey =
                authorization.Substring(
                    apiKeyPrefix.Length
                ).Trim();

            if (!SoSanhApiKey(
                expectedApiKey.Trim(),
                providedApiKey))
            {
                return TraLoi(
                    401,
                    false,
                    "API Key webhook không hợp lệ."
                );
            }

            // =====================================================
            // 2. ĐỌC NGUYÊN BẢN BODY
            // =====================================================

            byte[] rawBody;

            using (var stream = new MemoryStream())
            {
                if (Request.InputStream.CanSeek)
                {
                    Request.InputStream.Position = 0;
                }

                Request.InputStream.CopyTo(stream);
                rawBody = stream.ToArray();
            }

            if (rawBody == null || rawBody.Length == 0)
            {
                return TraLoi(
                    400,
                    false,
                    "Dữ liệu webhook rỗng."
                );
            }

            string rawText = Encoding.UTF8.GetString(rawBody);

            // =====================================================
            // 3. GIẢI MÃ JSON
            // =====================================================

            SePayPayload payload;

            try
            {
                var serializer = new JavaScriptSerializer();

                payload = serializer.Deserialize<SePayPayload>(
                    rawText
                );

                // Kiểm tra JSON có trường id thực sự.
                var rawFields =
                    serializer.Deserialize<Dictionary<string, object>>(
                        rawText
                    );

                if (payload == null ||
                    rawFields == null ||
                    !rawFields.ContainsKey("id") ||
                    rawFields["id"] == null)
                {
                    return TraLoi(
                        400,
                        false,
                        "Dữ liệu webhook thiếu mã giao dịch."
                    );
                }

                long transactionId;

                if (!long.TryParse(
                    Convert.ToString(
                        rawFields["id"],
                        CultureInfo.InvariantCulture
                    ),
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out transactionId))
                {
                    return TraLoi(
                        400,
                        false,
                        "Mã giao dịch không hợp lệ."
                    );
                }

                payload.id = transactionId;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);

                return TraLoi(
                    400,
                    false,
                    "Dữ liệu webhook không đúng định dạng JSON."
                );
            }

            // =====================================================
            // 4. XỬ LÝ PAYLOAD GỬI THỬ CỦA SEPAY
            // =====================================================

            // SePay thường sử dụng id = 0 cho chức năng Gửi thử.
            // Chỉ xác nhận endpoint đã nhận request.
            // Không ghi log và không cập nhật đơn hàng.
            if (payload.id == 0)
            {
                return TraLoi(
                    201,
                    true,
                    "Webhook test đã được tiếp nhận."
                );
            }

            if (payload.id < 0)
            {
                return TraLoi(
                    400,
                    false,
                    "Mã giao dịch không hợp lệ."
                );
            }

            // =====================================================
            // 5. KIỂM TRA DỮ LIỆU GIAO DỊCH
            // =====================================================

            if (payload.transferAmount <= 0 ||
                string.IsNullOrWhiteSpace(payload.transferType))
            {
                return TraLoi(
                    400,
                    false,
                    "Dữ liệu giao dịch không hợp lệ."
                );
            }

            // =====================================================
            // 6. CHỈ XỬ LÝ TIỀN VÀO
            // =====================================================

            if (!string.Equals(
                payload.transferType.Trim(),
                "in",
                StringComparison.OrdinalIgnoreCase))
            {
                return TraLoi(
                    201,
                    true,
                    "Không phải giao dịch tiền vào."
                );
            }

            // =====================================================
            // 7. LẤY MÃ THANH TOÁN SPX
            // =====================================================

            string maThanhToan = LayMaThanhToan(payload);

            try
            {
                string connectionString =
                    LaySqlConnectionString();

                using (var connection =
                    new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (var transaction =
                        connection.BeginTransaction(
                            IsolationLevel.Serializable))
                    {
                        // =====================================================
                        // 8. KIỂM TRA GIAO DỊCH ĐÃ XỬ LÝ CHƯA
                        // =====================================================

                        using (var check = new SqlCommand(
                            @"SELECT COUNT(1)
                              FROM dbo.SePayWebhookLog
                                   WITH (UPDLOCK, HOLDLOCK)
                              WHERE TransactionId = @id",
                            connection,
                            transaction))
                        {
                            check.Parameters.Add(
                                "@id",
                                SqlDbType.BigInt
                            ).Value = payload.id;

                            int daTonTai = Convert.ToInt32(
                                check.ExecuteScalar()
                            );

                            if (daTonTai > 0)
                            {
                                transaction.Commit();

                                return TraLoi(
                                    201,
                                    true,
                                    "Giao dịch đã được xử lý."
                                );
                            }
                        }

                        // =====================================================
                        // 9. GHI NHẬN WEBHOOK
                        // =====================================================

                        // Payload được lưu nguyên bản để đối soát.
                        using (var insertLog = new SqlCommand(
                            @"INSERT INTO dbo.SePayWebhookLog
                            (
                                TransactionId,
                                MaDonHang,
                                PaymentCode,
                                Gateway,
                                ReferenceCode,
                                TransferAmount,
                                TransferType,
                                Payload,
                                NgayNhan
                            )
                            VALUES
                            (
                                @id,
                                NULL,
                                @code,
                                @gateway,
                                @reference,
                                @amount,
                                @type,
                                @payload,
                                SYSDATETIME()
                            )",
                            connection,
                            transaction))
                        {
                            insertLog.Parameters.Add(
                                "@id",
                                SqlDbType.BigInt
                            ).Value = payload.id;

                            insertLog.Parameters.Add(
                                "@code",
                                SqlDbType.NVarChar,
                                200
                            ).Value =
                                (object)maThanhToan ?? DBNull.Value;

                            insertLog.Parameters.Add(
                                "@gateway",
                                SqlDbType.NVarChar,
                                200
                            ).Value =
                                (object)payload.gateway ?? DBNull.Value;

                            insertLog.Parameters.Add(
                                "@reference",
                                SqlDbType.NVarChar,
                                200
                            ).Value =
                                (object)payload.referenceCode ?? DBNull.Value;

                            var amountParameter =
                                insertLog.Parameters.Add(
                                    "@amount",
                                    SqlDbType.Decimal
                                );

                            amountParameter.Precision = 9;
                            amountParameter.Scale = 0;
                            amountParameter.Value =
                                payload.transferAmount;

                            insertLog.Parameters.Add(
                                "@type",
                                SqlDbType.VarChar,
                                10
                            ).Value = payload.transferType;

                            insertLog.Parameters.Add(
                                "@payload",
                                SqlDbType.NVarChar,
                                -1
                            ).Value = rawText;

                            insertLog.ExecuteNonQuery();
                        }

                        // =====================================================
                        // 10. KHÔNG CÓ MÃ SPX
                        // =====================================================

                        if (string.IsNullOrWhiteSpace(maThanhToan))
                        {
                            transaction.Commit();

                            return TraLoi(
                                201,
                                true,
                                "Giao dịch không có mã thanh toán SPORTIX."
                            );
                        }

                        int maThanhToanDb = 0;
                        int maDonHang = 0;

                        decimal soTienCanThanhToan = 0m;

                        string trangThaiThanhToan = null;
                        string ghiChuThanhToan = null;
                        string trangThaiDonHang = null;

                        // =====================================================
                        // 11. TÌM KHOẢN THANH TOÁN
                        // =====================================================

                        using (var selectPayment = new SqlCommand(
                            @"SELECT TOP (1)
                                    tt.MaThanhToan,
                                    tt.MaDonHang,
                                    tt.SoTien,
                                    tt.TrangThai,
                                    tt.GhiChu,
                                    dh.TrangThai
                              FROM dbo.ThanhToan AS tt
                                   WITH (UPDLOCK, HOLDLOCK)
                              INNER JOIN dbo.DonHang AS dh
                                   WITH (UPDLOCK, HOLDLOCK)
                                  ON dh.MaDonHang = tt.MaDonHang
                              WHERE tt.PhuongThuc = 'ChuyenKhoan'
                                AND tt.MaThamChieu = @code
                              ORDER BY tt.MaThanhToan DESC",
                            connection,
                            transaction))
                        {
                            selectPayment.Parameters.Add(
                                "@code",
                                SqlDbType.NVarChar,
                                100
                            ).Value = maThanhToan;

                            using (var reader =
                                selectPayment.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    maThanhToanDb =
                                        reader.GetInt32(0);

                                    maDonHang =
                                        reader.GetInt32(1);

                                    soTienCanThanhToan =
                                        reader.GetDecimal(2);

                                    trangThaiThanhToan =
                                        reader.IsDBNull(3)
                                            ? null
                                            : reader.GetString(3);

                                    ghiChuThanhToan =
                                        reader.IsDBNull(4)
                                            ? null
                                            : reader.GetString(4);

                                    trangThaiDonHang =
                                        reader.IsDBNull(5)
                                            ? null
                                            : reader.GetString(5);
                                }
                            }
                        }

                        // =====================================================
                        // 12. KHÔNG TÌM THẤY ĐƠN HÀNG
                        // =====================================================

                        if (maThanhToanDb <= 0 ||
                            maDonHang <= 0)
                        {
                            transaction.Commit();

                            return TraLoi(
                                201,
                                true,
                                "Không tìm thấy khoản thanh toán SPORTIX tương ứng."
                            );
                        }

                        // =====================================================
                        // 13. GẮN MÃ ĐƠN VÀO LOG
                        // =====================================================

                        using (var updateLog = new SqlCommand(
                            @"UPDATE dbo.SePayWebhookLog
                              SET MaDonHang = @orderId
                              WHERE TransactionId = @transactionId",
                            connection,
                            transaction))
                        {
                            updateLog.Parameters.Add(
                                "@orderId",
                                SqlDbType.Int
                            ).Value = maDonHang;

                            updateLog.Parameters.Add(
                                "@transactionId",
                                SqlDbType.BigInt
                            ).Value = payload.id;

                            updateLog.ExecuteNonQuery();
                        }

                        // =====================================================
                        // 14. ĐỌC METADATA SEPAY
                        // =====================================================

                        SePayTaoDonHangResult metadata =
                            SePayApiService.DocThongTinThanhToan(
                                ghiChuThanhToan
                            );

                        if (metadata == null)
                        {
                            // Rollback log để SePay có thể gửi lại.
                            throw new InvalidOperationException(
                                "Không đọc được metadata SePay của đơn hàng #" +
                                maDonHang
                            );
                        }

                        // =====================================================
                        // 15. ĐỐI CHIẾU MÃ ĐƠN
                        // =====================================================

                        bool maDonKhop =
                            string.Equals(
                                (metadata.OrderCode ?? "").Trim(),
                                maThanhToan,
                                StringComparison.OrdinalIgnoreCase
                            ) &&
                            string.Equals(
                                (metadata.OrderCode ?? "").Trim(),
                                (ghiChuThanhToan == null
                                    ? ""
                                    : metadata.OrderCode.Trim()),
                                StringComparison.OrdinalIgnoreCase
                            );

                        // =====================================================
                        // 16. ĐỐI CHIẾU TÀI KHOẢN SACOMBANK
                        // =====================================================

                        bool taiKhoanKhop =
                            !string.IsNullOrWhiteSpace(
                                metadata.AccountNumber
                            ) &&
                            !string.IsNullOrWhiteSpace(
                                payload.accountNumber
                            ) &&
                            string.Equals(
                                payload.accountNumber.Trim(),
                                metadata.AccountNumber.Trim(),
                                StringComparison.Ordinal
                            );

                        // =====================================================
                        // 17. ĐỐI CHIẾU SỐ TÀI KHOẢN ẢO
                        // =====================================================

                        bool vaKhop =
                            !string.IsNullOrWhiteSpace(
                                metadata.VaNumber
                            ) &&
                            !string.IsNullOrWhiteSpace(
                                payload.subAccount
                            ) &&
                            string.Equals(
                                payload.subAccount.Trim(),
                                metadata.VaNumber.Trim(),
                                StringComparison.OrdinalIgnoreCase
                            );

                        // =====================================================
                        // 18. ĐỐI CHIẾU SỐ TIỀN
                        // =====================================================

                        bool soTienKhop =
                            metadata.Amount > 0 &&
                            payload.transferAmount ==
                                soTienCanThanhToan &&
                            payload.transferAmount ==
                                (decimal)metadata.Amount;

                        // =====================================================
                        // 19. KIỂM TRA ĐỦ ĐIỀU KIỆN THANH TOÁN
                        // =====================================================

                        bool duDieuKienThanhToan =
                            maDonKhop &&
                            taiKhoanKhop &&
                            vaKhop &&
                            soTienKhop &&
                            trangThaiThanhToan == "ChuaThanhToan" &&
                            trangThaiDonHang != "DaHuy";

                        // =====================================================
                        // 20. CẬP NHẬT THANH TOÁN
                        // =====================================================

                        if (duDieuKienThanhToan)
                        {
                            string ghiChu =
                                "SePay Transaction ID: " +
                                payload.id +
                                "; Reference: " +
                                (payload.referenceCode ?? "");

                            using (var updatePayment = new SqlCommand(
                                @"UPDATE dbo.ThanhToan
                                  SET TrangThai = 'DaThanhToan',
                                      NgayThanhToan = SYSDATETIME(),
                                      GhiChu = LEFT(
                                          COALESCE(
                                              GhiChu + N' | ',
                                              N''
                                          ) + @note,
                                          1000
                                      )
                                  WHERE MaThanhToan = @paymentId
                                    AND TrangThai = 'ChuaThanhToan'
                                    AND SoTien = @amount",
                                connection,
                                transaction))
                            {
                                updatePayment.Parameters.Add(
                                    "@note",
                                    SqlDbType.NVarChar,
                                    1000
                                ).Value = ghiChu;

                                updatePayment.Parameters.Add(
                                    "@paymentId",
                                    SqlDbType.Int
                                ).Value = maThanhToanDb;

                                var amountParameter =
                                    updatePayment.Parameters.Add(
                                        "@amount",
                                        SqlDbType.Decimal
                                    );

                                amountParameter.Precision = 9;
                                amountParameter.Scale = 0;
                                amountParameter.Value =
                                    payload.transferAmount;

                                int soDongCapNhat =
                                    updatePayment.ExecuteNonQuery();

                                if (soDongCapNhat != 1)
                                {
                                    throw new InvalidOperationException(
                                        "Không thể cập nhật trạng thái thanh toán #" +
                                        maThanhToanDb
                                    );
                                }
                            }
                        }
                        else
                        {
                            // Không xác nhận khi:
                            // - Sai mã đơn hàng.
                            // - Sai tài khoản ngân hàng.
                            // - Sai số VA.
                            // - Sai số tiền.
                            // - Đơn đã bị hủy.
                            // - Khoản thanh toán không còn chờ.

                            System.Diagnostics.Debug.WriteLine(
                                "SePay không xác nhận thanh toán. " +
                                "DonHang=" + maDonHang +
                                "; MaThanhToan=" + maThanhToanDb +
                                "; MaDonKhop=" + maDonKhop +
                                "; TaiKhoanKhop=" + taiKhoanKhop +
                                "; VAKhop=" + vaKhop +
                                "; SoTienKhop=" + soTienKhop +
                                "; TrangThaiDonHang=" + trangThaiDonHang +
                                "; TrangThaiThanhToan=" + trangThaiThanhToan
                            );
                        }

                        // =====================================================
                        // 21. HOÀN TẤT TRANSACTION
                        // =====================================================

                        transaction.Commit();
                    }
                }

                return TraLoi(
                    201,
                    true,
                    "Đã nhận webhook."
                );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);

                return TraLoi(
                    500,
                    false,
                    "Không thể xử lý webhook."
                );
            }
        }

        // =====================================================
        // SO SÁNH API KEY
        // =====================================================

        private static bool SoSanhApiKey(
            string expected,
            string provided)
        {
            if (string.IsNullOrEmpty(expected) ||
                string.IsNullOrEmpty(provided))
            {
                return false;
            }

            byte[] a = Encoding.UTF8.GetBytes(expected);
            byte[] b = Encoding.UTF8.GetBytes(provided);

            int ketQua = a.Length ^ b.Length;
            int maxLength = Math.Max(a.Length, b.Length);

            for (int i = 0; i < maxLength; i++)
            {
                byte x = i < a.Length
                    ? a[i]
                    : (byte)0;

                byte y = i < b.Length
                    ? b[i]
                    : (byte)0;

                ketQua |= x ^ y;
            }

            return ketQua == 0;
        }

        // =====================================================
        // LẤY MÃ THANH TOÁN SPX
        // =====================================================

        private static string LayMaThanhToan(
            SePayPayload payload)
        {
            string ma = (payload.code ?? "")
                .Trim()
                .ToUpperInvariant();

            if (Regex.IsMatch(
                ma,
                @"^SPX\d{8,10}$",
                RegexOptions.IgnoreCase))
            {
                return ma;
            }

            string noiDung = payload.content ?? "";

            Match match = Regex.Match(
                noiDung,
                @"SPX\d{8,10}",
                RegexOptions.IgnoreCase
            );

            return match.Success
                ? match.Value.ToUpperInvariant()
                : null;
        }

        // =====================================================
        // KẾT NỐI SQL SERVER
        // =====================================================

        private static string LaySqlConnectionString()
        {
            var config =
                ConfigurationManager.ConnectionStrings[
                    "SPORTIXEntities"
                ];

            if (config == null ||
                string.IsNullOrWhiteSpace(config.ConnectionString))
            {
                throw new InvalidOperationException(
                    "Không tìm thấy connection string SPORTIXEntities."
                );
            }

            if (string.Equals(
                config.ProviderName,
                "System.Data.EntityClient",
                StringComparison.OrdinalIgnoreCase))
            {
                var entityBuilder =
                    new EntityConnectionStringBuilder(
                        config.ConnectionString
                    );

                if (string.IsNullOrWhiteSpace(
                    entityBuilder.ProviderConnectionString))
                {
                    throw new InvalidOperationException(
                        "Provider connection string không hợp lệ."
                    );
                }

                return entityBuilder.ProviderConnectionString;
            }

            return config.ConnectionString;
        }

        // =====================================================
        // PHẢN HỒI CHO SEPAY
        // =====================================================

        private ActionResult TraLoi(
            int statusCode,
            bool success,
            string message)
        {
            // SePay chấp nhận 200 hoặc 201.
            // Dùng 201 để tương thích webhook gắn trực tiếp
            // với một tài khoản ngân hàng.
            Response.StatusCode = success ? 201 : statusCode;

            Response.TrySkipIisCustomErrors = true;
            Response.ContentType = "application/json";

            if (success)
            {
                return Json(
                    new
                    {
                        success = true
                    },
                    JsonRequestBehavior.DenyGet
                );
            }

            return Json(
                new
                {
                    success = false,
                    message = message
                },
                JsonRequestBehavior.DenyGet
            );
        }

        // =====================================================
        // CẤU TRÚC PAYLOAD SEPAY
        // =====================================================

        public class SePayPayload
        {
            public long id { get; set; }

            public string gateway { get; set; }

            public string transactionDate { get; set; }

            public string accountNumber { get; set; }

            public string subAccount { get; set; }

            public string code { get; set; }

            public string content { get; set; }

            public string transferType { get; set; }

            public string description { get; set; }

            public decimal transferAmount { get; set; }

            public decimal accumulated { get; set; }

            public string referenceCode { get; set; }
        }
    }
}
