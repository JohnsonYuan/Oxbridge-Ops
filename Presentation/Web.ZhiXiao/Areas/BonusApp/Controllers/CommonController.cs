using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Domain;
using Nop.Core.Domain.BonusApp;
using Nop.Core.Domain.Customers;
using Nop.Services.BonusApp.Authentication;
using Nop.Services.BonusApp.Customers;
using Nop.Services.BonusApp.Logging;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Security;

namespace Web.ZhiXiao.Areas.BonusApp.Controllers
{
    public class CommonController : BonusAppBaseController
    {
        #region Fields

        private readonly IWebHelper _webHelper;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILogger _logger;
        private readonly IWorkContext _workContext;
        private readonly IEncryptionService _encryptionService;
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
            IEncryptionService encryptionService,
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
            this._encryptionService = encryptionService;
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
        public virtual ActionResult Login(Nop.Models.Customers.LoginModel model, string returnUrl)
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
                        _customerActivityService.InsertActivity(customer, BonusAppConstants.LogType_User_Login, _localizationService.GetResource("ActivityLog.PublicStore.Login"));

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
            _customerActivityService.InsertActivity(BonusAppConstants.LogType_User_Logout, _localizationService.GetResource("ActivityLog.PublicStore.Logout"));

            //standard logout 
            _authenticationService.SignOut();

            return RedirectToRoute("login");
        }

        #endregion

        #region Register

        public ActionResult Register()
        {
            return View();
        }
        /// <summary>
        /// 注册时候的username password
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Register([Bind(Include = "username, password, nickname, phone")]BonusApp.Models.RegisterModel model)
        {
            if (string.IsNullOrEmpty(model.Username))
                return ErrorJson("请输入用户名");
            if (string.IsNullOrEmpty(model.Nickname))
                return ErrorJson("请输入姓名");
            if (string.IsNullOrEmpty(model.Phone))
                return ErrorJson("请输入手机号");
            if (!Regex.IsMatch(model.Phone, "^1[34578]\\d{9}$"))
                return ErrorJson("手机号格式不正确");
            if (string.IsNullOrEmpty(model.Password))
                return ErrorJson("请输入密码");

            var result = new CustomerRegistrationResult();
            // validate model
            var modelError = _customerService.Register(model.Username, model.Password, model.Nickname, model.Phone);
            if (!string.IsNullOrEmpty(modelError))
            {
                return ErrorJson(modelError);
            }
            
            //standard logout 
            _authenticationService.SignOut();

            var customer = _customerService.GetCustomerByUsername(model.Username);

            //sign in new customer
            _authenticationService.SignIn(customer, createPersistentCookie: false);

            //activity log
            _customerActivityService.InsertActivity(customer, BonusAppConstants.LogType_User_Login, _localizationService.GetResource("ActivityLog.PublicStore.Login"));

            return SuccessJson("注册成功");
        }

        #endregion
    }
}