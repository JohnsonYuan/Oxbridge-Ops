using System;
using System.Web;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Domain;
using Nop.Core.Domain.Customers;
using Nop.Models.Customers;
using Nop.Services.Authentication;
using Nop.Services.BonusApp.Authentication;
using Nop.Services.BonusApp.Customers;
using Nop.Services.BonusApp.Logging;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Security;
using Web.ZhiXiao.Factories;

namespace Web.ZhiXiao.Areas.BonusApp.Controllers
{
    public class CommonController : BonusAppBaseController
    {
        #region Fields

        private readonly IWebHelper _webHelper;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILogger _logger;
        private readonly IWorkContext _workContext;
        private readonly IPermissionService _permissionService;
        private readonly IBonusApp_CustomerService _customerService;

        private readonly IBonusAppFormsAuthenticationService _authenticationService;
        private readonly ILocalizationService _localizationService;
        private readonly HttpContextBase _httpContext;

        //fields for customer login/logont

        private readonly IBonusApp_CustomerActivityService _customerActivityService;
        private readonly CustomerSettings _customerSettings;

        #endregion

        #region Constructors

        public CommonController(IWebHelper webHelper,
            IDateTimeHelper dateTimeHelper,
            ILogger logger,
            IWorkContext workContext,
            IPermissionService permissionService,
            IBonusApp_CustomerService customerService,
            IBonusAppFormsAuthenticationService bonusAppAuthenticationService,
            ILocalizationService localizationService,
            HttpContextBase httpContext,

            IGenericAttributeService genericAttributeService,
            IBonusApp_CustomerActivityService customerActivityService,
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
            this._authenticationService = bonusAppAuthenticationService;
            this._localizationService = localizationService;
            this._httpContext = httpContext;

            this._customerActivityService = customerActivityService;
            this._customerSettings = customerSettings;
        }

        #endregion

        #region Login / logout

        // GET: Customer
        public virtual ActionResult Login(bool? checkoutAsGuest)
        {
            return View();
        }

        [HttpPost]
        public virtual ActionResult Login(LoginModel model, string returnUrl)
        {
            if (string.IsNullOrEmpty(model.Username))
                return ErrorJson("请输入用户名");
            if (string.IsNullOrEmpty(model.Password))
                return ErrorJson("请输入密码");

            var loginResult =
               _customerService.ValidateCustomer(model.Username, model.Password);

            switch (loginResult)
            {
                case CustomerLoginResults.Successful:
                    {
                        var customer = _customerService.GetCustomerByUsername(model.Username);

                        //sign in new customer
                        _authenticationService.SignIn(customer, model.RememberMe);

                        //activity log
                        _customerActivityService.InsertActivity(customer, "PublicStore.Login", _localizationService.GetResource("ActivityLog.PublicStore.Login"));

                        return SuccessJson("登陆成功");
                    }
                case CustomerLoginResults.CustomerNotExist:
                    return ErrorJson(_localizationService.GetResource("Account.Login.WrongCredentials.CustomerNotExist"));
                case CustomerLoginResults.Deleted:
                    return ErrorJson(_localizationService.GetResource("Account.Login.WrongCredentials.Deleted"));
                case CustomerLoginResults.NotActive:
                    return ErrorJson(_localizationService.GetResource("Account.Login.WrongCredentials.NotActive"));
                case CustomerLoginResults.WrongPassword:
                default:
                    return ErrorJson(_localizationService.GetResource("Account.Login.WrongCredentials"));
            }
        }

        //available even when a store is closed
        //[StoreClosed(true)]
        //available even when navigation is not allowed
        //[PublicStoreAllowNavigation(true)]
        public virtual ActionResult Logout()
        {
            //activity log
            _customerActivityService.InsertActivity("PublicStore.Logout", _localizationService.GetResource("ActivityLog.PublicStore.Logout"));

            //standard logout 
            _authenticationService.SignOut();

            return RedirectToRoute("login");
        }

        #endregion
    }
}