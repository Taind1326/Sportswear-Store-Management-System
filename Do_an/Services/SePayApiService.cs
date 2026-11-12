
using System;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Do_an.Services
{
    public class SePayTaoDonHangResult
    {
        public string Id { get; set; }
        public string OrderCode { get; set; }
        public string VaNumber { get; set; }
        public long Amount { get; set; }
        public string Status { get; set; }
        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public string AccountHolderName { get; set; }
        public string ExpiredAt { get; set; }
        public string QrCodeUrl { get; set; }
    }

    public class SePayApiService
    {
        private const string MetadataPrefix = "SEPAYMETA1:";
        private const string ProductionBaseUrl =
            "https://userapi.sepay.vn/v2";

        private static readonly HttpClient Http = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(25)
        };

        public async Task<SePayTaoDonHangResult> TaoDonHangAsync(
            string orderCode,
            decimal total)
        {
            string baseUrl = LayCauHinh(
                "SePay.LiveApiBaseUrl");

            string apiToken = LayCauHinh(
                "SePay.LiveApiToken");

            string bankAccountId = LayCauHinh(
                "SePay.LiveBankAccountId");

            string vaPrefix = LayCauHinh(
                "SePay.VaPrefix");

            if (!string.Equals(
                baseUrl.TrimEnd('/'),
                ProductionBaseUrl,
                StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "ApiBaseUrl không phải môi trường Production SePay."
                );
            }

            Guid bankId;

            if (!Guid.TryParse(bankAccountId, out bankId))
            {
                throw new InvalidOperationException(
                    "BankAccountId Sacombank không hợp lệ."
                );
            }

            orderCode = (orderCode ?? "")
                .Trim()
                .ToUpperInvariant();

            if (!Regex.IsMatch(
                orderCode,
                @"^[A-Z0-9]{6,50}$"))
            {
                throw new InvalidOperationException(
                    "Mã đơn phải gồm 6 đến 50 ký tự chữ và số."
                );
            }

            vaPrefix = vaPrefix.Trim().ToUpperInvariant();

            if (!Regex.IsMatch(vaPrefix, @"^[A-Z0-9]+$") ||
                vaPrefix.Length >= 18)
            {
                throw new InvalidOperationException(
                    "Tiền tố VA Sacombank không hợp lệ."
                );
            }

            if (total <= 0)
            {
                throw new InvalidOperationException(
                    "Số tiền thanh toán phải lớn hơn 0."
                );
            }

            decimal amountRounded = decimal.Round(
                total,
                0,
                MidpointRounding.AwayFromZero
            );

            if (total != amountRounded)
            {
                throw new InvalidOperationException(
                    "Số tiền đơn hàng phải là số nguyên VND."
                );
            }

            if (amountRounded > 999999999999m)
            {
                throw new InvalidOperationException(
                    "Số tiền vượt giới hạn tạo đơn hàng SePay."
                );
            }

            long amount = decimal.ToInt64(amountRounded);

            // Sacombank bắt buộc va_prefix, không sử dụng tid.
            var requestBody = new
            {
                va_prefix = vaPrefix,
                order_code = orderCode,
                amount = amount,
                with_qrcode = "1",
                qrcode_template = "compact"
            };

            var serializer = new JavaScriptSerializer();

            string json = serializer.Serialize(requestBody);

            string endpoint =
                baseUrl.TrimEnd('/') +
                "/bank-accounts/" +
                Uri.EscapeDataString(bankAccountId) +
                "/orders";

            using (var request = new HttpRequestMessage(
                HttpMethod.Post,
                endpoint))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue(
                        "Bearer",
                        apiToken
                    );

                request.Content = new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json"
                );

                using (HttpResponseMessage response =
                    await Http.SendAsync(request))
                {
                    string responseText =
                        await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new InvalidOperationException(
                            "SePay trả về HTTP " +
                            (int)response.StatusCode +
                            ". " + DocThongBaoLoi(responseText)
                        );
                    }

                    SePayApiResponse apiResult;

                    try
                    {
                        apiResult = serializer.Deserialize
                            <SePayApiResponse>(responseText);
                    }
                    catch
                    {
                        throw new InvalidOperationException(
                            "Không đọc được phản hồi từ SePay."
                        );
                    }

                    if (apiResult == null ||
                        apiResult.status != "success" ||
                        apiResult.data == null)
                    {
                        throw new InvalidOperationException(
                            "SePay không tạo được đơn hàng thanh toán."
                        );
                    }

                    var data = apiResult.data;

                    if (string.IsNullOrWhiteSpace(data.id) ||
                        string.IsNullOrWhiteSpace(data.order_code) ||
                        string.IsNullOrWhiteSpace(data.va_number) ||
                        string.IsNullOrWhiteSpace(data.account_number) ||
                        string.IsNullOrWhiteSpace(data.qr_code_url))
                    {
                        throw new InvalidOperationException(
                            "SePay trả về thiếu UUID, mã đơn, số VA, " +
                            "tài khoản ngân hàng hoặc đường dẫn QR."
                        );
                    }

                    if (!string.Equals(
                        data.order_code.Trim(),
                        orderCode,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException(
                            "Mã đơn SePay trả về không khớp đơn hàng SPORTIX."
                        );
                    }

                    if (data.amount != amount)
                    {
                        throw new InvalidOperationException(
                            "Số tiền SePay trả về không khớp đơn hàng."
                        );
                    }

                    Uri qrUri;

                    if (!Uri.TryCreate(
                        data.qr_code_url,
                        UriKind.Absolute,
                        out qrUri) ||
                        qrUri.Scheme != Uri.UriSchemeHttps)
                    {
                        throw new InvalidOperationException(
                            "Đường dẫn QR SePay không hợp lệ."
                        );
                    }

                    return new SePayTaoDonHangResult
                    {
                        Id = data.id,
                        OrderCode = data.order_code,
                        VaNumber = data.va_number,
                        Amount = data.amount,
                        Status = data.status,
                        BankName = data.bank_name,
                        AccountNumber = data.account_number,
                        AccountHolderName = data.account_holder_name,
                        ExpiredAt = data.expired_at,
                        QrCodeUrl = data.qr_code_url
                    };
                }
            }
        }

        private static string LayCauHinh(string key)
        {
            string value =
                ConfigurationManager.AppSettings[key];

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException(
                    "Chưa cấu hình " + key +
                    " trong Secrets.config."
                );
            }

            return value.Trim();
        }

        private static string DocThongBaoLoi(
            string responseText)
        {
            try
            {
                var serializer = new JavaScriptSerializer();

                var result = serializer.Deserialize
                    <SePayApiResponse>(responseText);

                if (result != null &&
                    !string.IsNullOrWhiteSpace(result.message))
                {
                    return result.message;
                }
            }
            catch
            {
            }

            return "Không thể tạo lệnh thanh toán.";
        }

        public static string MaHoaThongTinThanhToan(
            SePayTaoDonHangResult data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            var serializer = new JavaScriptSerializer();

            string json = serializer.Serialize(data);

            string encoded =
                MetadataPrefix +
                Convert.ToBase64String(
                    Encoding.UTF8.GetBytes(json)
                );

            if (encoded.Length > 950)
            {
                throw new InvalidOperationException(
                    "Thông tin SePay vượt giới hạn lưu trữ GhiChu."
                );
            }

            return encoded;
        }

        public static bool CoMetadata(string ghiChu)
        {
            return !string.IsNullOrWhiteSpace(ghiChu) &&
                ghiChu.StartsWith(
                    MetadataPrefix,
                    StringComparison.Ordinal
                );
        }

        public static SePayTaoDonHangResult DocThongTinThanhToan(
            string ghiChu)
        {
            if (!CoMetadata(ghiChu))
            {
                return null;
            }

            try
            {
                string encoded = ghiChu.Substring(
                    MetadataPrefix.Length
                );

                int viTriGhiChu = encoded.IndexOf(
                    " | ",
                    StringComparison.Ordinal
                );

                if (viTriGhiChu >= 0)
                {
                    encoded = encoded.Substring(
                        0,
                        viTriGhiChu
                    );
                }

                string json = Encoding.UTF8.GetString(
                    Convert.FromBase64String(encoded)
                );

                return new JavaScriptSerializer()
                    .Deserialize<SePayTaoDonHangResult>(json);
            }
            catch
            {
                return null;
            }
        }

        private class SePayApiResponse
        {
            public string status { get; set; }
            public string message { get; set; }
            public SePayApiOrderData data { get; set; }
        }

        private class SePayApiOrderData
        {
            public string id { get; set; }
            public string order_code { get; set; }
            public string va_number { get; set; }
            public long amount { get; set; }
            public string status { get; set; }
            public string bank_name { get; set; }
            public string account_number { get; set; }
            public string account_holder_name { get; set; }
            public string expired_at { get; set; }
            public string qr_code_url { get; set; }
        }
    }
}
