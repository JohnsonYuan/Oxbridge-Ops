using System;
using System.Linq;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Domain.BonusApp;
using Nop.Core.Domain.BonusApp.Customers;
using Nop.Core.Domain.Customers;
using Nop.Extensions;
using Nop.Services.BonusApp.Customers;
using Nop.Services.BonusApp.Logging;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Web.ZhiXiao.Areas.YiJiaYi_Manage.Models.BonusApp.Customer;
using Web.ZhiXiao.Areas.YiJiaYi_Manage.Models.BonusApp.Log;

namespace Web.ZhiXiao.Areas.YiJiaYi_Manage.Controllers.BonusApp
{
    public class BonusApp_CustomerController : BaseAdminController
    {

        #region Fields

        private readonly IBonusApp_CustomerService _customerService;
        private readonly IBonusApp_CustomerActivityService _customerActivityService;
        private readonly IGeoLookupService _geoLookupService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly CustomerSettings _customerSettings;
        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;
        private readonly IPictureService _pictureService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Constructors

        public BonusApp_CustomerController(
            IBonusApp_CustomerService customerService,
            IBonusApp_CustomerActivityService customerActivityService,
            IGeoLookupService geoLookupService,
            IDateTimeHelper dateTimeHelper,
            CustomerSettings customerSettings,
            IPermissionService permissionService,
            ILocalizationService localizationService,
            IPictureService pictureService,
            IWorkContext workContext)
        {
            this._customerService = customerService;
            this._customerActivityService = customerActivityService;
            this._geoLookupService = geoLookupService;
            this._dateTimeHelper = dateTimeHelper;
            this._customerSettings = customerSettings;
            this._permissionService = permissionService;
            this._localizationService = localizationService;
            this._pictureService = pictureService;
            this._workContext = workContext;
        }

        #endregion

        #region Utilities

        [NonAction]
        protected virtual BonusAppCustomerModel PrepareCustomerModel(BonusApp_Customer customer)
        {
            return new BonusAppCustomerModel
            {
                Id = customer.Id,
                Username = customer.Username,
                NickName = customer.Nickname,
                AvatarFilePath = _pictureService.GetPictureUrl(customer.AvatarFileName),
                Money = customer.Money,
                Phone = customer.PhoneNumber,
                Active = customer.Active,
                CreatedOn = _dateTimeHelper.ConvertToUserTime(customer.CreatedOnUtc, DateTimeKind.Utc),
                LastActivityDate = _dateTimeHelper.ConvertToUserTime(customer.LastActivityDateUtc, DateTimeKind.Utc),
            };
        }

        private void PrepareCustomerModel(BonusAppCustomerModel customerModel, BonusApp_Customer customer)
        {
            customerModel.Id = customer.Id;
            customerModel.Username = customer.Username;
            customerModel.NickName = customer.Nickname;
            customerModel.AvatarFilePath = _pictureService.GetPictureUrl(customer.AvatarFileName);
            customerModel.Money = customer.Money;
            customerModel.Phone = customer.PhoneNumber;
            customerModel.Active = customer.Active;
            customerModel.CreatedOn = _dateTimeHelper.ConvertToUserTime(customer.CreatedOnUtc, DateTimeKind.Utc);
            customerModel.LastActivityDate = _dateTimeHelper.ConvertToUserTime(customer.LastActivityDateUtc, DateTimeKind.Utc);
        }

        #endregion

        #region Methods

        /// <summary>
        /// 客户
        /// </summary>
        /// <returns></returns>
        public ActionResult CustomerList()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            // load all customer roles
            //var defaultLevelids = Enum.GetValues(typeof(CustomerLevel)).OfType<CustomerLevel>().Select(x => (int)x).ToList();
            var model = new BonusAppCustomerListModel
            {
                PhoneEnabled = _customerSettings.PhoneEnabled,
            };

            return View(model);
        }

        [HttpPost]
        public virtual ActionResult CustomerList(DataSourceRequest command, BonusAppCustomerListModel model)
        {
            //we use own own binder for searchCustomerRoleIds property 
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedKendoGridJson();

            var customers = _customerService.GetAllCustomers(
                username: model.SearchUsername,
                nickname: model.SearchNickname,
                phone: model.SearchPhone,
                ipAddress: model.SearchIpAddress,
                minMoney: model.SearchMoney,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = customers.Select(PrepareCustomerModel),
                Total = customers.TotalCount
            };

            return Json(gridModel);
        }

