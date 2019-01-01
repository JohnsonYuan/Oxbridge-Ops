using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Domain.BonusApp;
using Nop.Core.Infrastructure;
using Nop.Services.BonusApp.Customers;
using Nop.Services.BonusApp.Logging;
using Nop.Services.Helpers;
using Nop.Services.Media;
using Nop.Services.ZhiXiao.BonusApp;
using Web.ZhiXiao.Areas.BonusApp.Factories;
using Web.ZhiXiao.Areas.BonusApp.Models;

namespace Web.ZhiXiao.Areas.BonusApp.Controllers
{
    public class UserController : BaseUserController
    {
        #region Fields

        private const int PAGE_SIZE = 15;

        private IPictureService _pictureService;
        private IBonusAppService _appService;
        private IBonusApp_CustomerService _customerService;
        private IBonusApp_CustomerActivityService _customerActivityService;

        private IDateTimeHelper _dateTimeHelper;
        private IWebHelper _webHelper;
        private IWorkContext _workContext;
        private CustomerModelFactory _factory;

        #endregion

        #region Ctor

        public UserController(
            IPictureService pictureService,
            IBonusAppService appService,
            IBonusApp_CustomerService customerService,
            IBonusApp_CustomerActivityService customerActivityService,
            IDateTimeHelper dateTimeHelper,
            IWebHelper webHelper,
            IWorkContext workContext,
            CustomerModelFactory factory)
        {
            this._pictureService = pictureService;
            this._appService = appService;
            this._customerService = customerService;
            this._customerActivityService = customerActivityService;
            this._dateTimeHelper = dateTimeHelper;
            this._webHelper = webHelper;

            this._workContext = workContext;
            this._factory = factory;
        }

        #endregion

        #region Utilities

        private CustomerModel PrepareCustomerModel()
        {
            var customer = _workContext.CurrentBonusAppCustomer;
            return _factory.PrepareCustomerModel(customer);
        }

        #endregion

        public ActionResult Index()
        {
            var model = PrepareCustomerModel();
            return View(model);
        }

        #region user info

        public ActionResult Edit()
        {
            var model = PrepareCustomerModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult UpdatePwd(
            string oldPwd,
            string newPwd,
            string newPwd2)
        {
            if (string.IsNullOrEmpty(oldPwd))
                return ErrorJson("请输入原密码");
            if (string.IsNullOrEmpty(newPwd))
                return ErrorJson("请输入新密码");
            if (string.IsNullOrEmpty(newPwd2))
                return ErrorJson("请再次输入新密码");

            if (newPwd != newPwd2)
                return ErrorJson("两次密码不一致");

            var loginResult =
               _customerService.ValidateCustomer(_workContext.CurrentBonusAppCustomer.Username, oldPwd);

            if (loginResult != Nop.Core.Domain.Customers.CustomerLoginResults.Successful)
                return ErrorJson("原密码错误");

            _customerService.UpdateCustomerPassword(_workContext.CurrentBonusAppCustomer,newPwd);
            _customerActivityService.InsertActivity(BonusAppConstants.LogType_User_ChangePwd, "旧密码: {0}, 新密码: {1}", oldPwd, newPwd);

            return SuccessJson("修改密码成功");
        }

        #endregion

        #region tixian and log

        public ActionResult TiXian()
        {
            var model = PrepareCustomerModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult TiXian(decimal money,
            string account,
            string type)
        {
            if (string.IsNullOrEmpty(account))
                return ErrorJson("请输入提现账号");
            if (string.IsNullOrEmpty(type))
                return ErrorJson("请输入提现类型");

            var moneyRegex = new Regex(@"^\d*(\.\d{1,2})?$");
            if (!moneyRegex.IsMatch(money.ToString()))
                return ErrorJson("提现金额最多两位小数");

            // 实际提取金额会扣除手续费
            // var actualAmount = Convert.ToInt32(_zhiXiaoSettings.Withdraw_Rate * amount);
            var actualAmount = money;

            var customer = _workContext.CurrentBonusAppCustomer;
            
            if (customer.Money <= 0)
                return ErrorJson("当前余额为0, 不能提现");

            if (actualAmount > customer.Money)
                return ErrorJson("提现金额超出当前余额");

            _customerActivityService.InsertWithdraw(customer,
                (double)actualAmount,
                "提现申请金额{0}, 账号: {1}, 类型: {2}",
                actualAmount,
                account,
                type);

            // update user money
            _workContext.CurrentBonusAppCustomer.Money -= actualAmount;
            _customerService.UpdateCustomer(_workContext.CurrentBonusAppCustomer);

            return View();
        }

        public ActionResult Log()
        {
            var withdrawLogs = _customerActivityService.GetAllWithdraws(customerId: _workContext.CurrentBonusAppCustomer.Id);
            var model = withdrawLogs.Select(x => new WithdrawLogModel
            {
                Comment = x.Comment,
                Amount = x.Amount,
                IsDone = x.IsDone,
                CreatedOnUtc = x.CreatedOnUtc,
                CompleteOnUtc = x.CompleteOnUtc,
                IpAddress = x.IpAddress,
            });
            return View(model);
        }

        #endregion
    }
}