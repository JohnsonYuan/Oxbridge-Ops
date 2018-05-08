using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Security;
using Nop.Web.Framework.Controllers;
using Nop.Web.Models.Home;
using Web.ZhiXiao.Controllers;

namespace Web.ZhiXiao.Areas.Admin.Controllers
{
    public class HomeController : BaseAdminController
    {
        #region Fields
        private readonly IStoreContext _storeContext;
        private readonly AdminAreaSettings _adminAreaSettings;
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;
        //private readonly IProductService _productService;
        //private readonly IOrderService _orderService;
        private readonly ICustomerService _customerService;
        //private readonly IReturnRequestService _returnRequestService;
        private readonly IWorkContext _workContext;
        private readonly ICacheManager _cacheManager;

        #endregion

        #region Ctor

        public HomeController(IStoreContext storeContext,
            AdminAreaSettings adminAreaSettings,
            ISettingService settingService,
            IPermissionService permissionService,
            //IProductService productService,
            //IOrderService orderService,
            ICustomerService customerService,
            //IReturnRequestService returnRequestService,
            IWorkContext workContext,
            ICacheManager cacheManager)
        {
            this._storeContext = storeContext;
            this._adminAreaSettings = adminAreaSettings;
            this._settingService = settingService;
            this._permissionService = permissionService;
            //this._productService = productService;
            //this._orderService = orderService;
            this._customerService = customerService;
            //this._returnRequestService = returnRequestService;
            this._workContext = workContext;
            this._cacheManager = cacheManager;
        }

        #endregion

        public HomeController(IWorkContext workContext,
            ICustomerService customerService)
        {
            this._workContext = workContext;
            this._customerService = customerService;
        }

        public virtual ActionResult Index()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var model = new DashboardModel();
            model.IsLoggedInAsVendor = false;
            return View(model);
        }

        [ChildActionOnly]
        public virtual ActionResult CommonStatistics()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers) ||
                !_permissionService.Authorize(StandardPermissionProvider.ManageOrders) ||
                !_permissionService.Authorize(StandardPermissionProvider.ManageReturnRequests) ||
                !_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return Content("");

            var model = new CommonStatisticsModel();

            //model.NumberOfOrders = _orderService.SearchOrders(
            //    pageIndex: 0,
            //    pageSize: 1).TotalCount;

            model.NumberOfCustomers = _customerService.GetAllCustomers(
                customerRoleIds: new[] { _customerService.GetCustomerRoleBySystemName(SystemCustomerRoleNames.Registered).Id },
                pageIndex: 0,
                pageSize: 1).TotalCount;

            //model.NumberOfPendingReturnRequests = _returnRequestService.SearchReturnRequests(
            //    rs: ReturnRequestStatus.Pending,
            //    pageIndex: 0,
            //    pageSize: 1).TotalCount;

            //model.NumberOfLowStockProducts = _productService.GetLowStockProducts(0, 0, 1).TotalCount +
            //                                 _productService.GetLowStockProductCombinations(0, 0, 1).TotalCount;

            return PartialView(model);
        }

        //page not found
        public ActionResult Users()
        {
            var customers = _customerService.GetAllCustomers();
            return View(customers);
        }
    }
}