using System.Web.Mvc;

namespace Nop.Web.Framework.Controllers
{
    //[CheckAffiliate]
    //[StoreClosed]
    //[PublicStoreAllowNavigation]
    //[LanguageSeoCode]
    //[NopHttpsRequirement(SslRequirement.NoMatter)]
    //[WwwRequirement]
    public abstract partial class BasePublicController : BaseController
    {
        public ActionResult ScrollEndContent()
        {
            return Content(".pagination__next");
        }
    }
}