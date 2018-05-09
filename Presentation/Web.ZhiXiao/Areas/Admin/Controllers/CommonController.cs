using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.Mvc;
using Nop.Admin.Models.Common;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain;
using Nop.Core.Domain.Customers;
using Nop.Core.Infrastructure;
using Nop.Models.Customers;
using Nop.Services.Authentication;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Security;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Web.ZhiXiao.Factories;

namespace Web.ZhiXiao.Areas.Admin.Controllers
{
    public class CommonController : BasePublicController
    {
        #region Fields

        private readonly IWebHelper _webHelper;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILogger _logger;
        private readonly IWorkContext _workContext;
        private readonly IPermissionService _permissionService;
        private readonly ICustomerService _customerService;
        private readonly ILocalizationService _localizationService;
        private readonly HttpContextBase _httpContext;
        private readonly IMaintenanceService _maintenanceService;

        //fields for customer login/logont
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly ICustomerModelFactory _customerModelFactory;
        private readonly IAuthenticationService _authenticationService;
        private readonly StoreInformationSettings _storeInformationSettings;
        private readonly CustomerSettings _customerSettings;

        #endregion

        #region Constructors

        public CommonController(IWebHelper webHelper,
            IDateTimeHelper dateTimeHelper,
            ILogger logger,
            IWorkContext workContext,
            IPermissionService permissionService,
            ICustomerService customerService,
            ILocalizationService localizationService,
            HttpContextBase httpContext,
            IMaintenanceService maintenanceService,

            IGenericAttributeService genericAttributeService,
            ICustomerActivityService customerActivityService,
            ICustomerRegistrationService customerRegistrationService,
            ICustomerModelFactory customerModelFactory,
            IAuthenticationService authenticationService,
            StoreInformationSettings storeInformationSettings,
            CustomerSettings customerSettings
            )
        {
            this._webHelper = webHelper;
            this._dateTimeHelper = dateTimeHelper;
            this._logger = logger;
            this._workContext = workContext;
            this._permissionService = permissionService;
            this._customerService = customerService;
            this._localizationService = localizationService;
            this._httpContext = httpContext;
            this._maintenanceService = maintenanceService;

            this._genericAttributeService = genericAttributeService;
            this._customerActivityService = customerActivityService;
            this._customerRegistrationService = customerRegistrationService;
            this._customerModelFactory = customerModelFactory;
            this._authenticationService = authenticationService;
            this._storeInformationSettings = storeInformationSettings;
            this._customerSettings = customerSettings;
        }

        #endregion

        #region Utitlies

        private bool IsDebugAssembly(Assembly assembly)
        {
            var attribs = assembly.GetCustomAttributes(typeof(System.Diagnostics.DebuggableAttribute), false);

            if (attribs.Length > 0)
            {
                var attr = attribs[0] as System.Diagnostics.DebuggableAttribute;
                if (attr != null)
                {
                    return attr.IsJITOptimizerDisabled;
                }
            }

            return false;
        }

        private DateTime GetBuildDate(Assembly assembly, TimeZoneInfo target = null)
        {
            var filePath = assembly.Location;

            const int cPeHeaderOffset = 60;
            const int cLinkerTimestampOffset = 8;

            var buffer = new byte[2048];

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                stream.Read(buffer, 0, 2048);
            }

            var offset = BitConverter.ToInt32(buffer, cPeHeaderOffset);
            var secondsSince1970 = BitConverter.ToInt32(buffer, offset + cLinkerTimestampOffset);
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var linkTimeUtc = epoch.AddSeconds(secondsSince1970);

            var tz = target ?? TimeZoneInfo.Local;
            var localTime = TimeZoneInfo.ConvertTimeFromUtc(linkTimeUtc, tz);

            return localTime;
        }

        #endregion

        #region Methods

