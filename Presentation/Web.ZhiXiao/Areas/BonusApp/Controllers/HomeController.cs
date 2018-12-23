using System.Web.Mvc;
using Nop.Web.Framework.Controllers;

namespace Web.ZhiXiao.Areas.BunusApp.Controllers
{
    public class HomeController : BasePublicController
    {
        public ActionResult Index()
        {
            return Content(Request.Url.Host);
        }
    }
}