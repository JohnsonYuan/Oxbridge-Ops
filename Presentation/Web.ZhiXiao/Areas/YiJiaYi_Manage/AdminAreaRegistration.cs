using System.Web.Mvc;

namespace Web.ZhiXiao.Areas.Admin
{
    public class AdminAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "YiJiaYi_Manage";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute("AdminHomePage",
                            "YiJiaYi_Manage",
                            new { controller = "Home", action = "Index" },
                            new[] { "Web.ZhiXiao.Areas.Admin.Controllers" });

            context.MapRoute("AdminLogin",
                            "YiJiaYi_Manage/yijiayi",
                            new { controller = "Common", action = "Login" },
                            new[] { "Web.ZhiXiao.Areas.Admin.Controllers" });

            context.MapRoute(
                "Admin_default",
                "YiJiaYi_Manage/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                new[] { "Web.ZhiXiao.Areas.Admin.Controllers",  // zhixiao controllers
                        "Web.ZhiXiao.Areas.YiJiaYi_Manage.Controllers.BonusApp"
                }
            );
        }
    }
}