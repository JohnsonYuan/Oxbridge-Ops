using System;
using System.Data.Entity.Infrastructure;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Domain.BonusApp;
using Nop.Core.Domain.BonusApp.Configuration;
using Nop.Core.Domain.BonusApp.Customers;
using Nop.Core.Domain.BonusApp.Logging;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.BonusApp.Configuration;
using Nop.Services.BonusApp.Customers;
using Nop.Services.BonusApp.Logging;
using Nop.Services.Security;
using Nop.Services.ZhiXiao.BonusApp;
using Nop.Web.Framework.Controllers;

namespace Web.ZhiXiao.Areas.BonusApp.Controllers
{
    public class InstallController : BasePublicController
    {
        #region Fields
        private IDbContext _dbContext;
        private IWebHelper _webHelper;
        private IBonusAppService _appService;
        private IBonusApp_CustomerService _customerService;
        private IEncryptionService _encryptionService;
        private IBonusApp_SettingService _settingService;
        private IBonusApp_CustomerActivityService _customerActivityService;

        #endregion

        #region Ctor

        public InstallController(
            IBonusAppService appService,
            IBonusApp_CustomerService customerService,
            IBonusApp_SettingService settingService,
            IBonusApp_CustomerActivityService customerActivityService,
            IEncryptionService encryptionService,
            IWebHelper webHelper,
            IDbContext dbContext)
        {
            this._appService = appService;
            this._customerService = customerService;
            this._settingService = settingService;
            this._customerActivityService = customerActivityService;
            this._encryptionService = encryptionService;
            this._webHelper = webHelper;
            this._dbContext = dbContext;
        }

        #endregion

        #region Utilities

        #endregion

        public ActionResult Index()
        {
            // 0. init app status and customer
            var appStatus = new Nop.Core.Domain.BonusApp.BonusAppStatus
            {
                CurrentMoney = 0,
                WaitingUserCount = 0,
                CompleteUserCount = 0,
                MoneyPaied = 0
            };
            _appService.InsertBonusAppStatus(appStatus);

            // 1. pool setting
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
            _settingService.SaveSetting(defaultSetting);

            // 2. Log type (TODO)
            BonusApp_ActivityLogType logType = new BonusApp_ActivityLogType
            {
                Enabled = true,
                SystemKeyword = BonusAppConstants.LogType_AddNewCustomer,
                Name = "Add a new customer"
            };
            _customerActivityService.InsertActivityType(logType);

            _customerActivityService.InsertActivityType(logType);

            logType = new BonusApp_ActivityLogType
            {
                Enabled = true,
                SystemKeyword = BonusAppConstants.LogType_Bonus_PoolAdd,
                Name = "奖金池增加"
            };
            _customerActivityService.InsertActivityType(logType);

            logType = new BonusApp_ActivityLogType
            {
                Enabled = true,
                SystemKeyword = BonusAppConstants.LogType_Bonus_PoolMinus,
                Name = "奖金池减少"
            };
            _customerActivityService.InsertActivityType(logType);

            // 3. customers and money log
            const int totalUserCount = 80;

            for (int i = 0; i < totalUserCount; i++)
            {
                var randomMoney = CommonHelper.GenerateRandomInteger(20, 5000);

                var waitingCustomer = new BonusApp_Customer
                {
                    Username = "testUser_" + i,
                    Password = _encryptionService.CreatePasswordHash("123456", defaultSetting.CustomerPasswordSalt, "md5"),
                    AvatarUrl = (i % 3 == 0 ? "/Content/bonus/img/41.png" : "/Content/bonus/img/42.png"),
                    Nickname = "waiting_" + i,
                    PhoneNumber = "1345644",
                    Active = true,
                    Deleted = false,
                    LastIpAddress = _webHelper.GetCurrentIpAddress(),
                    CreatedOnUtc = DateTime.UtcNow,
                    LastActivityDateUtc = DateTime.UtcNow.AddMinutes(CommonHelper.GenerateRandomInteger(-300, 0)),
                    Money = 0,
                };
                _customerService.InsertCustomer(waitingCustomer);

                // log and update pool money, check to see if need return money
                _customerActivityService.InsertMoneyLog(waitingCustomer, randomMoney, "金额 {0}", randomMoney);
            }

            // random comment
            const int commentCount = 200;
            var customers = _customerService.GetAllCustomers();
            for (int i = 0; i < customers.Count; i++)
            {
                _customerService.InsertComment(new BonusApp_CustomerComment
                {
                    CustomerId = customers[i].Id,
                    Comment = i + " 随机测试评论" + ", 用户" + customers[i].Nickname,
                    Rate = CommonHelper.GenerateRandomInteger(1, 6),
                    Enabled = true,
                    IpAddress = _webHelper.GetCurrentIpAddress(),
                    CreatedOnUtc = DateTime.UtcNow
                });
            }

            return Content("Success");
        }

        public ActionResult Comment()
        {
            var customers = _customerService.GetAllCustomers();
            for (int i = 0; i < customers.Count; i++)
            {
                _customerService.InsertComment(new BonusApp_CustomerComment
                {
                    CustomerId = customers[i].Id,
                    Comment = i + " 随机测试评论" + ", 用户" + customers[i].Nickname,
                    Rate = CommonHelper.GenerateRandomInteger(1, 6),
                    Enabled = true,
                    IpAddress = _webHelper.GetCurrentIpAddress(),
                    CreatedOnUtc = DateTime.UtcNow
                });
            }

            return Content("Success");
        }

        public ActionResult DbSql()
        {
             var dbCreationScript = ((IObjectContextAdapter)_dbContext).ObjectContext.CreateDatabaseScript();
            return Content(dbCreationScript);
        }
    }
}