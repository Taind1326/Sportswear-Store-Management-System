using System.Web.Mvc;
using Do_an.Services;

namespace Do_an.Areas.Admin.Controllers
{
    // Endpoint cho chuông thông báo trên header Admin. Chỉ Admin truy cập được.
    [YeuCauAdmin]
    public class ThongBaoController : Controller
    {
        private readonly ThongBaoService dichVu = new ThongBaoService();

        [HttpGet]
        public ActionResult DanhSach()
        {
            int ma = XacThucService.LayMaTaiKhoanHienTai().Value;

            return Json(new
            {
                chuaDoc = ThongBaoService.DemChuaDoc(ma),
                danhSach = dichVu.LayDanhSach(ma, 20)
            }, JsonRequestBehavior.AllowGet);
        }

        // Cấp token để trang Admin mở kết nối SignalR.
        [HttpGet]
        public ActionResult Token()
        {
            int ma = XacThucService.LayMaTaiKhoanHienTai().Value;
            Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
            return Json(new { token = ThongBaoService.TaoToken(ma) }, JsonRequestBehavior.AllowGet);
        }

        // Đánh dấu đã đọc rồi trả URL của đơn để trình duyệt chuyển tới.
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult DaDoc(int id)
        {
            int ma = XacThucService.LayMaTaiKhoanHienTai().Value;

            if (!dichVu.DanhDauDaDoc(ma, id))
                return HttpNotFound();

            return Json(new { chuaDoc = ThongBaoService.DemChuaDoc(ma) });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult DocTatCa()
        {
            int ma = XacThucService.LayMaTaiKhoanHienTai().Value;
            dichVu.DanhDauTatCaDaDoc(ma);
            return Json(new { chuaDoc = 0 });
        }
    }
}
