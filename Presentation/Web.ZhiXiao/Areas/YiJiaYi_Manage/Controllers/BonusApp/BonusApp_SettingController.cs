using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Domain.BonusApp.Configuration;
using Nop.Extensions;
using Nop.Services.BonusApp.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Framework.Controllers;
using Web.ZhiXiao.Areas.YiJiaYi_Manage.Models.BonusApp;

namespace Web.ZhiXiao.Areas.YiJiaYi_Manage.Controllers.BonusApp
{
    public class BonusApp_SettingController : BaseAdminController
    {
        #region Fields

        private readonly IBonusApp_SettingService _settingService;
        private readonly IPermissionService _permissionService;
        private readonly IStoreService _storeService;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerActivityService _customerActivityService;

        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public BonusApp_SettingController(IBonusApp_SettingService settingService,
            IPermissionService permissionService,
            IStoreService storeService,
            ILocalizationService localizationService,
            ICustomerActivityService customerActivityService,
            IWorkContext workContext)
        {
            this._settingService = settingService;
            this._permissionService = permissionService;
            this._storeService = storeService;
            this._localizationService = localizationService;
            this._customerActivityService = customerActivityService;
            this._workContext = workContext;
        }

        #endregion

        #region Methods

        public ActionResult Index()
        {
            return Content("INdex bonus");
        }
        public virtual ActionResult BonusApp()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var appSettings = _settingService.LoadSetting<BonusAppSettings>(storeScope);
            var model = appSettings.ToModel();
            //model.ActiveStoreScopeConfiguration = storeScope;
            if (storeScope > 0)
            {
                //model.Enabled_OverrideForStore = _settingService.SettingExists(newsSettings, x => x.Enabled, storeScope);
                //model.AllowNotRegisteredUsersToLeaveComments_OverrideForStore = _settingService.SettingExists(newsSettings, x => x.AllowNotRegisteredUsersToLeaveComments, storeScope);
                //model.NotifyAboutNewNewsComments_OverrideForStore = _settingService.SettingExists(newsSettings, x => x.NotifyAboutNewNewsComments, storeScope);
                //model.ShowNewsOnMainPage_OverrideForStore = _settingService.SettingExists(newsSettings, x => x.ShowNewsOnMainPage, storeScope);
                //model.MainPageNewsCount_OverrideForStore = _settingService.SettingExists(newsSettings, x => x.MainPageNewsCount, storeScope);
                //model.NewsArchivePageSize_OverrideForStore = _settingService.SettingExists(newsSettings, x => x.NewsArchivePageSize, storeScope);
                //model.ShowHeaderRssUrl_OverrideForStore = _settingService.SettingExists(newsSettings, x => x.ShowHeaderRssUrl, storeScope);
                //model.NewsCommentsMustBeApproved_OverrideForStore = _settingService.SettingExists(newsSettings, x => x.NewsCommentsMustBeApproved, storeScope);
            }
            return View(model);
        }
        [HttpPost]
        public virtual ActionResult BonusApp(BonusAppSettingsModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var appSettings = _settingService.LoadSetting<BonusAppSettings>(storeScope);
            appSettings = model.ToEntity(appSettings);

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            //_settingService.SaveSettingOverridablePerStore(appSettings, x => x.SiteTitle, model.Enabled_OverrideForStore, storeScope, false);
            // _settingService.SaveSettingOverridablePerStore(appSettings, x => x.AllowNotRegisteredUsersToLeaveComments, model.AllowNotRegisteredUsersToLeaveComments_OverrideForStore, storeScope, false);
            // _settingService.SaveSettingOverridablePerStore(appSettings, x => x.NotifyAboutNewNewsComments, model.NotifyAboutNewNewsComments_OverrideForStore, storeScope, false);
            //_settingService.SaveSettingOverridablePerStore(appSettings, x => x.ShowNewsOnMainPage, model.ShowNewsOnMainPage_OverrideForStore, storeScope, false);
            //_settingService.SaveSettingOverridablePerStore(appSettings, x => x.MainPageNewsCount, model.MainPageNewsCount_OverrideForStore, storeScope, false);
            // _settingService.SaveSettingOverridablePerStore(appSettings, x => x.NewsArchivePageSize, model.NewsArchivePageSize_OverrideForStore, storeScope, false);
            //_settingService.SaveSettingOverridablePerStore(appSettings, x => x.ShowHeaderRssUrl, model.ShowHeaderRssUrl_OverrideForStore, storeScope, false);
            _settingService.SaveSetting(appSettings, x => x.SiteTitle, clearCache: false);
            _settingService.SaveSetting(appSettings, x => x.SaveToAppMoneyPercent, clearCache: false);

            _settingService.SaveSetting(appSettings, x => x.AuthCookieName, clearCache: false);
            _settingService.SaveSetting(appSettings, x => x.UserReturnMoneyPercent, clearCache: false);
            _settingService.SaveSetting(appSettings, x => x.Withdraw_Rate, clearCache: false);
            //_settingService.SaveSetting(appSettings, x => x.CustomerPasswordSalt, clearCache: false);
            //_settingService.SaveSetting(appSettings, x => x.HashedPasswordFormat, clearCache: false);

            //now clear settings cache
            _settingService.ClearCache();

            //activity log
            // _customerActivityService.InsertActivity("EditSettings", _localizationService.GetResource("ActivityLog.EditSettings", logIfNotFound: false));
            _customerActivityService.InsertActivity("EditSettings", "bonus appsetting 设置");


            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));
            return RedirectToAction("BonusApp");
        }


        #endregion
    }
}