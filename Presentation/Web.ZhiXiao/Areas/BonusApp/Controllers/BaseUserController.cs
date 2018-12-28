using Nop.Web.Framework.Security;
using Web.ZhiXiao.Areas.BonusApp.Mvc;

namespace Web.ZhiXiao.Areas.BonusApp.Controllers
{
    [AdminAntiForgery]
    [BonusAppCustomerAuthorize]
    public class BaseUserController : BonusAppBaseController
    {
    }
}