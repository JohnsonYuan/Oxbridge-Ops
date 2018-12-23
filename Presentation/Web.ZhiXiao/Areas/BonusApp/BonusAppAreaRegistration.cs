using System.Web.Mvc;

namespace Web.ZhiXiao.Areas.BunusApp
{
    public class BunusAppAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "BunusApp";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute("BonusApp_HomePage",
                            "",
                            new { controller = "Home", action = "Index" },
                            new[] { "Web.ZhiXiao.Areas.BunusApp.Controllers" });

            context.MapRoute("BonusApp_Login",
                            "login",
                            new { controller = "Common", action = "Login" },
                            new[] { "Web.ZhiXiao.Areas.BunusApp.Controllers" });

            context.MapRoute(
                "BonusApp_default",
                "{controller}/{action}/{id}",
                new { id = UrlParameter.Optional },
                new[] { "Web.ZhiXiao.Areas.BunusApp.Controllers" }
            );
        }
    }
}