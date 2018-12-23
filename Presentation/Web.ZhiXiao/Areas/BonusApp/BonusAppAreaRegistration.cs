using System.Web.Mvc;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Web.Infrastructure.Mvc.Routes;

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
            var config = EngineContext.Current.Resolve<NopConfig>();

            var bonusAppDomain = config.BonusAppDomain;
            
            context.MapRoute("BonusApp_HomePage",
                            "",
                            new { controller = "Home", action = "Index" },
                            //new { },
                            new[] { "Web.ZhiXiao.Areas.BunusApp.Controllers" });

            context.MapRoute("BonusApp_Login",
                            "login",
                            new { controller = "Common", action = "Login" },
                            new { host = new BonusAppHostRouteConstraint(bonusAppDomain) },
                            new[] { "Web.ZhiXiao.Areas.BunusApp.Controllers" });

            context.MapRoute(
                "BonusApp_default",
                "{controller}/{action}/{id}",
                new { id = UrlParameter.Optional },
                new { host = new BonusAppHostRouteConstraint(bonusAppDomain) },
                new[] { "Web.ZhiXiao.Areas.BunusApp.Controllers" }
            );
        }
    }
}