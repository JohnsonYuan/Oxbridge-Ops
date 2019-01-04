using System.Web.Mvc;

namespace Web.ZhiXiao.Controllers
{
    public partial class KeepAliveController : Controller
    {
        public virtual ActionResult Index()
        {
            return Content("I am alive!");
        }
    }
}