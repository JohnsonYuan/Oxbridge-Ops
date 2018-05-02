using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Domain;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.ZhiXiao;
using Nop.Models.Customers;
using Nop.Services.Authentication;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Services.ZhiXiao;
using Nop.Web.Framework.Controllers;
using Web.ZhiXiao.Factories;

namespace Web.ZhiXiao.Controllers
{
    public class CustomerController : BaseAdminController
    {
        #region Fields

        private readonly ICustomerModelFactory _customerModelFactory;
        private readonly IAuthenticationService _authenticationService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly DateTimeSettings _dateTimeSettings;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ICustomerService _customerService;
        private readonly ICustomerTeamService _customerTeamService;
        //private readonly ICustomerAttributeParser _customerAttributeParser;
        private readonly ICustomerAttributeService _customerAttributeService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICustomerRegistrationService _customerRegistrationService;

        private readonly CustomerSettings _customerSettings;
        private readonly IWebHelper _webHelper;

        private readonly ICustomerActivityService _customerActivityService;
        private readonly IStoreService _storeService;

        private readonly ZhiXiaoSettings _zhiXiaoSettings;
        private readonly LocalizationSettings _localizationSettings;
        private readonly StoreInformationSettings _storeInformationSettings;

        private readonly IPermissionService _permissionService;

        #endregion

        #region Ctor

        public CustomerController(
            //IAddressModelFactory addressModelFactory,
            ICustomerModelFactory customerModelFactory,
            IAuthenticationService authenticationService,
            //TaxSettings taxSettings,
            IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService,
            DateTimeSettings dateTimeSettings,
            IWorkContext workContext,
            IStoreContext storeContext,
            ICustomerService customerService,
            ICustomerTeamService customerTeamService,
            //ICustomerAttributeParser customerAttributeParser,
            //ICustomerAttributeService customerAttributeService,
            IGenericAttributeService genericAttributeService,
            ICustomerRegistrationService customerRegistrationService,
            //ITaxService taxService,
            CustomerSettings customerSettings,
            //AddressSettings addressSettings,
            //ForumSettings forumSettings,
            //IAddressService addressService,
            //ICountryService countryService,
            //IOrderService orderService,
            //IPictureService pictureService,
            //INewsLetterSubscriptionService newsLetterSubscriptionService,
            //IShoppingCartService shoppingCartService,
            //IOpenAuthenticationService openAuthenticationService,
            IWebHelper webHelper,
            ICustomerActivityService customerActivityService,
            //IAddressAttributeParser addressAttributeParser,
            //IAddressAttributeService addressAttributeService,
            IStoreService storeService,
            //IEventPublisher eventPublisher,
            //MediaSettings mediaSettings,
            //IWorkflowMessageService workflowMessageService,
            LocalizationSettings localizationSettings,
            //CaptchaSettings captchaSettings,
            ZhiXiaoSettings zhiXiaoSettings,
            StoreInformationSettings storeInformationSettings,
            IPermissionService permissionService)
        {
            //this._addressModelFactory = addressModelFactory;
            this._customerModelFactory = customerModelFactory;
            this._authenticationService = authenticationService;
            this._dateTimeHelper = dateTimeHelper;
            this._localizationService = localizationService;
            this._dateTimeSettings = dateTimeSettings;
            //this._taxSettings = taxSettings;
            this._localizationService = localizationService;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._customerService = customerService;
            this._customerTeamService = customerTeamService;
            //this._customerAttributeParser = customerAttributeParser;
            //this._customerAttributeService = customerAttributeService;
            this._genericAttributeService = genericAttributeService;
            this._customerRegistrationService = customerRegistrationService;
            //this._taxService = taxService;
            this._customerSettings = customerSettings;
            //this._addressSettings = addressSettings;
            //this._forumSettings = forumSettings;
            //this._addressService = addressService;
            //this._countryService = countryService;
            //this._orderService = orderService;
            //this._pictureService = pictureService;
            //this._newsLetterSubscriptionService = newsLetterSubscriptionService;
            //this._shoppingCartService = shoppingCartService;
            //this._openAuthenticationService = openAuthenticationService;
            this._webHelper = webHelper;
            this._customerActivityService = customerActivityService;
            //this._addressAttributeParser = addressAttributeParser;
            //this._addressAttributeService = addressAttributeService;
            this._storeService = storeService;
            //this._eventPublisher = eventPublisher;
            //this._mediaSettings = mediaSettings;
            //this._workflowMessageService = workflowMessageService;
            this._zhiXiaoSettings = zhiXiaoSettings;
            this._localizationSettings = localizationSettings;
            //this._captchaSettings = captchaSettings;
            this._storeInformationSettings = storeInformationSettings;

            this._permissionService = permissionService;
        }

