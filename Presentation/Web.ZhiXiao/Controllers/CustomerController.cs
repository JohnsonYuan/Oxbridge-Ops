using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Nop.Admin.Helpers;
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
        private readonly IRegisterZhiXiaoUserHelper _registerZhiXiaoUserHelper;

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

            this._registerZhiXiaoUserHelper = registerZhiXiaoUserHelper;
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

            var validateResult = _registerZhiXiaoUserHelper.ValidateCustomerModel(model, registerAdvanceUser, false);
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