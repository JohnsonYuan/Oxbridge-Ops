using System.Web.Mvc;

namespace Web.ZhiXiao.Areas.Admin
{
    public class AdminAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Admin";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute("AdminHomePage",
                            "Admin",
                            new { controller = "Home", action = "Index" },
                            new[] { "Web.ZhiXiao.Areas.Admin.Controllers" });

            context.MapRoute(
                "Admin_default",
                "Admin/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                new[] { "Web.ZhiXiao.Areas.Admin.Controllers" }
            );
        }
    }
}