        #endregion

        #region Utilities

        [NonAction]
        protected virtual void PrepareCustomerModel(CustomerModel model)
        {
            var allStores = _storeService.GetAllStores();

            model.UsernamesEnabled = _customerSettings.UsernamesEnabled;
            //model.AllowCustomersToSetTimeZone = _dateTimeSettings.AllowCustomersToSetTimeZone;
            model.AllowCustomersToSetTimeZone = false;
            foreach (var tzi in _dateTimeHelper.GetSystemTimeZones())
                model.AvailableTimeZones.Add(new SelectListItem { Text = tzi.DisplayName, Value = tzi.Id, Selected = (tzi.Id == model.TimeZoneId) });

            model.DisplayVatNumber = false;


            //vendors
            //PrepareVendorsModel(model);
            //customer attributes
            //PrepareCustomerAttributeModel(model, customer);

            model.GenderEnabled = _customerSettings.GenderEnabled;
            model.DateOfBirthEnabled = _customerSettings.DateOfBirthEnabled;
            model.CompanyEnabled = _customerSettings.CompanyEnabled;
            model.StreetAddressEnabled = _customerSettings.StreetAddressEnabled;
            //model.StreetAddress2Enabled = _customerSettings.StreetAddress2Enabled;
            //model.ZipPostalCodeEnabled = _customerSettings.ZipPostalCodeEnabled;
            model.CityEnabled = _customerSettings.CityEnabled;
            model.StateProvinceEnabled = _customerSettings.StateProvinceEnabled;
            model.PhoneEnabled = _customerSettings.PhoneEnabled;
            //model.FaxEnabled = _customerSettings.FaxEnabled;

            //countries and states
            if (_customerSettings.CountryEnabled)
            {
                // delete this
            }

            //newsletter subscriptions
            model.AvailableNewsletterSubscriptionStores = allStores
                .Select(s => new CustomerModel.StoreModel() { Id = s.Id, Name = s.Name })
                .ToList();

            //sending of the welcome message:
            //1. "admin approval" registration method
            //2. already created customer
            //3. registered
            model.AllowSendingOfWelcomeMessage = false;

            //sending of the activation message
            //1. "email validation" registration method
            //2. already created customer
            //3. registered
            //4. not active
            model.AllowReSendingOfActivationMessage = true;
        }

        #endregion

        #region Register

        [ParameterBasedOnQueryString("advanced", "registerAdvanceUser")]
        public virtual ActionResult Register(bool registerAdvanceUser)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            var model = new CustomerModel();
            PrepareCustomerModel(model);
            //default value
            model.Active = true;

            if (registerAdvanceUser)
            {
                ViewBag.Notes = string.Format("注册高级会员, 所需电子币{0}", _zhiXiaoSettings.Register_Money_AdvancedUser);
            }
            else
            {
                ViewBag.Notes = string.Format("注册普通会员, 所需电子币{0}", _zhiXiaoSettings.Register_Money_NormalUser);
            }
            return View(model);
        }

        [HttpPost, ParameterBasedOnQueryString("advanced", "registerAdvanceUser")]
        [FormValueRequired("save", "save-continue")]
        [ValidateInput(false)]
        public virtual ActionResult Register(CustomerModel model, bool registerAdvanceUser)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            if (!String.IsNullOrWhiteSpace(model.Email))
            {
                var cust2 = _customerService.GetCustomerByEmail(model.Email);
                if (cust2 != null)
                    ModelState.AddModelError("", "Email is already registered");
            }

            if (!String.IsNullOrWhiteSpace(model.Username) & _customerSettings.UsernamesEnabled)
            {
                var cust2 = _customerService.GetCustomerByUsername(model.Username);
                if (cust2 != null)
                    ModelState.AddModelError("", "用户名已经使用");
            }

            {
                var cust2 = _customerService.GetCustomerByNickName(model.NickName);
                if (cust2 != null)
                    ModelState.AddModelError("", "昵称已经使用");
            }

            if (!String.IsNullOrWhiteSpace(model.Phone) && _customerSettings.PhoneEnabled)
            {
                var cust2 = _customerService.GetCustomerByPhoneNumber(model.Username);
                if (cust2 != null)
                    ModelState.AddModelError("", "手机号已经使用");
            }

            // 检查推荐人
            // 如果是管理员注册用户, 需要填写推荐人
            // 如果饰普通用户注册用户, 推荐人就是该用户
            bool isManager = _permissionService.Authorize(StandardPermissionProvider.ManageCustomers);

