using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Common;
using Nop.Extensions;
using Nop.Services.BonusApp.Configuration;
using Nop.Services.BonusApp.Customers;
using Nop.Services.BonusApp.Logging;
using Nop.Services.Helpers;
using Nop.Services.Security;
using Nop.Services.ZhiXiao.BonusApp;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Models.Home;
using Web.ZhiXiao.Areas.YiJiaYi_Manage.Models.BonusApp;

namespace Web.ZhiXiao.Areas.YiJiaYi_Manage.Controllers.BonusApp
{
    public class BonusApp_HomeController : BaseAdminController
    {
        #region Fields
        private readonly IStoreContext _storeContext;
        private readonly AdminAreaSettings _adminAreaSettings;
        private readonly IBonusApp_SettingService _settingService;
        private readonly IPermissionService _permissionService;
        //private readonly IProductService _productService;
        //private readonly IOrderService _orderService;
        
        private readonly IBonusAppService _appService;
        private readonly IBonusApp_CustomerService _customerService;
        private readonly IBonusApp_CustomerActivityService _customerActivityService;
        //private readonly IReturnRequestService _returnRequestService;
        private readonly IWorkContext _workContext;
        private readonly ICacheManager _cacheManager;
        private readonly IDateTimeHelper _dateTimeHelper;

        #endregion

        #region Ctor

        public BonusApp_HomeController(IStoreContext storeContext,
            AdminAreaSettings adminAreaSettings,
            IBonusApp_SettingService settingService,
            IPermissionService permissionService,
            //IProductService productService,
            //IOrderService orderService,
            IBonusAppService appService,
            IBonusApp_CustomerService customerService,
            IBonusApp_CustomerActivityService customerActivityService,
            //IReturnRequestService returnRequestService,
            IWorkContext workContext,
            ICacheManager cacheManager,
            IDateTimeHelper dateTimeHelper)
        {
            this._storeContext = storeContext;
            this._adminAreaSettings = adminAreaSettings;
            this._settingService = settingService;
            this._permissionService = permissionService;
            //this._productService = productService;
            //this._orderService = orderService;

            this._appService = appService;
            this._customerService = customerService;
            this._customerActivityService = customerActivityService;
            //this._returnRequestService = returnRequestService;
            this._workContext = workContext;
            this._cacheManager = cacheManager;
            this._dateTimeHelper = dateTimeHelper;
        }

        #endregion

        #region Methods

        public virtual ActionResult Index()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            return View();
        }

        [ChildActionOnly]
        public virtual ActionResult CommonStatistics()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return Content("");

            var model = new CommonStatisticsModel();

            //model.NumberOfOrders = _orderService.SearchOrders(
            //    pageIndex: 0,
            //    pageSize: 1).TotalCount;

            model.NumberOfCustomers = _customerService.GetAllCustomers(
                pageIndex: 0,
                pageSize: 1).TotalCount;

            model.NumberOfPendingWithdrawRequest = _customerActivityService.GetAllWithdraws(
                isDone: false).TotalCount;
            
            //model.NumberOfPendingReturnRequests = _returnRequestService.SearchReturnRequests(
            //    rs: ReturnRequestStatus.Pending,
            //    pageIndex: 0,
            //    pageSize: 1).TotalCount;

            //model.NumberOfLowStockProducts = _productService.GetLowStockProducts(0, 0, 1).TotalCount +
            //                                 _productService.GetLowStockProductCombinations(0, 0, 1).TotalCount;

            return PartialView(model);
        }

        /// <summary>
        /// 业绩统计
        /// </summary>
        /// <returns></returns>
        public ActionResult MoneyOverview()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedKendoGridJson();

            BonusAppOverviewModel model = new BonusAppOverviewModel();

            model.AppStatus = _appService.GetAppStatus();

            // 统计log中花费金额(管理员, 普通用户注册)
            return View(model);
        }

        [HttpPost]
        public ActionResult MoneyOverview(DataSourceRequest command, BonusAppOverviewModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedKendoGridJson();

            DateTime? startDateValue = (model.CreatedOnFrom == null) ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.CreatedOnFrom.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.CreatedOnTo == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.CreatedOnTo.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            // logs
            var activityLog = _customerActivityService.GetAllMoneyLogs(
                startDateValue,
                endDateValue,
                customerId: null,
                moneyReturnStatusId: model.LogStatusId,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize,
                ipAddress: model.IpAddress);

            var gridModel = new DataSourceResult
            {
                Data = activityLog.Select((x, index) =>
                {
                    var m = x.ToModel();
                    m.Username = x.Customer.Username;
                    m.Nickname = x.Customer.Nickname;
                    m.OrderNum = _customerActivityService.GetMoneyLogOrderNumber(x); // 排序
                    m.CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc);
                    if (x.CompleteOnUtc.HasValue)
                        m.CompleteOn = _dateTimeHelper.ConvertToUserTime(x.CompleteOnUtc.Value, DateTimeKind.Utc);
                    return m;
                }),
                Total = activityLog.TotalCount
            };
            return Json(gridModel);
        }

        #endregion
    }
}