        /// <summary>
        /// 在线客户
        /// </summary>
        /// <returns></returns>
        public ActionResult OnlineCustomers()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            return View();
        }

        [HttpPost]
        public ActionResult OnlineCustomers(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedKendoGridJson();

            var customers = _customerService.GetOnlineCustomers(DateTime.UtcNow.AddMinutes(-_customerSettings.OnlineCustomerMinutes),
                command.Page - 1, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = customers.Select(x => new Nop.Models.Customers.OnlineCustomerModel
                {
                    Id = x.Id,
                    CustomerInfo = x.Username,
                    LastIpAddress = x.LastIpAddress,
                    Location = _geoLookupService.LookupCountryName(x.LastIpAddress),
                    LastActivityDate = _dateTimeHelper.ConvertToUserTime(x.LastActivityDateUtc, DateTimeKind.Utc),
                    //LastVisitedPage = _customerSettings.StoreLastVisitedPage ?
                    //    x.GetAttribute<string>(SystemCustomerAttributeNames.LastVisitedPage) :
                    //    _localizationService.GetResource("Admin.Customers.OnlineCustomers.Fields.LastVisitedPage.Disabled")
                }),
                Total = customers.TotalCount
            };

            return Json(gridModel);
        }

        public virtual ActionResult CustomerEdit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(id);
            if (customer == null || customer.Deleted)
                //No customer found with the specified id
                return RedirectToAction("CustomerList");

            var model = new BonusAppCustomerModel();
            PrepareCustomerModel(model, customer);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        [ValidateInput(false)]
        public virtual ActionResult CustomerEdit(BonusAppCustomerModel model, bool continueEditing/*, FormCollection form*/)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(model.Id);
            if (customer == null || customer.Deleted)
                //No customer found with the specified id
                return RedirectToAction("CustomerList");

            if (ModelState.IsValid)
            {
                try
                {
                    //customer.AdminComment = model.AdminComment;
                    //customer.IsTaxExempt = model.IsTaxExempt;

                    customer.Active = model.Active;

                    //username

                    if (!String.IsNullOrWhiteSpace(model.Username))
                    {
                        customer.Username = model.Username;
                    }

                    // phone number
                    customer.PhoneNumber = model.Phone;

                    // nick name
                    if (!String.IsNullOrWhiteSpace(model.NickName))
                    {
                        customer.Nickname = model.NickName;
                    }

                    //VAT number
                    //if (_taxSettings.EuVatEnabled)
                    //{
                    //    var prevVatNumber = customer.GetAttribute<string>(SystemCustomerAttributeNames.VatNumber);

                    //    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.VatNumber, model.VatNumber);
                    //    //set VAT number status
                    //    if (!String.IsNullOrEmpty(model.VatNumber))
                    //    {
                    //        if (!model.VatNumber.Equals(prevVatNumber, StringComparison.InvariantCultureIgnoreCase))
                    //        {
                    //            _genericAttributeService.SaveAttribute(customer,
                    //                SystemCustomerAttributeNames.VatNumberStatusId,
                    //                (int)_taxService.GetVatNumberStatus(model.VatNumber));
                    //        }
                    //    }
                    //    else
                    //    {
                    //        _genericAttributeService.SaveAttribute(customer,
                    //            SystemCustomerAttributeNames.VatNumberStatusId,
                    //            (int)VatNumberStatus.Empty);
                    //    }
                    //}

                    //vendor
                    //customer.VendorId = model.VendorId;

                    //form fields
                    //_registerZhiXiaoUserHelper.SaveCustomerAttriubteValues(customer, model);

                    //newsletter subscriptions omit ...

                    _customerService.UpdateCustomer(customer);

                    //activity log
                    _customerActivityService.InsertActivity("EditCustomer", _localizationService.GetResource("ActivityLog.EditCustomer"), customer.Id);

                    SuccessNotification(_localizationService.GetResource("Admin.Customers.Customers.Updated"));
                    if (continueEditing)
                    {
                        //selected tab
                        SaveSelectedTabName();

                        return RedirectToAction("CustomerEdit", new { id = customer.Id });
                    }
                    return RedirectToAction("CustomerList");
                }
                catch (Exception exc)
                {
                    ErrorNotification(exc.Message, false);
                }
            }

            //If we got this far, something failed, redisplay form
            PrepareCustomerModel(model, customer);
            return View(model);
        }

        [HttpPost, ActionName("CustomerEdit")]
        [FormValueRequired("changepassword")]
        public virtual ActionResult ChangePassword(BonusAppCustomerModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(model.Id);
            if (customer == null)
                //No customer found with the specified id
                return RedirectToAction("CustomerList");

            //ensure that the current customer cannot change passwords of "Administrators" if he's not an admin himself
            //if (customer.IsAdmin() && !_workContext.CurrentCustomer.IsAdmin())
            //{
            //    ErrorNotification(_localizationService.GetResource("Admin.Customers.Customers.OnlyAdminCanChangePassword"));
            //    return RedirectToAction("Edit", new { id = customer.Id });
            //}

            if (string.IsNullOrEmpty(model.Password))
            {
                ErrorNotification("请输入新密码");
                return RedirectToAction("CustomerEdit", new { id = customer.Id });
            }

            if (ModelState.IsValid)
            {
                _customerActivityService.InsertActivity(customer, BonusAppConstants.LogType_User_ChangePwd, "管理员修改密码, 新密码: {0}", model.Password);
                _customerService.ChangeCustomerPassword(customer, model.Password);
                SuccessNotification(_localizationService.GetResource("Admin.Customers.Customers.PasswordChanged"));
            }

            return RedirectToAction("CustomerEdit", new { id = customer.Id });
        }

        /// <summary>
        /// 提现申请
        /// </summary>
        public virtual ActionResult Withdraw()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedKendoGridJson();

            var model = new BonusAppWithdrawLogSearchModel();
            return View(model);
        }

        /// <summary>
        /// 提现申请
        /// </summary>
        [HttpPost, ActionName("Withdraw")]
        public virtual ActionResult WithdrawList(DataSourceRequest command,
            BonusAppWithdrawLogSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedKendoGridJson();

            DateTime? startDateValue = (searchModel.CreatedOnFrom == null) ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.CreatedOnFrom.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (searchModel.CreatedOnTo == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.CreatedOnTo.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            var withdraws = _customerActivityService.GetAllWithdraws(startDateValue, endDateValue, null, searchModel.IsDone, command.Page - 1, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = withdraws.Select(x =>
                {
                    var m = x.ToModel();
                    m.Comment = x.Comment.Replace("<br/>", " ");    // 替换html
                    m.Username = x.Customer.Username;
                    m.Nickname = x.Customer.Nickname;
                    m.CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc);
                    if (x.CompleteOnUtc.HasValue)
                        m.CompleteOn = _dateTimeHelper.ConvertToUserTime(x.CompleteOnUtc.Value, DateTimeKind.Utc);
                    return m;
                }),
                Total = withdraws.TotalCount
            };

            return Json(gridModel);
        }

        /// <summary>
        /// Process withdraw
        /// </summary>
        /// <param name="id">Withdraw id</param> 
        public virtual ActionResult ProcessWithdrawPopup(int id, string btnId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedKendoGridJson();

            var withDraw = _customerActivityService.GetWithdrawById(id);

            if (withDraw == null)
                return RedirectToAction("List");

            ViewBag.btnId = btnId;
            var model = withDraw.ToModel();
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(withDraw.CreatedOnUtc, DateTimeKind.Utc);
            model.CompleteOn = withDraw.CompleteOnUtc.HasValue ? _dateTimeHelper.ConvertToUserTime(withDraw.CompleteOnUtc.Value, DateTimeKind.Utc) : (DateTime?)null;
            //model.CustomerModel = PrepareCustomerModelForZhiXiaoInfo(withDraw.Customer);
            //var customer = _customerService.GetCustomerById(model.CustomerId);
            PrepareCustomerModel(model.CustomerModel, withDraw.Customer);
            return View(model);
        }

        /// <summary>
        /// Process withdraw
        /// </summary>
        /// <param name="id">Withdraw id</param> 
        [HttpPost]
        public virtual ActionResult ProcessWithdrawPopup(int id, string btnId, bool isDone)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedKendoGridJson();

            var withDraw = _customerActivityService.GetWithdrawById(id);

            if (withDraw == null)
                return RedirectToAction("List");

            if (isDone)
            {
                ViewBag.RefreshPage = true;
                withDraw.IsDone = true;
                withDraw.CompleteOnUtc = DateTime.UtcNow;
                _customerActivityService.UpdateWithdrawLog(withDraw);

                var customer = withDraw.Customer;
                _customerActivityService.InsertActivity(BonusAppConstants.LogType_Admin_ProcessTiXian,
                    "管理员通过用户{0}({1})的{2}元提现申请",
                    customer.Username,
                    customer.Nickname,
                    withDraw.Amount);

                // 提示用户提现成功
                customer.NotificationMoneyLogId = withDraw.Id;
                customer.NotificationMoney = (int)withDraw.Amount;
                _customerService.UpdateCustomer(customer);

                //_customerActivityService.InsertMoneyLog(withDraw.Customer, SystemZhiXiaoLogTypes.ProcessWithdraw,
                //    0,
                //    "管理员通过兑换{0}积分申请",
                //    withDraw.Amount);
            }

            ViewBag.btnId = btnId;
            ViewBag.RefreshPage = true;

            var model = withDraw.ToModel();
            return View(model);
        }

        #region User logs

        /// <summary>
        /// 用户金钱变化log
        /// SystemZhiXiaoLogTypes
        /// </summary>
        [HttpPost]
        public virtual ActionResult MoneyLog(DataSourceRequest command, int customerId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedKendoGridJson();

            var moneyLogs = _customerActivityService.GetAllMoneyLogs(
                customerId: customerId,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = moneyLogs.Select((x, index) =>
                {
                    var m = x.ToModel<Web.ZhiXiao.Areas.BonusApp.Models.Log.MoneyLogModel>();
                    m.OrderNum = _customerActivityService.GetMoneyLogOrderNumber(x);
                    m.OrderNum = index + 1; // 排序
                    m.CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc);
                    if (x.CompleteOnUtc.HasValue)
                        m.CompleteOn = _dateTimeHelper.ConvertToUserTime(x.CompleteOnUtc.Value, DateTimeKind.Utc);

                    return m;
                }),
                Total = moneyLogs.TotalCount
            };

            return Json(gridModel);
        }

        /// <summary>
        /// 用户金钱变化log
        /// SystemZhiXiaoLogTypes
        /// </summary>
        [HttpPost]
        public virtual ActionResult WithdrawLog(DataSourceRequest command, int customerId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedKendoGridJson();

            var moneyLogs = _customerActivityService.GetAllWithdraws(
                customerId: customerId,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize);

            var withdrawLogs = _customerActivityService.GetAllWithdraws(customerId: customerId);

            var gridModel = new DataSourceResult
            {
                Data = withdrawLogs.Select(x => new Web.ZhiXiao.Areas.BonusApp.Models.Log.WithdrawLogModel
                {
                    Comment = x.Comment,
                    Amount = x.Amount,
                    IsDone = x.IsDone,
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc),
                    CompleteOn = x.CompleteOnUtc.HasValue ? _dateTimeHelper.ConvertToUserTime(x.CompleteOnUtc.Value, DateTimeKind.Utc) : (DateTime?)null,
                    IpAddress = x.IpAddress,
                }),
                Total = moneyLogs.TotalCount
            };

            return Json(gridModel);
        }

        /// <summary>
        /// 用户所有log
        /// </summary>
        /// <param name="command"></param>
        /// <param name="customerId"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual ActionResult ActivityLog(DataSourceRequest command, int customerId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedKendoGridJson();

            var activityLog = _customerActivityService.GetAllActivities(null, null, customerId, 0, command.Page - 1, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = activityLog.Select(x =>
                {
                    var m = new Nop.Admin.Models.Logging.ActivityLogModel
                    {
                        Id = x.Id,
                        ActivityLogTypeName = x.ActivityLogType.Name,
                        Comment = x.Comment,
                        CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc),
                        IpAddress = x.IpAddress
                    };
                    return m;

                }),
                Total = activityLog.TotalCount
            };

            return Json(gridModel);
        }

        #endregion

        #region 给用户充值金额

        /// <summary>
        /// 充值金额
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public virtual ActionResult Recharge(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedKendoGridJson();

            var customer = _customerService.GetCustomerById(id);
            if (customer == null)
                return RedirectToAction("CustomerList");

            ViewBag.NickName = customer.Nickname;
            ViewBag.UserName = customer.Username;
            ViewBag.MoneyNum = customer.Money;

            return View();
        }

        [HttpPost]
        /// <summary>
        /// 充值金额
        /// </summary>
        public virtual ActionResult Recharge(int id, decimal money, string btnId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedKendoGridJson();

            var customer = _customerService.GetCustomerById(id);
            if (customer == null)
                return RedirectToAction("CustomerList");

            if (money == 0)
                ModelState.AddModelError("", "充值金额不能为0");

            if (ModelState.IsValid)
            {
                try
                {
                    var moneyAfter = customer.Money + money;
                    string moneyChangeStr = string.Format("原金额{0}, 充值后{1}", customer.Money, moneyAfter);

                    customer.Money = moneyAfter;
                    _customerService.UpdateCustomer(customer);

                    // log for customer
                    _customerActivityService.InsertActivity(customer, BonusAppConstants.LogType_Admin_Recharge,
                        "管理员充值金额{0}, {1}",
                        money,
                        moneyChangeStr);

                    /************************************************/
                    /**********放入奖金池(最主要判断逻辑)*********/
                    /************************************************/
                    _customerActivityService.InsertMoneyLog(customer, money,
                        "充值 {0}, {1}", money, moneyChangeStr);

                    SuccessNotification(string.Format("用户{0}充值{1}成功", customer.Username, money));

                    ViewBag.btnId = btnId;
                    ViewBag.RefreshPage = true;
                }
                catch (Exception ex)
                {
                    ErrorNotification("充值失败, " + ex.Message);
                }
            }

            ViewBag.NickName = customer.Nickname;
            ViewBag.UserName = customer.Username;
            ViewBag.MoneyNum = customer.Money;
            return View();
        }

        #endregion

        #endregion
    }
}