            Customer parentUser;
            if (isManager)
            {
                parentUser = _customerService.GetCustomerById(model.ZhiXiao_ParentId);
                if (parentUser == null)
                    ModelState.AddModelError("", "推荐人不存在, 请重新输入!");
            }
            else
            {
                parentUser = _workContext.CurrentCustomer;
            }

            // 推荐人下线个数不能超过_zhiXiaoSettings.MaxChildCount
            int parentChildCount = parentUser.GetAttribute<int>(SystemCustomerAttributeNames.ZhiXiao_ChildCount);
            if (parentChildCount >= _zhiXiaoSettings.MaxChildCount)
                ModelState.AddModelError("", string.Format("该推荐人已达到{0}个下线， 不能添加!", _zhiXiaoSettings.MaxChildCount));

            // 添加用户所需钱
            int requiredMoney = _zhiXiaoSettings.Register_Money_NormalUser;
            if (registerAdvanceUser)
            {
                requiredMoney = _zhiXiaoSettings.Register_Money_AdvancedUser;
            }

            var currentUserMoney = parentUser.GetAttribute<long>(SystemCustomerAttributeNames.ZhiXiao_MoneyNum);

            if (!isManager)
            {
                if (currentUserMoney < requiredMoney)
                {
                    ModelState.AddModelError("", "电子币不足" + requiredMoney + ", 不能注册会员, 请充值!");
                }
            }
            
            //custom customer attributes
            //var customerAttributesXml = ParseCustomCustomerAttributes(form);
            //if (newCustomerRoles.Any() && newCustomerRoles.FirstOrDefault(c => c.SystemName == SystemCustomerRoleNames.Registered) != null)
            //{
            //    var customerAttributeWarnings = _customerAttributeParser.GetAttributeWarnings(customerAttributesXml);
            //    foreach (var error in customerAttributeWarnings)
            //    {
            //        ModelState.AddModelError("", error);
            //    }
            //}

