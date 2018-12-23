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
                return "BonusApp";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            var config = EngineContext.Current.Resolve<NopConfig>();

            var bonusAppDomain = config.BonusAppDomain;

            context.MapRoute("BonusApp_HomePage",
                            "",
                            new { controller = "Home", action = "Index" },
                            new { host = new BonusAppHostRouteConstraint(bonusAppDomain) },
                            new[] { "Web.ZhiXiao.Areas.BunusApp.Controllers" });

            context.MapRoute("BonusApp_Login",
                            "login",
                            new { controller = "Common", action = "Login" },
                            new { host = new BonusAppHostRouteConstraint(bonusAppDomain) },
                            new[] { "Web.ZhiXiao.Areas.BunusApp.Controllers" });

            context.MapRoute("BonusApp_UserCenter",
                            "user",
                            new { controller = "Home", action = "UserCenter" },
                            new { host = new BonusAppHostRouteConstraint(bonusAppDomain) },
                            new[] { "Web.ZhiXiao.Areas.BunusApp.Controllers" });

            context.MapRoute("BonusApp_Comments",
                                     "comment",
                                     new { controller = "Home", action = "Comment" },
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