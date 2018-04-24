using System.Web.Mvc;
using System.Web.Routing;
using Nop.Core.Infrastructure;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;

namespace Web.ZhiXiao.Controllers
{
    //[CheckAffiliate]
    [StoreClosed]
    //[PublicStoreAllowNavigation]
    //[LanguageSeoCode]
    //[NopHttpsRequirement(SslRequirement.NoMatter)]
    //[WwwRequirement]
    public abstract partial class BasePublicController : BaseController
    {
    }
}