            if (ModelState.IsValid)
            {
                // 1. 新增用户
                var customer = new Customer
                {
                    CustomerGuid = Guid.NewGuid(),
                    // 邮箱默认后缀为@yourStore.com
                    Email = CommonHelper.IsValidEmail(model.Email) ? model.Email : string.Format("{0}@yourStore.com", model.Username),
                    Username = model.Username,
                    //VendorId = model.VendorId,
                    AdminComment = model.AdminComment,
                    IsTaxExempt = model.IsTaxExempt,
                    Active = model.Active,
                    CreatedOnUtc = DateTime.UtcNow,
                    LastActivityDateUtc = DateTime.UtcNow,
                    RegisteredInStoreId = _storeContext.CurrentStore.Id
                };

                _customerService.InsertCustomer(customer);

                if (!isManager)
                {
                    // 扣钱
                    if (currentUserMoney < requiredMoney)
                        throw new Exception("所需电子币不足");
                    _genericAttributeService.SaveAttribute(parentUser, SystemCustomerAttributeNames.ZhiXiao_MoneyNum, currentUserMoney - requiredMoney);
                    _customerActivityService.InsertActivity(parentUser, SystemZhiXiaoLogTypes.AddNewUser,
                        "注册会员{0}, 电子币-{1}",
                        customer.GetNickName(),
                        requiredMoney);
                }

                var zhiXiaoRoles = _customerService.GetAllCustomerRoles(true)
                                     .Where(cr => cr.SystemName == SystemCustomerRoleNames.Registered
                                     || cr.SystemName == SystemCustomerRoleNames.Registered_Advanced);

                var newCustomerRoles = new List<CustomerRole>();

                // user roles
                newCustomerRoles.Add(zhiXiaoRoles.Where(cr => cr.SystemName == SystemCustomerRoleNames.Registered).First());
                if (registerAdvanceUser)
                    newCustomerRoles.Add(zhiXiaoRoles.Where(cr => cr.SystemName == SystemCustomerRoleNames.Registered_Advanced).First());

                //form fields
                if (_dateTimeSettings.AllowCustomersToSetTimeZone)
                    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.TimeZoneId, model.TimeZoneId);
                if (_customerSettings.GenderEnabled)
                    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Gender, model.Gender);
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.FirstName, model.FirstName);
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.LastName, model.LastName);
                if (_customerSettings.DateOfBirthEnabled)
                    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.DateOfBirth, model.DateOfBirth);
                if (_customerSettings.CompanyEnabled)
                    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Company, model.Company);
                if (_customerSettings.StreetAddressEnabled)
                    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.StreetAddress, model.StreetAddress);
                //if (_customerSettings.StreetAddress2Enabled)
                //    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.StreetAddress2, model.StreetAddress2);
                //if (_customerSettings.ZipPostalCodeEnabled)
                //    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZipPostalCode, model.ZipPostalCode);
                if (_customerSettings.CityEnabled)
                {
                    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.City, model.City);
                    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.District, model.District);
                }
                //if (_customerSettings.CountryEnabled)
                //    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.CountryId, model.CountryId);
                if (_customerSettings.CountryEnabled && _customerSettings.StateProvinceEnabled)
                    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.StateProvince, model.StateProvince);
                if (_customerSettings.PhoneEnabled)
                    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Phone, model.Phone);
                //if (_customerSettings.FaxEnabled)
                //    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Fax, model.Fax);

                //custom customer attributes
                //_genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.CustomCustomerAttributes, customerAttributesXml);

                // 直销用户个人信息
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZhiXiao_NickName, model.NickName);
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZhiXiao_Password2, model.Password2);          // 二级密码
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZhiXiao_IdCardNum, model.ZhiXiao_IdCardNum);
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZhiXiao_YinHang, model.ZhiXiao_YinHang);      // 银行
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZhiXiao_KaiHuHang, model.ZhiXiao_KaiHuHang);  // 开户行
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZhiXiao_KaiHuMing, model.ZhiXiao_KaiHuMing);  // 开户名
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZhiXiao_BandNum, model.ZhiXiao_BandNum);      // 银行卡号

                //password
                if (!String.IsNullOrWhiteSpace(model.Password))
                {
                    var changePassRequest = new ChangePasswordRequest(model.Email, false, _customerSettings.DefaultPasswordFormat, model.Password);
                    var changePassResult = _customerRegistrationService.ChangePassword(changePassRequest);
                    if (!changePassResult.Success)
                    {
                        foreach (var changePassError in changePassResult.Errors)
                            ErrorNotification(changePassError);
                    }
                }

                //customer roles
                foreach (var customerRole in newCustomerRoles)
                {
                    //ensure that the current customer cannot add to "Administrators" system role if he's not an admin himself
                    if (customerRole.SystemName == SystemCustomerRoleNames.Administrators &&
                        !_workContext.CurrentCustomer.IsAdmin())
                        continue;

                    customer.CustomerRoles.Add(customerRole);
                }
                _customerService.UpdateCustomer(customer);

                // 直销用户分组信息
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZhiXiao_ParentId, parentUser.Id);

                // var teamId = parentUser.GetAttribute<int>(SystemCustomerAttributeNames.ZhiXiao_TeamId);
                var parentTeam = parentUser.CustomerTeam;

                customer.CustomerTeam = parentTeam;
                // _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZhiXiao_TeamId, teamId);

                var sortId = _customerTeamService.GetNewUserSortId(parentTeam);
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZhiXiao_InTeamOrder, sortId);
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZhiXiao_InTeamTime, DateTime.UtcNow);
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZhiXiao_LevelId, (int)CustomerLevel.ZuYuan);

                // 2. Update Parent Info
                _genericAttributeService.SaveAttribute(parentUser, SystemCustomerAttributeNames.ZhiXiao_ChildCount, parentChildCount + 1);

                // 3. 更新小组人数
                parentTeam.UserCount += 1;
                _customerTeamService.UpdateCustomerTeam(parentTeam);

                // 1. 给组长， 副组长分钱, 2. 如果人数满足, 重新分组
                _customerTeamService.AddNewUserToTeam(parentTeam, customer);

                //activity log
                _customerActivityService.InsertActivity("AddNewCustomer", _localizationService.GetResource("ActivityLog.AddNewCustomer"), customer.Id);

                SuccessNotification(_localizationService.GetResource("Admin.Customers.Customers.Added"));

                //if (continueEditing)
                //{
                //    //selected tab
                //    SaveSelectedTabName();

                //    return RedirectToAction("Edit", new { id = customer.Id });
                //}
                return RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            PrepareCustomerModel(model);

            if (registerAdvanceUser)
            {
                ViewBag.Notes = string.Format("注册高级会员, 所需电子币{0}", _zhiXiaoSettings.Register_Money_AdvancedUser);
            }
            else
            {
                ViewBag.Notes = string.Format("注册普通会员, 所需电子币{0}", _zhiXiaoSettings.Register_Money_NormalUser);
            }
            return View(model);
        }

        #endregion
    }
}