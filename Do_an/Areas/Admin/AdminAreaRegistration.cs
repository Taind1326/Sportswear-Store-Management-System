using System.Web.Mvc;

namespace Do_an.Areas.Admin
{
    public class AdminAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get { return "Admin"; }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                name: "Admin_default",
                url: "Admin/{controller}/{action}/{id}",
                defaults: new
                {
                    controller = "Dashboard",
                    action = "Index",
                    id = UrlParameter.Optional
                },
                namespaces: new[] { "Do_an.Areas.Admin.Controllers" }
            );
        }
    }
}