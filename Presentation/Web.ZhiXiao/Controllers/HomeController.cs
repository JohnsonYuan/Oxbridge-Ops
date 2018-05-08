using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Services.Configuration;
using Nop.Services.Security;
using Nop.Web.Framework.Controllers;

namespace Web.ZhiXiao.Controllers
{
    public class HomeController : BaseAdminController
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public HomeController(
            IPermissionService permissionService,
            IWorkContext workContext)
        {
            this._permissionService = permissionService;

            this._workContext = workContext;
        }

        #endregion

        public virtual ActionResult Index()
        {
            var currentCustomer = _workContext.CurrentCustomer;
            if (currentCustomer.IsAdmin() || currentCustomer.IsManager())
                return RedirectToRoute("AdminHomePage");

            return RedirectToAction("Index", "Customer");
        }
    }
}