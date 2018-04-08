using System.Web.Mvc;
using System.Web.Routing;

namespace Web.ZhiXiao.App_Start
{
    public class RouteConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterRoutes(RouteCollection routes)
        {
            //home page
            routes.IgnoreRoute("favicon.ico");
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional }
                //, new[] { "Nop.Web.Controllers" }
            );

            routes.MapRoute("HomePage",
                            "",
                            new { controller = "Home", action = "Index" },
                            new[] { "Web.ZhiXiao.Controllers" });
        }
    }
}