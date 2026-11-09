using System.Threading.Tasks;
using Do_an.Services;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace Do_an.Hubs
{
    // Token được cấp từ trang Admin để xác thực kết nối SignalR.
    public class AdminHubAttribute : AuthorizeAttribute
    {
        public override bool AuthorizeHubConnection(
            HubDescriptor hubDescriptor,
            IRequest request)
        {
            return ThongBaoService.LayMaAdminTuToken(
                request.QueryString["token"]).HasValue;
        }

        public override bool AuthorizeHubMethodInvocation(
            IHubIncomingInvokerContext context,
            bool appliesToMethod)
        {
            return ThongBaoService.LayMaAdminTuToken(
                context.Hub.Context.QueryString["token"])
                .HasValue;
        }
    }

    [AdminHub]
    public class ThongBaoHub : Hub
    {
        public static string TenNhom(int maTaiKhoan)
        {
            return "admin-" + maTaiKhoan;
        }

        public override async Task OnConnected()
        {
            int? maAdmin =
                ThongBaoService.LayMaAdminTuToken(
                    Context.QueryString["token"]);

            if (maAdmin.HasValue)
            {
                await Groups.Add(
                    Context.ConnectionId,
                    TenNhom(maAdmin.Value));
            }

            await base.OnConnected();
        }

        public override async Task OnReconnected()
        {
            int? maAdmin =
                ThongBaoService.LayMaAdminTuToken(
                    Context.QueryString["token"]);

            if (maAdmin.HasValue)
            {
                await Groups.Add(
                    Context.ConnectionId,
                    TenNhom(maAdmin.Value));
            }

            await base.OnReconnected();
        }
    }
}