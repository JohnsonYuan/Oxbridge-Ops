using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.ZhiXiao;
using Nop.Extensions;
//using Nop.Admin.Helpers;
using Nop.Models.Customers;
using Nop.Services;
//using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Security;
using Nop.Services.ZhiXiao;
using Nop.Web.Framework.Controllers;
//using Nop.Services.Vendors;
using Nop.Web.Framework.Kendoui;
using Web.ZhiXiao.Factories;

namespace Web.ZhiXiao.Areas.Admin.Controllers
{
    public partial class CustomerTeamController : BaseAdminController
    {
        #region Fields
        private readonly IZhiXiaoService _customerTeamService;
        private readonly ICustomerService _customerService;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly ICacheManager _cacheManager;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ZhiXiaoSettings _zhiXiaoSettings;
        private readonly ICustomerModelFactory _customerModelFactory;
        #endregion

        #region Constructors

        public CustomerTeamController(
            IZhiXiaoService customerTeamService,
            ICustomerService customerService,
            ILocalizationService localizationService,
            ICustomerActivityService customerActivityService,
            IPermissionService permissionService,
            IWorkContext workContext,
            ICacheManager cacheManager,
            IDateTimeHelper dateTimeHelper,
            ZhiXiaoSettings zhiXiaoSettings,
            ICustomerModelFactory customerModelFactory)
        {
            this._customerTeamService = customerTeamService;
            this._customerService = customerService;
            this._localizationService = localizationService;
            this._customerActivityService = customerActivityService;
            this._permissionService = permissionService;
            this._workContext = workContext;
            this._cacheManager = cacheManager;
            this._dateTimeHelper = dateTimeHelper;
            this._zhiXiaoSettings = zhiXiaoSettings;
            this._customerModelFactory = customerModelFactory;
        }

        #endregion

        #region Customer teams

        public virtual ActionResult Index()
        {
            return RedirectToAction("List");
        }

        /// <summary>
        /// Team list
        /// </summary>
        /// <returns></returns>
        public virtual ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            CustomerTeamSearchModel searchModel = new CustomerTeamSearchModel();

            var values = from CustomerTeamType enumValue in Enum.GetValues(typeof(CustomerTeamType))
                         select new { ID = Convert.ToInt32(enumValue), Name = enumValue.GetDescription() };

            searchModel.AvailableTeamTypes = new SelectList(values, "ID", "Name", null).ToList();
            searchModel.AvailableTeamTypes.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            return View(searchModel);
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

            CustomerTeamType? teamType = searchModel.SearchTeamType > 0 ? (CustomerTeamType?)(searchModel.SearchTeamType) : null;

            var customerTeams = _customerTeamService.GetAllCustomerTeams(
                searchModel.SearchTeamNumber,
                teamType,
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

        #region Team diagarm

        public ActionResult Diagarm(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var team = _customerTeamService.GetCustomerTeamById(id);
            if (team == null)
                //No customer role found with the specified id
                return RedirectToRoute("HomePage");

            var model = _customerModelFactory.PrepareTeamDiagarmModel(team, true);

            ViewBag.IsAdmin = true;
            // 显示不在一个小组的child的tree
            ViewBag.FullChildList = _customerModelFactory.PrepareTeamDiagarmModel(team, false).AllUsers;

            return View("TeamDiagarm", model);
        }

        #endregion
    }
}