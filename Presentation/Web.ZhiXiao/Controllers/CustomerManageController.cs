using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Nop.Admin.Helpers;
using Nop.Admin.Models.Logging;
using Nop.Core;
using Nop.Core.Domain;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.ZhiXiao;
using Nop.Core.Infrastructure;
using Nop.Extensions;
using Nop.Models.Customers;
using Nop.Services.Authentication;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.News;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Services.ZhiXiao;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc;
using Web.ZhiXiao.Factories;
using static Nop.Models.Customers.CustomerListModel;

namespace Web.ZhiXiao.Controllers
{
    public class CustomerManageController : BaseAdminController
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
        private readonly IZhiXiaoService _zhiXiaoService;
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
        private readonly IRegisterZhiXiaoUserHelper _registerZhiXiaoUserHelper;

        #endregion

        #region Ctor

        public CustomerManageController(
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
            IZhiXiaoService zhiXiaoTeamService,
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
            IPermissionService permissionService,
            IRegisterZhiXiaoUserHelper registerZhiXiaoUserHelper)
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
            this._zhiXiaoService = zhiXiaoTeamService;
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
            this._registerZhiXiaoUserHelper = registerZhiXiaoUserHelper;
        }

        #endregion

        #region  Utilities

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
        protected virtual CustomerModel PrepareCustomerModelForZhiXiaoInfo(Customer customer)
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

                NickName = customer.GetNickName(),
                ZhiXiao_MoneyNum = customer.GetMoneyNum(),

                ZhiXiao_IdCardNum = customer.GetAttribute<string>(SystemCustomerAttributeNames.ZhiXiao_IdCardNum),
                ZhiXiao_YinHang = customer.GetAttribute<string>(SystemCustomerAttributeNames.ZhiXiao_YinHang),        // 银行
                ZhiXiao_KaiHuHang = customer.GetAttribute<string>(SystemCustomerAttributeNames.ZhiXiao_KaiHuHang),      // 开户行
                ZhiXiao_KaiHuMing = customer.GetAttribute<string>(SystemCustomerAttributeNames.ZhiXiao_KaiHuMing),     // 开户名
                ZhiXiao_BandNum = customer.GetAttribute<string>(SystemCustomerAttributeNames.ZhiXiao_BandNum),      // 银行卡号

                ZhiXiao_LevelId = customer.GetAttribute<int>(SystemCustomerAttributeNames.ZhiXiao_LevelId),
                ZhiXiao_MoneyHistory = customer.GetAttribute<long>(SystemCustomerAttributeNames.ZhiXiao_MoneyHistory),
            };
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

                NickName = customer.GetNickName(),
                ZhiXiao_MoneyNum = customer.GetMoneyNum(),
                ProductStatusId = customer.GetAttribute<int>(SystemCustomerAttributeNames.ZhiXiao_SendProductStatus) // 发货状态
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

                    //var teamId = customer.GetAttribute<int>(SystemCustomerAttributeNames.ZhiXiao_TeamId);
                    model.CustomerTeam = customer.CustomerTeam;
                    model.ZhiXiao_LevelId = customer.GetAttribute<int>(SystemCustomerAttributeNames.ZhiXiao_LevelId);
                    model.ZhiXiao_MoneyNum = customer.GetAttribute<long>(SystemCustomerAttributeNames.ZhiXiao_MoneyNum);
                    model.ZhiXiao_MoneyHistory = customer.GetAttribute<long>(SystemCustomerAttributeNames.ZhiXiao_MoneyHistory);

                    model.ProductStatusId = customer.GetAttribute<int>(SystemCustomerAttributeNames.ZhiXiao_SendProductStatus); // 发货状态
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
            //precheck Registered Role as a default role while creating a new customer through admin
            if (customer != null)
            {
                var allRoles = _customerService.GetAllCustomerRoles(true);
                var managerRole = allRoles.FirstOrDefault(c => c.SystemName == SystemCustomerRoleNames.Managers);
                if (managerRole != null)
                    model.SelectedCustomerRoleIds.Add(managerRole.Id);

                foreach (var role in allRoles)
                {
                    model.AvailableCustomerRoles.Add(new SelectListItem
                    {
                        Text = role.Name,
                        Value = role.Id.ToString(),
                        Selected = model.SelectedCustomerRoleIds.Contains(role.Id)
                    });
                }
            }

            // parent为直销用户
            var zhiXiaoRoleIds = _customerService.GetRoles_ZhiXiao(true)
                                .Select(x => x.Id).ToArray();
            var availableParents = _customerService.GetAllCustomers(customerRoleIds: zhiXiaoRoleIds);

            foreach (var item in availableParents)
            {
                //var nickName = item.GetAttribute<string>(SystemCustomerAttributeNames.ZhiXiao_NickName);
                model.AvailableParents.Add(new SelectListItem()
                {
                    Text = item.GetNickNameAndUserName(),
                    Value = item.Id.ToString(),
                    Selected = model.ParentUser != null && model.ParentUser.Id == item.Id
                });
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

        #region Register zhixiao user

        [ParameterBasedOnQueryString("advanced", "registerAdvanceUser")]
        public virtual ActionResult RegisterZhiXiaoUser(bool registerAdvanceUser)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var model = new CustomerModel();
            PrepareCustomerModel(model, null, true);
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
        public virtual ActionResult RegisterZhiXiaoUser(CustomerModel model, bool registerAdvanceUser)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            var validateResult = _registerZhiXiaoUserHelper.ValidateCustomerModel(model, registerAdvanceUser, true);
            foreach (var error in validateResult.Errors)
            {
                ModelState.AddModelError("", error);
            }

            if (ModelState.IsValid)
            {
                var errors = _registerZhiXiaoUserHelper.RegisterNewUser(model, validateResult);

                foreach (var error in errors)
                {
                    ErrorNotification(error);
                }

                SuccessNotification(_localizationService.GetResource("Admin.Customers.Customers.Added"));
                return RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            PrepareCustomerModel(model, null, true);

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

        #region admin Manage

        public virtual ActionResult Index()
        {
            for (int i = 1; i <= 11; i++)
            {
                var team = _zhiXiaoService.GetAllCustomerTeams().First();
                var customer = _customerService.GetCustomerByUsername("user_" + i);

                customer.CustomerTeam = team;
                _customerService.UpdateCustomer(customer);
            }
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

        public virtual ActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var model = new CustomerModel();
            PrepareCustomerModel(model, null, false);
            //default value
            model.Active = true;
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        [ValidateInput(false)]
        public virtual ActionResult Create(CustomerModel model, bool continueEditing, FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
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
                    ModelState.AddModelError("", "Username is already registered");
            }

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
            //if (newCustomerRoles.Any() && newCustomerRoles.FirstOrDefault(c => c.SystemName == SystemCustomerRoleNames.Registered) != null && !CommonHelper.IsValidEmail(model.Email))
            //{
            //    ModelState.AddModelError("", _localizationService.GetResource("Admin.Customers.Customers.ValidEmailRequiredRegisteredRole"));
            //    ErrorNotification(_localizationService.GetResource("Admin.Customers.Customers.ValidEmailRequiredRegisteredRole"), false);
            //}

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
                var customer = new Customer
                {
                    CustomerGuid = Guid.NewGuid(),
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


                //newsletter subscriptions

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

                //ensure that a customer with a vendor associated is not in "Administrators" role
                //otherwise, he won't have access to other functionality in admin area
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

        [HttpPost]
        public virtual ActionResult SendProduct(int id, [Bind(Prefix = "SendProduct")]SendProductModel sendModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(id);
            if (customer == null || customer.Deleted)
                //No customer found with the specified id
                return RedirectToAction("List");

            try
            {
                if (_zhiXiaoService.GetSendProductStatus(customer) != SendProductStatus.NotYet)
                    throw new Exception("该用户的产品已经发货");

                if (string.IsNullOrEmpty(sendModel.OrderNo))
                    throw new NopException("快递单号不能为空");

                _customerActivityService.InsertActivity(SystemZhiXiaoLogTypes.SendProduct,
                    "给用户 {0} 发货, 快递单号: {1}, 备注: {2}",
                    customer.GetNickNameAndUserName(),
                    sendModel.OrderNo,
                    sendModel.Comment);

                var userProductLog = _customerActivityService.InsertActivity(customer,
                     SystemZhiXiaoLogTypes.SendProduct,
                     "快递单号: {0}, 备注: {1}",
                      sendModel.OrderNo,
                      sendModel.Comment);

                _zhiXiaoService.SetSendProductStatus(customer, SendProductStatus.Sended);
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZhiXiao_SendProductLogId, (int)userProductLog.Id);
            }
            catch (Exception exc)
            {
                ErrorNotification(exc.Message);
            }

            return RedirectToAction("List");
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
            //if (newCustomerRoles.Any() && newCustomerRoles.FirstOrDefault(c => c.SystemName == SystemCustomerRoleNames.Registered) != null && !CommonHelper.IsValidEmail(model.Email))
            //{
            //    ModelState.AddModelError("", _localizationService.GetResource("Admin.Customers.Customers.ValidEmailRequiredRegisteredRole"));
            //    ErrorNotification(_localizationService.GetResource("Admin.Customers.Customers.ValidEmailRequiredRegisteredRole"), false);
            //}

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

                    // 直销用户个人信息 (只在编辑时更新, Create时不显示这些信息)
                    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZhiXiao_NickName, model.NickName);
                    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZhiXiao_Password2, model.Password2);          // 二级密码
                    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZhiXiao_IdCardNum, model.ZhiXiao_IdCardNum);
                    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZhiXiao_YinHang, model.ZhiXiao_YinHang);      // 银行
                    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZhiXiao_KaiHuHang, model.ZhiXiao_KaiHuHang);  // 开户行
                    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZhiXiao_KaiHuMing, model.ZhiXiao_KaiHuMing);  // 开户名
                    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZhiXiao_BandNum, model.ZhiXiao_BandNum);      // 银行卡号

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

        /// <summary>
        /// 用户金钱变化log
        /// SystemZhiXiaoLogTypes
        /// </summary>
        public virtual ActionResult ListMoneyLog(DataSourceRequest command, int customerId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedKendoGridJson();

            var activityLog = _customerActivityService.GetAllActivitiesByTypes(new string[]
            {
                SystemZhiXiaoLogTypes.AddNewUser,
                SystemZhiXiaoLogTypes.ReGroupTeam_AddMoney,
                SystemZhiXiaoLogTypes.ReGroupTeam_ReSort,
                SystemZhiXiaoLogTypes.ReGroupTeam_UpdateLevel,
                SystemZhiXiaoLogTypes.RechargeMoney,
                SystemZhiXiaoLogTypes.Withdraw,
                SystemZhiXiaoLogTypes.ProcessWithdraw,
            }, customerId, command.Page - 1, command.PageSize);

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

        /// <summary>
        /// 用户所有log
        /// </summary>
        /// <param name="command"></param>
        /// <param name="customerId"></param>
        /// <returns></returns>
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

        #region 直销

        /// <summary>
        /// 充值电子币
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
                return RedirectToAction("List");

            ViewBag.NickName = customer.GetNickName();
            ViewBag.UserName = customer.Username;
            ViewBag.MoneyNum = customer.GetMoneyNum();

            return View();
        }

        [HttpPost]
        /// <summary>
        /// 充值电子币
        /// </summary>
        public virtual ActionResult Recharge(int id, int amount)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedKendoGridJson();

            var customer = _customerService.GetCustomerById(id);
            if (customer == null)
                return RedirectToAction("List");

            if (amount == 0)
                ModelState.AddModelError("", "充值金额不能为0");

            if (ModelState.IsValid)
            {
                try
                {
                    _zhiXiaoService.UpdateMoneyForUserAndLog(customer, amount, SystemZhiXiaoLogTypes.RechargeMoney,
                        "管理员充值电子币{0}", amount);

                    //activity log
                    _customerActivityService.InsertActivity(SystemZhiXiaoLogTypes.RechargeMoney,
                        _localizationService.GetResource("给用户{0}充值电子币{1}"),
                        customer.GetNickNameAndUserName(),
                        amount);

                    SuccessNotification(string.Format("用户{0}充值{1}", customer.GetNickName(), amount));
                }
                catch (Exception ex)
                {
                    ErrorNotification("充值失败, " + ex.Message);
                }

            }

            ViewBag.NickName = customer.GetNickName();
            ViewBag.UserName = customer.Username;
            ViewBag.MoneyNum = customer.GetMoneyNum();
            return View();
        }

        /// <summary>
        /// 提现申请
        /// </summary>
        public virtual ActionResult Withdraw()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedKendoGridJson();

            var model = new WithdrawLogSearchModel();
            return View(model);
        }

        /// <summary>
        /// 提现申请
        /// </summary>
        [HttpPost, ActionName("Withdraw")]
        public virtual ActionResult WithdrawList(DataSourceRequest command,
            WithdrawLogSearchModel searchModel)
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
            model.CustomerModel = PrepareCustomerModelForZhiXiaoInfo(withDraw.Customer);
            var customer = _customerService.GetCustomerById(model.CustomerId);
            PrepareCustomerModel(model.CustomerModel, customer, false);
            return View(model);
        }

        /// <summary>
        /// Process withdraw
        /// </summary>
        /// <param name="id">Withdraw id</param> 
        [HttpPost]
        public virtual ActionResult ProcessWithdrawPopup(string btnId, int id, bool isDone)
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
                _customerActivityService.InsertActivity(SystemZhiXiaoLogTypes.ProcessWithdraw,
                    "管理员通过用户{0}的{1}电子币提现申请",
                    customer.GetNickNameAndUserName(),
                    withDraw.Amount);

                _customerActivityService.InsertActivity(withDraw.Customer, SystemZhiXiaoLogTypes.ProcessWithdraw,
                    "管理员通过提现{0}电子币申请",
                    withDraw.Amount);
            }

            ViewBag.btnId = btnId;
            ViewBag.RefreshPage = true;

            var model = withDraw.ToModel();
            return View(model);
        }


        public void InstallTestNews()
        {
            var newsService = EngineContext.Current.Resolve<INewsService>();
            for (int i = 0; i < 25; i++)
            {
                var newsItem = new Nop.Core.Domain.News.NewsItem();
                newsItem.LanguageId = 2;
                newsItem.Title = "测试新闻" + i;
                newsItem.Short = "新闻描述" + i;
                newsItem.Published = true;
                
                if (i % 2 == 0)
                {
                    newsItem.StartDateUtc = DateTime.UtcNow.AddMinutes(-(i * 5));
                    newsItem.EndDateUtc = DateTime.UtcNow.AddDays((i * 10));
                }

                newsItem.Full = "第" + i + @"条测试内容 : 历史长河奔流不息，真理之光穿越时空。1818年5月5日，德国小城特里尔，一代伟人马克思诞生。200年后，世界东方。高擎马克思主义的精神火炬，在习近平新时代中国特色社会主义思想指引下，中国共产党正领导亿万人民书写人类发展史上新的奇迹。这是2018年5月3日在德国特里尔马克思故居纪念馆内拍摄的1848年出版的《共产党宣言》第一版。 新华社记者单宇琦摄
历史长河奔流不息，真理之光穿越时空。
1818年5月5日，德国小城特里尔，一代伟人马克思诞生。
<img src='http://p3.ifengimg.com/a/2018_19/05b8c24d55616be_size293_w900_h658.jpg'>
200年后，世界东方。高擎马克思主义的精神火炬，在习近平新时代中国特色社会主义思想指引下，中国共产党正领导亿万人民书写人类发展史上新的奇迹。
“历史和人民选择马克思主义是完全正确的，中国共产党把马克思主义写在自己的旗帜上是完全正确的，坚持马克思主义基本原理同中国具体实际相结合、不断推进马克思主义中国化时代化是完全正确的！”
2018年5月4日，纪念马克思诞辰200周年大会在北京人民大会堂隆重举行，习近平总书记向世人庄严宣示——
“前进道路上，我们要继续高扬马克思主义伟大旗帜，让马克思、恩格斯设想的人类社会美好前景不断在中国大地上生动展现出来！”
历史长河奔流不息，真理之光穿越时空。1818年5月5日，德国小城特里尔，一代伟人马克思诞生。200年后，世界东方。高擎马克思主义的精神火炬，在习近平新时代中国特色社会主义思想指引下，中国共产党正领导亿万人民书写人类发展史上新的奇迹。这是2018年5月3日在德国特里尔市立西麦翁博物馆拍摄的马克思主题展入口。 新华社记者单宇琦摄";
                
                newsItem.CreatedOnUtc = DateTime.UtcNow;
                newsService.InsertNews(newsItem);
                
                _customerActivityService.InsertActivity("AddNewNews", _localizationService.GetResource("ActivityLog.AddNewNews"), newsItem.Id);
            }
        }

        public void InsallLangData()
        {
            var localResources = EngineContext.Current.Resolve<ILocalizationService>();
            //localResources.InsertLocaleStringResource(new LocaleStringResource
            //{
            //    LanguageId = 2,
            //    ResourceName = "Account.Fields.ConfirmPassword2",
            //    ResourceValue = "确认二级密码"
            //});
            localResources.InsertLocaleStringResource(new LocaleStringResource
            {
                LanguageId = 2,
                ResourceName = "account.changepassword.errors.passwordmatcheswithprevious",
                ResourceValue = "新的密码之前已经使用过, 为了安全, 请重新输入"
            });

            
        }
        public void InstallRequiredData()
        {

            var types = new List<ActivityLogType>() {
                //new ActivityLogType
                //{
                //    SystemKeyword = SystemZhiXiaoLogTypes.RechargeMoney,
                //    Enabled = true,
                //    Name = "充值电子币"
                //},
                //new ActivityLogType
                //{
                //    SystemKeyword = SystemZhiXiaoLogTypes.SendProduct,
                //    Enabled = true,
                //    Name = "管理员发货"
                //},
                new ActivityLogType
                {
                    SystemKeyword = SystemZhiXiaoLogTypes.ProcessWithdraw,
                    Enabled = true,
                    Name = "管理员处理提现申请"
                },
                new ActivityLogType
                {
                    SystemKeyword = SystemZhiXiaoLogTypes.Withdraw,
                    Enabled = true,
                    Name = "提现申请"
                },
            };

            // 用属性来关联是否收货

            var repos = EngineContext.Current.Resolve<Nop.Core.Data.IRepository<ActivityLogType>>();

            repos.Insert(types);

            var settingService = EngineContext.Current.Resolve<Nop.Services.Configuration.ISettingService>();
            // 直销相关配置
            settingService.SaveSetting(new ZhiXiaoSettings
            {
                ///<summary>
                ///二级密码缓存时间
                /// </summary>
                Password2_ValidTime = 15,
                /// <summary>
                /// 提现比例
                /// </summary>
                Withdraw_Rate = 0.95,
                /// <summary>
                /// 注册普通用户需要金币
                /// </summary>
                Register_Money_NormalUser = 10000,
                /// <summary>
                /// 注册高级用户需要钱
                /// </summary>
                Register_Money_AdvancedUser = 26800,
                /// <summary>
                /// 最多下线个数
                /// </summary>
                MaxChildCount = 2,

                /// <summary>
                /// 小组中组长个数为1
                /// </summary>
                Team_ZuZhangCount = 1,
                /// <summary>
                /// 小组中副组长个数为2
                /// </summary>
                Team_FuZuZhangCount = 2,

                /// <summary>
                /// 小组初始人数为7人
                /// </summary>
                TeamInitUserCount = 7,
                /// <summary>
                /// 小组满足重新分组人数 (TeamInitUserCount * 2 + 1)
                /// </summary>
                TeamReGroupUserCount = 15,

                /// <summary>
                /// 新增用户时组长分的钱
                /// </summary>
                NewUserMoney_ZuZhang_Normal = 3000,
                NewUserMoney_ZuZhang_Advanced = 3000,

                /// <summary>
                /// 新增用户时副组长分的钱
                /// </summary>
                NewUserMoney_FuZuZhang_Normal = 800,
                NewUserMoney_FuZuZhang_Advanced = 1000,

                /// <summary>
                /// 五星董事出盘, 奖励27万(五星董事升级！奖金30万， 扣除3万的税)
                /// </summary>
                ReGroupMoney_DongShi5_ChuPan_Normal = 250000,
                /// <summary>
                /// 五星董事出盘, 奖励27万(五星董事升级！奖金30万， 扣除3万的税)
                /// </summary>
                ReGroupMoney_DongShi5_ChuPan_Advanced = 270000,
                /// <summary>
                /// 组长升级, 董事级别的推荐人根据级别拿提成的基数 84000 + 8000 + 1600 * 4 = 98400
                /// </summary>
                ReGroupMoney_DongShiBase_Normal = 98400,
                ReGroupMoney_DongShiBase_Advanced = 98400,
                ReGroupMoney_Rate_DongShi1 = 0.02,
                ReGroupMoney_Rate_DongShi2 = 0.04,
                ReGroupMoney_Rate_DongShi3 = 0.06,
                ReGroupMoney_Rate_DongShi4 = 0.08,
                ReGroupMoney_Rate_DongShi5 = 0.02,
                /// <summary>
                /// 重新分组时组长分的钱
                /// </summary>
                ReGroupMoney_ZuZhang_Normal = 40000,
                ReGroupMoney_ZuZhang_Advanced = 50000,
                /// <summary>
                /// 重新分组时前x个组员分钱
                /// </summary>
                ReGroupMoney_ZuYuan_Count = 4,
                /// <summary>
                /// 重新分组时组员钱数(一般用户)
                /// </summary>
                ReGroupMoney_ZuYuan_Normal = 1200,
                /// <summary>
                /// 重新分组时组员钱数(高级用户)
                /// </summary>
                ReGroupMoney_ZuYuan_Advanced = 1600
            });

            //var firstCustomer = _customerService.GetCustomerByUsername("user_1");
            //_customerActivityService.InsertWithdraw(firstCustomer, 500, "申请提现500", "");
        }

        #endregion
    }
}