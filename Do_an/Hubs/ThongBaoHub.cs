using System;
using System.Linq;
using System.Threading.Tasks;
using Do_an.Services;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace Do_an.Hubs
{
    // Session ASP.NET không có sẵn trong request SignalR, nên kết nối được xác thực
    // bằng token ngắn hạn do server cấp trong trang Admin (ThongBaoService.TaoToken).
    // Chỉ Admin mới lấy được token => chỉ Admin nhận thông báo đơn mới.
    public class AdminHubAttribute : AuthorizeAttribute
    {
        public override bool AuthorizeHubConnection(
            HubDescriptor hubDescriptor, Microsoft.AspNet.SignalR.IRequest request)
        {
            return ThongBaoService.LayMaAdminTuToken(request.QueryString["token"]) != null;
        }

        public override bool AuthorizeHubMethodInvocation(
            IHubIncomingInvokerContext ctx, bool appliesToMethod)
        {
            return ThongBaoService.LayMaAdminTuToken(
                ctx.Hub.Context.QueryString["token"]) != null;
        }
    }

    [AdminHub]
    public class ThongBaoHub : Hub
    {
        public static string TenNhom(int maTaiKhoan)
        {
            return "admin-" + maTaiKhoan;
        }

        public override Task OnConnected()
        {
            int? ma = ThongBaoService.LayMaAdminTuToken(Context.QueryString["token"]);

            if (ma != null)
                Groups.Add(Context.ConnectionId, TenNhom(ma.Value));

            return base.OnConnected();
        }

        public override Task OnReconnected()
        {
            int? ma = ThongBaoService.LayMaAdminTuToken(Context.QueryString["token"]);

            if (ma != null)
                Groups.Add(Context.ConnectionId, TenNhom(ma.Value));

            return base.OnReconnected();
        }
    }
}
