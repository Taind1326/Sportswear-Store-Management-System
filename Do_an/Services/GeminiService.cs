using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Do_an.Services
{
    // Lỗi có thông báo thân thiện, hiển thị thẳng cho khách.
    public class GeminiException : Exception
    {
        public GeminiException(string thongBao) : base(thongBao) { }
    }

    public class GeminiService
    {
        private static readonly HttpClient Http = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(25)
        };

        private readonly string apiKey;
        private readonly string model;

        public GeminiService()
        {
            apiKey = ConfigurationManager.AppSettings["Gemini:ApiKey"];
            model = ConfigurationManager.AppSettings["Gemini:Model"];

            if (string.IsNullOrWhiteSpace(model)) model = "gemini-2.5-flash";
        }

        // luot: danh sách (role, text) với role là "user" hoặc "model".
        public async Task<string> TaoNoiDungAsync(
            string huongDan,
            IList<KeyValuePair<string, string>> luot)
        {
            if (string.IsNullOrWhiteSpace(apiKey) || apiKey.StartsWith("DAN_API_KEY"))
                throw new GeminiException("Trợ lý AI chưa được cấu hình. Vui lòng quay lại sau.");

            var body = new JObject
            {
                ["system_instruction"] = new JObject
                {
                    ["parts"] = new JArray(new JObject { ["text"] = huongDan })
                },
                ["contents"] = new JArray(luot.Select(l => new JObject
                {
                    ["role"] = l.Key,
                    ["parts"] = new JArray(new JObject { ["text"] = l.Value })
                })),
                ["generationConfig"] = new JObject
                {
                    ["temperature"] = 0.4,
                    ["maxOutputTokens"] = 2048
                }
            };

            string url = "https://generativelanguage.googleapis.com/v1beta/models/"
                + Uri.EscapeDataString(model) + ":generateContent";

            using (var yeuCau = new HttpRequestMessage(HttpMethod.Post, url))
            {
                // Khóa API đặt trong header, không đưa lên URL.
                yeuCau.Headers.Add("x-goog-api-key", apiKey);
                yeuCau.Content = new StringContent(
                    body.ToString(Formatting.None), Encoding.UTF8, "application/json");

                HttpResponseMessage phanHoi;

                try
                {
                    phanHoi = await Http.SendAsync(yeuCau).ConfigureAwait(false);
                }
                catch (TaskCanceledException)
                {
                    throw new GeminiException(
                        "Trợ lý AI phản hồi quá lâu. Bạn vui lòng thử lại sau nhé.");
                }
                catch (HttpRequestException)
                {
                    throw new GeminiException(
                        "Không kết nối được tới trợ lý AI. Bạn vui lòng thử lại sau nhé.");
                }

                using (phanHoi)
                {
                    string noiDung = await phanHoi.Content.ReadAsStringAsync().ConfigureAwait(false);

                    if (!phanHoi.IsSuccessStatusCode)
                    {
                        int ma = (int)phanHoi.StatusCode;
                        Trace.TraceError("Gemini lỗi {0}: {1}", ma,
                            noiDung.Length > 300 ? noiDung.Substring(0, 300) : noiDung);

                        if (ma == 429)
                            throw new GeminiException(
                                "Trợ lý AI đang quá tải. Bạn vui lòng thử lại sau ít phút.");

                        throw new GeminiException(
                            "Trợ lý AI đang gặp sự cố. Bạn vui lòng thử lại sau nhé.");
                    }

                    return TrichVanBan(noiDung);
                }
            }
        }

        private static string TrichVanBan(string json)
        {
            JObject goc;
            try
            {
                goc = JObject.Parse(json);
            }
            catch (JsonException)
            {
                throw new GeminiException("Trợ lý AI trả về dữ liệu không hợp lệ. Vui lòng thử lại.");
            }

            var cacPhan = goc.SelectToken("candidates[0].content.parts") as JArray;

            string text = cacPhan == null
                ? ""
                : string.Join("", cacPhan.Select(p => (string)p["text"]));

            if (string.IsNullOrWhiteSpace(text))
                throw new GeminiException(
                    "Trợ lý AI chưa thể trả lời câu hỏi này. Bạn thử diễn đạt khác nhé.");

            return text.Trim();
        }
    }
}