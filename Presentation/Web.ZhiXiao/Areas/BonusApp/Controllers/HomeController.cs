using System;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Domain.BonusApp.Configuration;
using Nop.Core.Domain.BonusApp.Customers;
using Nop.Core.Domain.BonusApp.Logging;
using Nop.Core.Infrastructure;
using Nop.Services.BonusApp.Configuration;
using Nop.Services.BonusApp.Customers;
using Nop.Services.BonusApp.Logging;
using Nop.Services.Security;
using Nop.Services.ZhiXiao.BonusApp;
using Nop.Web.Framework.Controllers;

namespace Web.ZhiXiao.Areas.BunusApp.Controllers
{
    public class HomeController : BasePublicController
    {
        #region Fields

        private IWebHelper _webHelper;
        private IBonusApp_CustomerService _customerService;
        private IBonusApp_CustomerActivityService _customerActivityService;

        #endregion

        #region Ctor

        public HomeController(
            IBonusApp_CustomerService customerService,
            IBonusApp_CustomerActivityService customerActivityService,
            IWebHelper webHelper)
        {
            this._customerService = customerService;
            this._customerActivityService = customerActivityService;
            this._webHelper = webHelper;
        }

        #endregion

        #region Utilities

        #endregion

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Index2()
        {
            return Content(_webHelper.GetThisPageUrl(false));
        }

        public ActionResult Index3()
        {
            return Content(Request.ServerVariables["HTTP_HOST"]);
        }

        public ActionResult TestData()
        {
            // 0. init app status and customer
            var appService = EngineContext.Current.Resolve<IBonusAppService>();
            var appStatus = new Nop.Core.Domain.BonusApp.BonusAppStatus
            {
                CurrentMoney = 0,
                WaitingUserCount = 0,
                CompleteUserCount = 0,
                MoneyPaied = 0
            };
            appService.InsertBonusAppStatus(appStatus);

            // 1. pool setting
            var settingService = EngineContext.Current.Resolve<IBonusApp_SettingService>();
            var defaultSetting = new BonusAppSettings
            {
                SiteTitle = "奖金池系统",
                // 存入奖金池比例
                SaveToAppMoneyPercent = 0.2,
                // 用户返回金额比例
                UserReturnMoneyPercent = 1,
                Withdraw_Rate = 1,
                CustomerPasswordSalt = "Z3GP1bc="
            };
            settingService.SaveSetting(defaultSetting);

            // 2. Log type (TODO)
            var activityService = EngineContext.Current.Resolve<IBonusApp_CustomerActivityService>();
            BonusApp_ActivityLogType logType = new BonusApp_ActivityLogType
            {
                Enabled = true,
                SystemKeyword = "AddNewCustomer",
                Name = "Add a new customer"
            };
            activityService.InsertActivityType(logType);

            logType = new BonusApp_ActivityLogType
            {
                Enabled = true,
                SystemKeyword = "Bonus_MoneyReturn",
                Name = "奖金池返还金额"
            };
            activityService.InsertActivityType(logType);

            logType = new BonusApp_ActivityLogType
            {
                Enabled = true,
                SystemKeyword = "Bonus_PoolAdd",
                Name = "奖金池增加"
            };
            activityService.InsertActivityType(logType);

            logType = new BonusApp_ActivityLogType
            {
                Enabled = true,
                SystemKeyword = "Bonus_PoolMinus",
                Name = "奖金池减少"
            };
            activityService.InsertActivityType(logType);

            // 3. customers and money log
            var customerService = EngineContext.Current.Resolve<IBonusApp_CustomerService>();
            var encryService = EngineContext.Current.Resolve<IEncryptionService>();

            const int totalUserCount = 80;

            for (int i = 0; i < totalUserCount; i++)
            {
                var randomMoney = CommonHelper.GenerateRandomInteger(20, 5000);

                var waitingCustomer = new BonusApp_Customer
                {
                    Username = "testUser_" + i,
                    Password = encryService.CreatePasswordHash("123456", defaultSetting.CustomerPasswordSalt, "md5"),
                    AvatarUrl = (i % 3 == 0 ? "/Content/bonus/img/41.png" : "/Content/bonus/img/42.png"),
                    Nickname = "waiting_" + i,
                    PhoneNumber = "1345644",
                    Active = true,
                    Deleted = false,
                    LastIpAddress = "",
                    CreatedOnUtc = DateTime.UtcNow,
                    LastActivityDateUtc = DateTime.UtcNow.AddMinutes(CommonHelper.GenerateRandomInteger(-300, 0)),
                    Money = 0,
                };
                customerService.InsertCustomer(waitingCustomer);

                // log and update pool money
                activityService.InsertMoneyLog(waitingCustomer, randomMoney, "金额 {0}", randomMoney);

                // check to see if need return money
                appService.ReturnUserMoneyIfNeeded();
            }

            return Content("Success");
        }
    }
}