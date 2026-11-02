using System.Web.Mvc;
using System.Web.Routing;

namespace Do_an
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            var route = routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new
                {
                    controller = "Home",
                    action = "Index",
                    id = UrlParameter.Optional
                },
                namespaces: new[] { "Do_an.Controllers" }
            );

            route.DataTokens["UseNamespaceFallback"] = false;
        }
    }
}