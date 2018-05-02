using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.ZhiXiao;
using Nop.Data;
using Nop.Extensions;
//using Nop.Admin.Helpers;
using Nop.Models.Customers;
//using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Services.ZhiXiao;
//using Nop.Services.Vendors;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;

namespace Web.ZhiXiao.Controllers
{
    public partial class CustomerTeamController : BaseAdminController
    {
        #region Fields
        private readonly ICustomerTeamService _customerTeamService;
        private readonly ICustomerService _customerService;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly ICacheManager _cacheManager;
        private readonly IDateTimeHelper _dateTimeHelper;
        #endregion

        #region Constructors

        public CustomerTeamController(
            ICustomerTeamService customerTeamService,
            ICustomerService customerService,
            ILocalizationService localizationService,
            ICustomerActivityService customerActivityService,
            IPermissionService permissionService,
            IWorkContext workContext,
            ICacheManager cacheManager,
            IDateTimeHelper dateTimeHelper)
        {
            this._customerTeamService = customerTeamService;
            this._customerService = customerService;
            this._localizationService = localizationService;
            this._customerActivityService = customerActivityService;
            this._permissionService = permissionService;
            this._workContext = workContext;
            this._cacheManager = cacheManager;
            this._dateTimeHelper = dateTimeHelper;
        }

        #endregion

        #region Customer teams

        public virtual ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public virtual ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            return View();
        }

        [HttpPost]
        public virtual ActionResult List(DataSourceRequest command, CustomerTeamSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedKendoGridJson();

            DateTime? startDateValue = (searchModel.CreatedOnFrom == null) ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.CreatedOnFrom.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (searchModel.CreatedOnTo == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.CreatedOnTo.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            var customerTeams = _customerTeamService.GetAllCustomerTeams(searchModel.
                SearchTeamNumber,
                searchModel.CreatedOnFrom,
                searchModel.CreatedOnTo,
                command.Page - 1,
                command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = customerTeams.Select(x =>
                {
                    var m = x.ToModel();
                    m.CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc);
                    return m;
                }),
                Total = customerTeams.TotalCount
            };

            return Json(gridModel);
        }
        #endregion

        #region Team test
        public ActionResult TeamInfo(int id = 1)
        {
            //List<object> result = new List<object>();
            //for (int i = 1; i <= 1; i++)
            //{
            //    var customer = _customerService.GetCustomerByUsername("user_" + i);

            //    result.Add(new { TeamInfo = customer.CustomerTeam.Customers });
            //}

            //return Json(result, JsonRequestBehavior.AllowGet);

            var team = _customerTeamService.GetCustomerTeamById(id);

            return Json(team, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}