        [AdminAuthorize]
        public virtual ActionResult SystemInfo()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMaintenance))
                return AccessDeniedView();

            var model = new SystemInfoModel();
            model.NopVersion = NopVersion.CurrentVersion;
            try
            {
                model.OperatingSystem = Environment.OSVersion.VersionString;
            }
            catch (Exception) { }
            try
            {
                model.AspNetInfo = RuntimeEnvironment.GetSystemVersion();
            }
            catch (Exception) { }
            try
            {
                model.IsFullTrust = AppDomain.CurrentDomain.IsFullyTrusted.ToString();
            }
            catch (Exception) { }
            model.ServerTimeZone = TimeZone.CurrentTimeZone.StandardName;
            model.ServerLocalTime = DateTime.Now;
            model.UtcTime = DateTime.UtcNow;
            model.CurrentUserTime = _dateTimeHelper.ConvertToUserTime(DateTime.Now);
            model.HttpHost = _webHelper.ServerVariables("HTTP_HOST");
            foreach (var key in _httpContext.Request.ServerVariables.AllKeys)
            {
                if (key.StartsWith("ALL_")) continue;

                model.ServerVariables.Add(new SystemInfoModel.ServerVariableModel
                {
                    Name = key,
                    Value = _httpContext.Request.ServerVariables[key]
                });
            }

            //Environment.GetEnvironmentVariable("USERNAME");

            var trustLevel = CommonHelper.GetTrustLevel();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var loadedAssembly = new SystemInfoModel.LoadedAssembly
                {
                    FullName = assembly.FullName,

                };
                //ensure no exception is thrown
                try
                {
                    var canGetLocation = trustLevel >= AspNetHostingPermissionLevel.High && !assembly.IsDynamic;
                    loadedAssembly.Location = canGetLocation ? assembly.Location : null;
                    loadedAssembly.IsDebug = IsDebugAssembly(assembly);
                    loadedAssembly.BuildDate = canGetLocation ? (DateTime?)GetBuildDate(assembly, TimeZoneInfo.Local) : null;
                }
                catch (Exception) { }
                model.LoadedAssemblies.Add(loadedAssembly);
            }
            return View(model);
        }

        [AdminAuthorize]
        public virtual ActionResult PathTest()
        {
            return Content(_httpContext.Request.PhysicalApplicationPath);
        }

        [AdminAuthorize]
        public virtual ActionResult Maintenance()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMaintenance))
                return AccessDeniedView();

            var model = new MaintenanceModel();
            model.DeleteGuests.EndDate = DateTime.UtcNow.AddDays(-7);
            model.DeleteGuests.OnlyWithoutShoppingCart = true;
            model.DeleteAbandonedCarts.OlderThan = DateTime.UtcNow.AddDays(-182);
            return View(model);
        }

        [HttpPost]
        [AdminAuthorize]
        public virtual ActionResult BackupFiles(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMaintenance))
                return AccessDeniedKendoGridJson();

            var backupFiles = _maintenanceService.GetAllBackupFiles().ToList();

            var gridModel = new DataSourceResult
            {
                Data = backupFiles.Select(p => new {
                    p.Name,
                    Length = string.Format("{0:F2} Mb", p.Length / 1024f / 1024f),
                    Link = _webHelper.GetStoreLocation(false) + "db_backups/" + p.Name
                }),
                Total = backupFiles.Count
            };
            return Json(gridModel);
        }

        [HttpPost, ActionName("Maintenance")]
        [FormValueRequired("backup-database")]
        [AdminAuthorize]
        public virtual ActionResult BackupDatabase(MaintenanceModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMaintenance))
                return AccessDeniedView();

            try
            {
                _maintenanceService.BackupDatabase();
                this.SuccessNotification(_localizationService.GetResource("Admin.System.Maintenance.BackupDatabase.BackupCreated"));
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
            }

            return View(model);
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
        [AdminAuthorize]
        public virtual ActionResult ClearCache(string returnUrl = "")
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMaintenance))
                return RedirectToAction("AccessDenied");

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

        #endregion
    }
}