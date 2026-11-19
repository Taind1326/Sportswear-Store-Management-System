using System;
using System.Web.Mvc;
using System.Web.Routing;
using Do_an.Models;
using Do_an.Services;

namespace Do_an.Services
{
    // Yêu cầu đã đăng nhập (kiểm tra lại DB mỗi lần: bắt được tài khoản bị khóa giữa chừng).
    public class YeuCauDangNhapAttribute : ActionFilterAttribute
    {
        protected virtual string VaiTroBatBuoc
        {
            get { return null; }
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            TaiKhoan tk;
            bool biKhoa;

            using (var db = new SPORTIXEntities())
            {
                tk = XacThucService.LayTaiKhoanHienTai(db, out biKhoa);
            }

            if (tk == null)
            {
                if (biKhoa)
                {
                    filterContext.Controller.TempData["Loi"] =
                        "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ cửa hàng.";
                }

                filterContext.Result = TaoKetQuaChuaDangNhap(filterContext);
                return;
            }

            if (VaiTroBatBuoc != null && tk.VaiTro != VaiTroBatBuoc)
            {
                filterContext.Result = new HttpStatusCodeResult(
                    403, "Bạn không có quyền truy cập chức năng này.");
            }
        }

        private static ActionResult TaoKetQuaChuaDangNhap(ActionExecutingContext ctx)
        {
            var req = ctx.HttpContext.Request;

            if (req.IsAjaxRequest())
                return new HttpStatusCodeResult(401, "Cần đăng nhập.");

            string returnUrl = null;

            if (string.Equals(req.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
            {
                returnUrl = req.Url == null ? null : req.Url.PathAndQuery;
            }
            else if (req.UrlReferrer != null && req.Url != null
                && req.UrlReferrer.Host == req.Url.Host)
            {
                returnUrl = req.UrlReferrer.PathAndQuery;
            }

            var route = new RouteValueDictionary
            {
                { "area", "" },
                { "controller", "TaiKhoan" },
                { "action", "DangNhap" }
            };

            if (!string.IsNullOrEmpty(returnUrl))
                route["returnUrl"] = returnUrl;

            return new RedirectToRouteResult(route);
        }
    }

    // Yêu cầu đăng nhập VÀ có vai trò Admin (User thường nhận 403).
    public class YeuCauAdminAttribute : YeuCauDangNhapAttribute
    {
        protected override string VaiTroBatBuoc
        {
            get { return XacThucService.VaiTroAdmin; }
        }
    }

    // Chỉ khách hàng đang hoạt động được sử dụng giỏ hàng và đặt hàng.
    public class YeuCauKhachHangAttribute : YeuCauDangNhapAttribute
    {
        protected override string VaiTroBatBuoc
        {
            get { return XacThucService.VaiTroUser; }
        }
    }

}
