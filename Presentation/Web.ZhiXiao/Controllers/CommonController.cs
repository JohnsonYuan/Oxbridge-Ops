using Nop.Core.Caching;
using Nop.Core.Infrastructure;
using Nop.Services.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Web.ZhiXiao.Controllers
{
    public class CommonController : BasePublicController
    {
        #region Fields

        private readonly IPermissionService _permissionService;

        #endregion

        public CommonController(IPermissionService permissionService)
        {
            this._permissionService = permissionService;
        }

        //page not found
        public virtual ActionResult PageNotFound()
        {
            this.Response.StatusCode = 404;
            this.Response.TrySkipIisCustomErrors = true;
            this.Response.ContentType = "text/html";

            return View();
        }

        [HttpPost]
        public virtual ActionResult ClearCache(string returnUrl = "")
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMaintenance))
                return Content("没有权限");

            var cacheManager = EngineContext.Current.ContainerManager.Resolve<ICacheManager>("nop_cache_static");
            cacheManager.Clear();

            //home page
            if (String.IsNullOrEmpty(returnUrl))
                return RedirectToAction("Index", "Home");
            //prevent open redirection attack
            if (!Url.IsLocalUrl(returnUrl))
                return RedirectToAction("Index", "Home");
            return Redirect(returnUrl);
        }
    }
}