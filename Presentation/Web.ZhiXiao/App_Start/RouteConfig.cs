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

            routes.MapRoute("HomePage",
                            "",
                            new { controller = "Home", action = "Index" },
                            new[] { "Web.ZhiXiao.Controllers" });

            // customer
            //customer account links
            routes.MapRoute("CustomerInfo",
                            "customer/info",
                            new { controller = "Customer", action = "Info" },
                            new[] { "Web.ZhiXiao.Controllers" });

            //login
            routes.MapRoute("Login",
                            "login/",
                            new { controller = "Common", action = "Login" },
                            new[] { "Web.ZhiXiao.Controllers" });
            //register
            routes.MapRoute("Register",
                            "register/",
                            new { controller = "Common", action = "Register" },
                            new[] { "Web.ZhiXiao.Controllers" });
            //logout
            routes.MapRoute("Logout",
                            "logout/",
                            new { controller = "Common", action = "Logout" },
                            new[] { "Web.ZhiXiao.Controllers" });

            //passwordrecovery
            routes.MapRoute("PasswordRecovery",
                            "passwordrecovery",
                            new { controller = "Common", action = "PasswordRecovery" },
                            new[] { "Nop.Web.Controllers" });
            //password recovery confirmation
            routes.MapRoute("PasswordRecoveryConfirm",
                            "passwordrecovery/confirm",
                            new { controller = "Common", action = "PasswordRecoveryConfirm" },
                            new[] { "Nop.Web.Controllers" });


            //robots.txt
            routes.MapRoute("robots.txt",
                            "robots.txt",
                            new { controller = "Common", action = "RobotsTextFile" },
                            new[] { "Web.ZhiXiao.Controllers" });

            //sitemap (XML)
            routes.MapRoute("sitemap.xml",
                            "sitemap.xml",
                            new { controller = "Common", action = "SitemapXml" },
                            new[] { "Web.ZhiXiao.Controllers" });
            routes.MapRoute("sitemap-indexed.xml",
                            "sitemap-{Id}.xml",
                            new { controller = "Common", action = "SitemapXml" },
                            new { Id = @"\d+" },
                            new[] { "Web.ZhiXiao.Controllers" });

            //store closed
            routes.MapRoute("StoreClosed",
                            "storeclosed",
                            new { controller = "Common", action = "StoreClosed" },
                            new[] { "Web.ZhiXiao.Controllers" });

            //install
            routes.MapRoute("Installation",
                            "install",
                            new { controller = "Install", action = "Index" },
                            new[] { "Web.ZhiXiao.Controllers" });

            //page not found
            routes.MapRoute("PageNotFound",
                            "page-not-found",
                            new { controller = "Common", action = "PageNotFound" },
                            new[] { "Web.ZhiXiao.Controllers" });

            // Default
            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional }
                //, new[] { "Web.ZhiXiao.Controllers" }
            );
        }
    }
}