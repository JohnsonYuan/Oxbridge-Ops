using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Security;

namespace Web.ZhiXiao.Controllers
{
    //[NopHttpsRequirement(SslRequirement.Yes)]
    [AdminAuthorize]
    public abstract partial class BaseAdminController : BaseController
    {}
}