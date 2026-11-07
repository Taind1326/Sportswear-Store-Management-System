using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Web.Mvc;
using Do_an.Services;

namespace Do_an.Controllers
{
    public class TroLyAIController : Controller
    {
        // POST /TroLyAI/Chat  (JSON: { cauHoi, lichSu:[{role,text}] })
        // Khách chưa đăng nhập vẫn dùng được. Endpoint chỉ đọc dữ liệu, không sửa gì.
        [HttpPost]
        public async Task<ActionResult> Chat(ChatYeuCau yeuCau)
        {
            string cauHoi = yeuCau == null ? "" : (yeuCau.CauHoi ?? "").Trim();

            if (cauHoi.Length == 0)
                return Json(new ChatPhanHoi { ThanhCong = false, NoiDung = "Bạn hãy nhập câu hỏi nhé." });

            if (cauHoi.Length > 500)
                return Json(new ChatPhanHoi
                {
                    ThanhCong = false,
                    NoiDung = "Câu hỏi hơi dài, bạn hãy rút gọn trong 500 ký tự nhé."
                });

            string khoa = "chat:" + Request.UserHostAddress;
            if (!GioiHanTanSuat.ChoPhep(khoa, 12, TimeSpan.FromMinutes(1)))
            {
                Response.StatusCode = 429;
                Response.TrySkipIisCustomErrors = true;

                return Json(new ChatPhanHoi
                {
                    ThanhCong = false,
                    NoiDung = "Bạn nhắn hơi nhanh. Vui lòng đợi một chút rồi hỏi tiếp nhé."
                });
            }

            try
            {
                string traLoi = await TroLyAIService.TraLoiAsync(cauHoi, yeuCau.LichSu);
                return Json(new ChatPhanHoi { ThanhCong = true, NoiDung = traLoi });
            }
            catch (GeminiException ex)
            {
                return Json(new ChatPhanHoi { ThanhCong = false, NoiDung = ex.Message });
            }
            catch (Exception ex)
            {
                Trace.TraceError("TroLyAI lỗi: " + ex);

                return Json(new ChatPhanHoi
                {
                    ThanhCong = false,
                    NoiDung = "Xin lỗi, trợ lý đang gặp sự cố. Bạn vui lòng thử lại sau nhé."
                });
            }
        }
    }
}