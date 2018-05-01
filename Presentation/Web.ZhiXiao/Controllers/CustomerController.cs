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
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc;
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
        protected virtual string GetCustomerRolesNames(IList<CustomerRole> customerRoles, string separator = ",")
        {
            var sb = new StringBuilder();
            for (int i = 0; i < customerRoles.Count; i++)
            {
                sb.Append(customerRoles[i].Name);
                if (i != customerRoles.Count - 1)
                {
                    sb.Append(separator);
                    sb.Append(" ");
                }
            }
            return sb.ToString();
        }

        [NonAction]
        protected virtual CustomerModel PrepareCustomerModelForList(Customer customer)
        {
            return new CustomerModel
            {
                Id = customer.Id,
                Email = customer.IsRegistered() ? customer.Email : _localizationService.GetResource("Admin.Customers.Guest"),
                Username = customer.Username,
                FullName = customer.GetFullName(),
                Company = customer.GetAttribute<string>(SystemCustomerAttributeNames.Company),
                Phone = customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone),
                //ZipPostalCode = customer.GetAttribute<string>(SystemCustomerAttributeNames.ZipPostalCode),
                CustomerRoleNames = GetCustomerRolesNames(customer.CustomerRoles.ToList()),
                Active = customer.Active,
                CreatedOn = _dateTimeHelper.ConvertToUserTime(customer.CreatedOnUtc, DateTimeKind.Utc),
                LastActivityDate = _dateTimeHelper.ConvertToUserTime(customer.LastActivityDateUtc, DateTimeKind.Utc),
            };
        }

        [NonAction]
        protected virtual void PrepareCustomerModel(CustomerModel model, Customer customer, bool excludeProperties)
        {
            var allStores = _storeService.GetAllStores();
            if (customer != null)
            {
                model.Id = customer.Id;
                if (!excludeProperties)
                {
                    if (model.ZhiXiao_ParentId > 0)
                    {
                        model.ParentUser = _customerService.GetCustomerById(model.ZhiXiao_ParentId);
                    }

                    model.Email = customer.Email;
                    model.Username = customer.Username;
                    //model.VendorId = customer.VendorId;
                    model.AdminComment = customer.AdminComment;
                    model.IsTaxExempt = customer.IsTaxExempt;
                    model.Active = customer.Active;

                    if (customer.RegisteredInStoreId == 0 || allStores.All(s => s.Id != customer.RegisteredInStoreId))
                        model.RegisteredInStore = string.Empty;
                    else
                        model.RegisteredInStore = allStores.First(s => s.Id == customer.RegisteredInStoreId).Name;

                    //var affiliate = _affiliateService.GetAffiliateById(customer.AffiliateId);
                    //if (affiliate != null)
                    //{
                    //    model.AffiliateId = affiliate.Id;
                    //    model.AffiliateName = affiliate.GetFullName();
                    //}

                    model.TimeZoneId = customer.GetAttribute<string>(SystemCustomerAttributeNames.TimeZoneId);
                    model.VatNumber = customer.GetAttribute<string>(SystemCustomerAttributeNames.VatNumber);
                    //model.VatNumberStatusNote = ((VatNumberStatus)customer.GetAttribute<int>(SystemCustomerAttributeNames.VatNumberStatusId))
                    //    .GetLocalizedEnum(_localizationService, _workContext);
                    model.CreatedOn = _dateTimeHelper.ConvertToUserTime(customer.CreatedOnUtc, DateTimeKind.Utc);
                    model.LastActivityDate = _dateTimeHelper.ConvertToUserTime(customer.LastActivityDateUtc, DateTimeKind.Utc);
                    model.LastIpAddress = customer.LastIpAddress;
                    model.LastVisitedPage = customer.GetAttribute<string>(SystemCustomerAttributeNames.LastVisitedPage);

                    model.SelectedCustomerRoleIds = customer.CustomerRoles.Select(cr => cr.Id).ToList();

                    //newsletter subscriptions
                    if (!String.IsNullOrEmpty(customer.Email))
                    {
                        var newsletterSubscriptionStoreIds = new List<int>();
                        //foreach (var store in allStores)
                        //{
                        //    var newsletterSubscription = _newsLetterSubscriptionService
                        //        .GetNewsLetterSubscriptionByEmailAndStoreId(customer.Email, store.Id);
                        //    if (newsletterSubscription != null)
                        //        newsletterSubscriptionStoreIds.Add(store.Id);
                        //    model.SelectedNewsletterSubscriptionStoreIds = newsletterSubscriptionStoreIds.ToArray();
                        //}
                    }

                    //form fields
                    model.FirstName = customer.GetAttribute<string>(SystemCustomerAttributeNames.FirstName);
                    model.LastName = customer.GetAttribute<string>(SystemCustomerAttributeNames.LastName);
                    model.Gender = customer.GetAttribute<string>(SystemCustomerAttributeNames.Gender);
                    model.DateOfBirth = customer.GetAttribute<DateTime?>(SystemCustomerAttributeNames.DateOfBirth);
                    model.Company = customer.GetAttribute<string>(SystemCustomerAttributeNames.Company);
                    model.StreetAddress = customer.GetAttribute<string>(SystemCustomerAttributeNames.StreetAddress);
                    //model.StreetAddress2 = customer.GetAttribute<string>(SystemCustomerAttributeNames.StreetAddress2);
                    //model.ZipPostalCode = customer.GetAttribute<string>(SystemCustomerAttributeNames.ZipPostalCode);
                    model.District = customer.GetAttribute<string>(SystemCustomerAttributeNames.District);
                    model.City = customer.GetAttribute<string>(SystemCustomerAttributeNames.City);
                    model.StateProvince = customer.GetAttribute<string>(SystemCustomerAttributeNames.StateProvince);
                    model.Phone = customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone);
                    //model.Fax = customer.GetAttribute<string>(SystemCustomerAttributeNames.Fax);

                    model.NickName = customer.GetAttribute<string>(SystemCustomerAttributeNames.ZhiXiao_NickName);
                    model.ZhiXiao_IdCardNum = customer.GetAttribute<string>(SystemCustomerAttributeNames.ZhiXiao_IdCardNum);
                    model.ZhiXiao_YinHang = customer.GetAttribute<string>(SystemCustomerAttributeNames.ZhiXiao_YinHang);        // 银行
                    model.ZhiXiao_KaiHuHang = customer.GetAttribute<string>(SystemCustomerAttributeNames.ZhiXiao_KaiHuHang);      // 开户行
                    model.ZhiXiao_KaiHuMing = customer.GetAttribute<string>(SystemCustomerAttributeNames.ZhiXiao_KaiHuMing);      // 开户名
                    model.ZhiXiao_BandNum = customer.GetAttribute<string>(SystemCustomerAttributeNames.ZhiXiao_BandNum);        // 银行卡号
                }
            }

            model.UsernamesEnabled = _customerSettings.UsernamesEnabled;
            //model.AllowCustomersToSetTimeZone = _dateTimeSettings.AllowCustomersToSetTimeZone;
            model.AllowCustomersToSetTimeZone = false;
            foreach (var tzi in _dateTimeHelper.GetSystemTimeZones())
                model.AvailableTimeZones.Add(new SelectListItem { Text = tzi.DisplayName, Value = tzi.Id, Selected = (tzi.Id == model.TimeZoneId) });
            if (customer != null)
            {
                //model.DisplayVatNumber = _taxSettings.EuVatEnabled;
            }
            else
            {
                model.DisplayVatNumber = false;
            }

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

            //customer roles
            var allRoles = _customerService.GetAllCustomerRoles(true);
            var registeredRole = allRoles.FirstOrDefault(c => c.SystemName == SystemCustomerRoleNames.Registered);
            //precheck Registered Role as a default role while creating a new customer through admin
            if (customer == null && registeredRole != null)
            {
                model.SelectedCustomerRoleIds.Add(registeredRole.Id);

                var availableParents = _customerService.GetAllCustomers(customerRoleIds: new int[] { registeredRole.Id });

                foreach (var item in availableParents)
                {
                    //var nickName = item.GetAttribute<string>(SystemCustomerAttributeNames.ZhiXiao_NickName);
                    model.AvailableParents.Add(new SelectListItem()
                    {
                        Text = item.GetNickNameAndUserName(),
                        Value = item.Id.ToString()
                    });
                }

                var zhiXiaoRoles = _customerService.GetAllCustomerRoles(true)
                                     .Where(cr => cr.SystemName == SystemCustomerRoleNames.Registered
                                     || cr.SystemName == SystemCustomerRoleNames.Registered_Advanced);

                foreach (var role in zhiXiaoRoles)
                {
                    model.AvailableCustomerRoles.Add(new SelectListItem
                    {
                        Text = role.Name,
                        Value = role.Id.ToString(),
                        Selected = model.SelectedCustomerRoleIds.Contains(role.Id)
                    });
                }
            }

            //sending of the welcome message:
            //1. "admin approval" registration method
            //2. already created customer
            //3. registered
            model.AllowSendingOfWelcomeMessage = _customerSettings.UserRegistrationType == UserRegistrationType.AdminApproval &&
                customer != null &&
                customer.IsRegistered();
            //sending of the activation message
            //1. "email validation" registration method
            //2. already created customer
            //3. registered
            //4. not active
            model.AllowReSendingOfActivationMessage = _customerSettings.UserRegistrationType == UserRegistrationType.EmailValidation &&
                customer != null &&
                customer.IsRegistered() &&
                !customer.Active;


        }

        [NonAction]
        protected virtual string ValidateCustomerRoles(IList<CustomerRole> customerRoles)
        {
            if (customerRoles == null)
                throw new ArgumentNullException("customerRoles");

            //ensure a customer is not added to both 'Guests' and 'Registered' customer roles
            //ensure that a customer is in at least one required role ('Guests' and 'Registered')
            //bool isInGuestsRole = customerRoles.FirstOrDefault(cr => cr.SystemName == SystemCustomerRoleNames.Guests) != null;
            bool isInRegisteredRole = customerRoles.FirstOrDefault(cr => cr.SystemName == SystemCustomerRoleNames.Registered) != null;
            bool isInRegistered_AdvancedRole = customerRoles.FirstOrDefault(cr => cr.SystemName == SystemCustomerRoleNames.Registered_Advanced) != null;
            if (!isInRegistered_AdvancedRole && !isInRegisteredRole)
                return _localizationService.GetResource("Admin.Customers.Customers.AddCustomerToGuestsOrRegisteredRoleError");

            //no errors
            return "";
        }

        [NonAction]
        private bool SecondAdminAccountExists(Customer customer)
        {
            var customers = _customerService.GetAllCustomers(customerRoleIds: new[] { _customerService.GetCustomerRoleBySystemName(SystemCustomerRoleNames.Administrators).Id });

            return customers.Any(c => c.Active && c.Id != customer.Id);
        }

        #endregion

        #region Register

        public virtual ActionResult Register()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            var model = new CustomerModel();
            PrepareCustomerModel(model, null, false);
            //default value
            model.Active = true;
            model.CanManageCustomers = _permissionService.Authorize(StandardPermissionProvider.ManageCustomers);
            return View("RegisterZhiXiaoUser", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        [ValidateInput(false)]
        public virtual ActionResult Register(CustomerModel model, bool continueEditing, bool? advanced/*, FormCollection form*/)
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
            if (advanced.HasValue && advanced.Value)
            {
                requiredMoney = _zhiXiaoSettings.Register_Money_AdvancedUser;
            }

            var currentUserMoney = parentUser.GetAttribute<long>(SystemCustomerAttributeNames.ZhiXiao_MoneyNum);

            if (!isManager)
            {
                if (currentUserMoney < requiredMoney)
                {
                    ModelState.AddModelError("", "电子币不足26800, 不能注册会员, 请充值!");
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
                    Email = !String.IsNullOrEmpty(model.Email) ? model.Email : string.Format("{0}@yourStore.com", model.Username),
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
                if (advanced.HasValue && advanced.Value)
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

                // 直销相关 分组的属性

                // 直销用户个人信息
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZhiXiao_NickName, model.NickName);
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZhiXiao_Password2, model.Password2);          // 二级密码
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZhiXiao_IdCardNum, model.ZhiXiao_IdCardNum);
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZhiXiao_YinHang, model.ZhiXiao_YinHang);      // 银行
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZhiXiao_KaiHuHang, model.ZhiXiao_KaiHuHang);  // 开户行
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZhiXiao_KaiHuMing, model.ZhiXiao_KaiHuMing);  // 开户名
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZhiXiao_BandNum, model.ZhiXiao_BandNum);      // 银行卡号

                // 直销用户分组信息
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZhiXiao_ParentId, parentUser.Id);

                var teamId = parentUser.GetAttribute<int>(SystemCustomerAttributeNames.ZhiXiao_TeamId);
                var team = _customerTeamService.GetCustomerTeamById(teamId);

                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZhiXiao_TeamId, teamId);

                var sortId = _customerTeamService.GetNewUserSortId(team);
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZhiXiao_InTeamOrder, sortId);
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZhiXiao_InTeamTime, DateTime.UtcNow);
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZhiXiao_LevelId, (int)CustomerLevel.ZuYuan);

                // 2. Update Parent Info
                _genericAttributeService.SaveAttribute(parentUser, SystemCustomerAttributeNames.ZhiXiao_ChildCount, parentChildCount + 1);

                // 1. 给组长， 副组长分钱, 2. 如果人数满足, 重新分组
                _customerTeamService.AddNewUserToTeam(team, customer);

                //activity log
                _customerActivityService.InsertActivity("AddNewCustomer", _localizationService.GetResource("ActivityLog.AddNewCustomer"), customer.Id);

                SuccessNotification(_localizationService.GetResource("Admin.Customers.Customers.Added"));

                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabName();

                    return RedirectToAction("Edit", new { id = customer.Id });
                }
                return RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            PrepareCustomerModel(model, null, true);
            return View("RegisterZhiXiaoUser", model);
        }

        #endregion

        #region admin Manage

        public virtual ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public virtual ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            //load registered customers by default
            var defaultRoleIds = new List<int> {
                _customerService.GetCustomerRoleBySystemName(SystemCustomerRoleNames.Registered).Id,
                _customerService.GetCustomerRoleBySystemName(SystemCustomerRoleNames.Registered_Advanced).Id
            };
            var model = new CustomerListModel
            {
                UsernamesEnabled = _customerSettings.UsernamesEnabled,
                DateOfBirthEnabled = _customerSettings.DateOfBirthEnabled,
                CompanyEnabled = _customerSettings.CompanyEnabled,
                PhoneEnabled = _customerSettings.PhoneEnabled,
                ZipPostalCodeEnabled = _customerSettings.ZipPostalCodeEnabled,
                SearchCustomerRoleIds = defaultRoleIds,
            };

            var allRoles = _customerService.GetAllCustomerRoles(true);
            foreach (var role in allRoles)
            {
                model.AvailableCustomerRoles.Add(new SelectListItem
                {
                    Text = role.Name,
                    Value = role.Id.ToString(),
                    Selected = defaultRoleIds.Any(x => x == role.Id)
                });
            }

            return View(model);
        }

        [HttpPost]
        public virtual ActionResult CustomerList(DataSourceRequest command, CustomerListModel model,
        [ModelBinder(typeof(CommaSeparatedModelBinder))]int[] searchCustomerRoleIds)
        {
            //we use own own binder for searchCustomerRoleIds property 
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedKendoGridJson();

            var searchDayOfBirth = 0;
            int searchMonthOfBirth = 0;
            if (!String.IsNullOrWhiteSpace(model.SearchDayOfBirth))
                searchDayOfBirth = Convert.ToInt32(model.SearchDayOfBirth);
            if (!String.IsNullOrWhiteSpace(model.SearchMonthOfBirth))
                searchMonthOfBirth = Convert.ToInt32(model.SearchMonthOfBirth);

            var customers = _customerService.GetAllCustomers(
                customerRoleIds: searchCustomerRoleIds,
                email: model.SearchEmail,
                username: model.SearchUsername,
                firstName: model.SearchFirstName,
                lastName: model.SearchLastName,
                dayOfBirth: searchDayOfBirth,
                monthOfBirth: searchMonthOfBirth,
                company: model.SearchCompany,
                phone: model.SearchPhone,
                zipPostalCode: model.SearchZipPostalCode,
                ipAddress: model.SearchIpAddress,
                loadOnlyWithShoppingCart: false,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = customers.Select(PrepareCustomerModelForList),
                Total = customers.Count
            };

            return Json(gridModel);
        }

        public virtual ActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(id);
            if (customer == null || customer.Deleted)
                //No customer found with the specified id
                return RedirectToAction("List");

            var model = new CustomerModel();
            PrepareCustomerModel(model, customer, false);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        [ValidateInput(false)]
        public virtual ActionResult Edit(CustomerModel model, bool continueEditing/*, FormCollection form*/)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(model.Id);
            if (customer == null || customer.Deleted)
                //No customer found with the specified id
                return RedirectToAction("List");

            //validate customer roles
            var allCustomerRoles = _customerService.GetAllCustomerRoles(true);
            var newCustomerRoles = new List<CustomerRole>();
            foreach (var customerRole in allCustomerRoles)
                if (model.SelectedCustomerRoleIds.Contains(customerRole.Id))
                    newCustomerRoles.Add(customerRole);
            var customerRolesError = ValidateCustomerRoles(newCustomerRoles);
            if (!String.IsNullOrEmpty(customerRolesError))
            {
                ModelState.AddModelError("", customerRolesError);
                ErrorNotification(customerRolesError, false);
            }

            // Ensure that valid email address is entered if Registered role is checked to avoid registered customers with empty email address
            if (newCustomerRoles.Any() && newCustomerRoles.FirstOrDefault(c => c.SystemName == SystemCustomerRoleNames.Registered) != null && !CommonHelper.IsValidEmail(model.Email))
            {
                ModelState.AddModelError("", _localizationService.GetResource("Admin.Customers.Customers.ValidEmailRequiredRegisteredRole"));
                ErrorNotification(_localizationService.GetResource("Admin.Customers.Customers.ValidEmailRequiredRegisteredRole"), false);
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
                try
                {
                    customer.AdminComment = model.AdminComment;
                    customer.IsTaxExempt = model.IsTaxExempt;

                    //prevent deactivation of the last active administrator
                    if (!customer.IsAdmin() || model.Active || SecondAdminAccountExists(customer))
                        customer.Active = model.Active;
                    else
                        ErrorNotification(_localizationService.GetResource("Admin.Customers.Customers.AdminAccountShouldExists.Deactivate"));

                    //email
                    if (!String.IsNullOrWhiteSpace(model.Email))
                    {
                        _customerRegistrationService.SetEmail(customer, model.Email, false);
                    }
                    else
                    {
                        customer.Email = model.Email;
                    }

                    //username
                    if (_customerSettings.UsernamesEnabled)
                    {
                        if (!String.IsNullOrWhiteSpace(model.Username))
                        {
                            _customerRegistrationService.SetUsername(customer, model.Username);
                        }
                        else
                        {
                            customer.Username = model.Username;
                        }
                    }

                    // phone number
                    if (_customerSettings.PhoneEnabled)
                    {
                        if (!String.IsNullOrWhiteSpace(model.Phone))
                        {
                            _customerRegistrationService.SetPhoneNumber(customer, model.Phone);
                        }
                    }

                    // nick name
                    if (!String.IsNullOrWhiteSpace(model.NickName))
                    {
                        _customerRegistrationService.SetNickName(customer, model.NickName);
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
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.City, model.City);
                    //if (_customerSettings.CountryEnabled)
                    //    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.CountryId, model.CountryId);
                    //if (_customerSettings.CountryEnabled && _customerSettings.StateProvinceEnabled)
                    //    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.StateProvinceId, model.StateProvinceId);
                    if (_customerSettings.PhoneEnabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Phone, model.Phone);
                    //if (_customerSettings.FaxEnabled)
                    //    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Fax, model.Fax);

                    //custom customer attributes
                    //_genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.CustomCustomerAttributes, customerAttributesXml);

                    //newsletter subscriptions omit ...

                    //customer roles
                    foreach (var customerRole in allCustomerRoles)
                    {
                        //ensure that the current customer cannot add/remove to/from "Administrators" system role
                        //if he's not an admin himself
                        if (customerRole.SystemName == SystemCustomerRoleNames.Administrators &&
                            !_workContext.CurrentCustomer.IsAdmin())
                            continue;

                        if (model.SelectedCustomerRoleIds.Contains(customerRole.Id))
                        {
                            //new role
                            if (customer.CustomerRoles.Count(cr => cr.Id == customerRole.Id) == 0)
                                customer.CustomerRoles.Add(customerRole);
                        }
                        else
                        {
                            //prevent attempts to delete the administrator role from the user, if the user is the last active administrator
                            if (customerRole.SystemName == SystemCustomerRoleNames.Administrators && !SecondAdminAccountExists(customer))
                            {
                                ErrorNotification(_localizationService.GetResource("Admin.Customers.Customers.AdminAccountShouldExists.DeleteRole"));
                                continue;
                            }

                            //remove role
                            if (customer.CustomerRoles.Count(cr => cr.Id == customerRole.Id) > 0)
                                customer.CustomerRoles.Remove(customerRole);
                        }
                    }
                    _customerService.UpdateCustomer(customer);


                    //ensure that a customer with a vendor associated is not in "Administrators" role
                    //otherwise, he won't have access to the other functionality in admin area
                    if (customer.IsAdmin() && customer.VendorId > 0)
                    {
                        customer.VendorId = 0;
                        _customerService.UpdateCustomer(customer);
                        ErrorNotification(_localizationService.GetResource("Admin.Customers.Customers.AdminCouldNotbeVendor"));
                    }

                    //ensure that a customer in the Vendors role has a vendor account associated.
                    //otherwise, he will have access to ALL products
                    if (customer.IsVendor() && customer.VendorId == 0)
                    {
                        var vendorRole = customer
                            .CustomerRoles
                            .FirstOrDefault(x => x.SystemName == SystemCustomerRoleNames.Vendors);
                        customer.CustomerRoles.Remove(vendorRole);
                        _customerService.UpdateCustomer(customer);
                        ErrorNotification(_localizationService.GetResource("Admin.Customers.Customers.CannotBeInVendoRoleWithoutVendorAssociated"));
                    }


                    //activity log
                    _customerActivityService.InsertActivity("EditCustomer", _localizationService.GetResource("ActivityLog.EditCustomer"), customer.Id);

                    SuccessNotification(_localizationService.GetResource("Admin.Customers.Customers.Updated"));
                    if (continueEditing)
                    {
                        //selected tab
                        SaveSelectedTabName();

                        return RedirectToAction("Edit", new { id = customer.Id });
                    }
                    return RedirectToAction("List");
                }
                catch (Exception exc)
                {
                    ErrorNotification(exc.Message, false);
                }
            }

            //If we got this far, something failed, redisplay form
            PrepareCustomerModel(model, customer, true);
            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("changepassword")]
        public virtual ActionResult ChangePassword(CustomerModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(model.Id);
            if (customer == null)
                //No customer found with the specified id
                return RedirectToAction("List");

            //ensure that the current customer cannot change passwords of "Administrators" if he's not an admin himself
            if (customer.IsAdmin() && !_workContext.CurrentCustomer.IsAdmin())
            {
                ErrorNotification(_localizationService.GetResource("Admin.Customers.Customers.OnlyAdminCanChangePassword"));
                return RedirectToAction("Edit", new { id = customer.Id });
            }

            if (ModelState.IsValid)
            {
                var changePassRequest = new ChangePasswordRequest(model.Email,
                    false, _customerSettings.DefaultPasswordFormat, model.Password);
                var changePassResult = _customerRegistrationService.ChangePassword(changePassRequest);
                if (changePassResult.Success)
                    SuccessNotification(_localizationService.GetResource("Admin.Customers.Customers.PasswordChanged"));
                else
                    foreach (var error in changePassResult.Errors)
                        ErrorNotification(error);
            }

            return RedirectToAction("Edit", new { id = customer.Id });
        }

        [HttpPost]
        public virtual ActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(id);
            if (customer == null)
                //No customer found with the specified id
                return RedirectToAction("List");

            try
            {
                //prevent attempts to delete the user, if it is the last active administrator
                if (customer.IsAdmin() && !SecondAdminAccountExists(customer))
                {
                    ErrorNotification(_localizationService.GetResource("Admin.Customers.Customers.AdminAccountShouldExists.DeleteAdministrator"));
                    return RedirectToAction("Edit", new { id = customer.Id });
                }

                //ensure that the current customer cannot delete "Administrators" if he's not an admin himself
                if (customer.IsAdmin() && !_workContext.CurrentCustomer.IsAdmin())
                {
                    ErrorNotification(_localizationService.GetResource("Admin.Customers.Customers.OnlyAdminCanDeleteAdmin"));
                    return RedirectToAction("Edit", new { id = customer.Id });
                }

                //delete
                _customerService.DeleteCustomer(customer);

                //remove newsletter subscription (if exists)
                //foreach (var store in _storeService.GetAllStores())
                //{
                //    var subscription = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreId(customer.Email, store.Id);
                //    if (subscription != null)
                //        _newsLetterSubscriptionService.DeleteNewsLetterSubscription(subscription);
                //}

                //activity log
                _customerActivityService.InsertActivity("DeleteCustomer", _localizationService.GetResource("ActivityLog.DeleteCustomer"), customer.Id);

                SuccessNotification(_localizationService.GetResource("Admin.Customers.Customers.Deleted"));
                return RedirectToAction("List");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc.Message);
                return RedirectToAction("Edit", new { id = customer.Id });
            }
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("impersonate")]
        public virtual ActionResult Impersonate(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AllowCustomerImpersonation))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(id);
            if (customer == null)
                //No customer found with the specified id
                return RedirectToAction("List");

            //ensure that a non-admin user cannot impersonate as an administrator
            //otherwise, that user can simply impersonate as an administrator and gain additional administrative privileges
            if (!_workContext.CurrentCustomer.IsAdmin() && customer.IsAdmin())
            {
                ErrorNotification(_localizationService.GetResource("Admin.Customers.Customers.NonAdminNotImpersonateAsAdminError"));
                return RedirectToAction("Edit", customer.Id);
            }

            //activity log
            _customerActivityService.InsertActivity("Impersonation.Started", _localizationService.GetResource("ActivityLog.Impersonation.Started.StoreOwner"), customer.Email, customer.Id);
            _customerActivityService.InsertActivity(customer, "Impersonation.Started", _localizationService.GetResource("ActivityLog.Impersonation.Started.Customer"), _workContext.CurrentCustomer.Email, _workContext.CurrentCustomer.Id);

            //ensure login is not required
            customer.RequireReLogin = false;
            _customerService.UpdateCustomer(customer);
            _genericAttributeService.SaveAttribute<int?>(_workContext.CurrentCustomer, SystemCustomerAttributeNames.ImpersonatedCustomerId, customer.Id);

            return RedirectToAction("Index", "Home", new { area = "" });
        }

        #endregion

        #region Activity log

        [HttpPost]
        public virtual ActionResult ListActivityLog(DataSourceRequest command, int customerId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedKendoGridJson();

            var activityLog = _customerActivityService.GetAllActivities(null, null, customerId, 0, command.Page - 1, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = activityLog.Select(x =>
                {
                    var m = new CustomerModel.ActivityLogModel
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
    }
}