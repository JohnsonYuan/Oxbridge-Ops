using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Nop.Admin.Helpers;
using Nop.Admin.Models.Logging;
using Nop.Admin.Models.News;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.News;
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
using Nop.Web.Models.Home;
using Web.ZhiXiao.Factories;

namespace Web.ZhiXiao.Controllers
{
    public class CustomerController : BaseCustomerController
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
        private readonly NewsSettings _newsSettings;
        private readonly LocalizationSettings _localizationSettings;
        private readonly StoreInformationSettings _storeInformationSettings;

        private readonly INewsService _newsService;
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
            IZhiXiaoService customerTeamService,
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
            NewsSettings newsSettings,
            StoreInformationSettings storeInformationSettings,
            INewsService newsService,
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
            this._zhiXiaoService = customerTeamService;
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
            this._newsSettings = newsSettings;
            this._localizationSettings = localizationSettings;
            //this._captchaSettings = captchaSettings;
            this._storeInformationSettings = storeInformationSettings;

            this._newsService = newsService;
            this._permissionService = permissionService;

            this._registerZhiXiaoUserHelper = registerZhiXiaoUserHelper;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// 首页用户信息
        /// </summary>
        /// <param name="customer"></param>
        /// <returns></returns>
        [NonAction]
        protected virtual CustomerModel PrepareCustomerModelForIndex(Customer customer)
        {
            return new CustomerModel
            {
                Id = customer.Id,
                //Email = customer.IsRegistered() ? customer.Email : _localizationService.GetResource("Admin.Customers.Guest"),
                Username = customer.Username,
                //FullName = customer.GetFullName(),
                //Company = customer.GetAttribute<string>(SystemCustomerAttributeNames.Company),
                Phone = customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone),
                Active = customer.Active,
                CreatedOn = _dateTimeHelper.ConvertToUserTime(customer.CreatedOnUtc, DateTimeKind.Utc),
                LastActivityDate = _dateTimeHelper.ConvertToUserTime(customer.LastActivityDateUtc, DateTimeKind.Utc),
                NickName = customer.GetNickName(),
                ZhiXiao_MoneyNum = customer.GetMoneyNum(),
                ZhiXiao_MoneyHistory = customer.GetAttribute<int>(SystemCustomerAttributeNames.ZhiXiao_MoneyHistory),
                ZhiXiao_LevelId = customer.GetAttribute<int>(SystemCustomerAttributeNames.ZhiXiao_LevelId),
                //ProductStatusId = customer.GetAttribute<int>(SystemCustomerAttributeNames.ZhiXiao_SendProductStatus) // 发货状态
            };
        }

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

        /// <summary>
        /// 系谱图页面分为7个, 8个2组用户
        /// </summary>
        /// <param name="team"></param>
        /// <returns></returns>
        [NonAction]
        protected virtual TeamDiagramModel PrepareTeamDiagarmModel(CustomerTeam team)
        {
            if (team == null)
                throw new ArgumentNullException("team");

            // 默认为7
            var teamUnitCount = _zhiXiaoSettings.TeamInitUserCount;

            List<CustomerDiagramModel> diagarmModel = new List<CustomerDiagramModel>();
            foreach (var customer in team.Customers)
            {
                var model = customer.ToModel();

                var childs = _customerService.GetCustomerChildren(customer.Id);

                foreach (var child in childs)
                {
                    model.Child.Add(child.ToModel());
                }

                diagarmModel.Add(model);
            }

            var group1Users = diagarmModel.Where(x => x.InTeamOrder < teamUnitCount).OrderBy(x => x.CreatedOnUtc).ToList();
            var group2Users = diagarmModel.Where(x => x.InTeamOrder >= teamUnitCount).OrderBy(x => x.CreatedOnUtc).ToList();

            return new TeamDiagramModel
            {
                TopHalfUsers = group1Users,
                LastHalfUsers = group2Users,
                Team = team
            };
        }

        /// <summary>
        /// 显示tree, 返回所有小组用户
        /// </summary>
        /// <param name="team"></param>
        /// <returns></returns>
        [NonAction]
        protected virtual IList<CustomerDiagramModel> PrepareTeamDiagarmInfo(CustomerTeam team)
        {
            if (team == null)
                throw new ArgumentNullException("team");

            // 默认为7
            var teamUnitCount = _zhiXiaoSettings.TeamInitUserCount;

            List<CustomerDiagramModel> diagarmModel = new List<CustomerDiagramModel>();
            foreach (var customer in team.Customers)
            {
                var model = customer.ToModel();

                var childs = _customerService.GetCustomerChildren(customer.Id);

                foreach (var child in childs)
                {
                    model.Child.Add(child.ToModel());
                }

                diagarmModel.Add(model);
            }
            diagarmModel = diagarmModel.OrderBy(x => x.InTeamOrder).ThenBy(x => x.CreatedOnUtc).ToList();
            return diagarmModel;
        }

        #endregion

        #region Register

        [ParameterBasedOnQueryString("advanced", "registerAdvanceUser")]
        public virtual ActionResult Register(bool registerAdvanceUser)
        {
            _zhiXiaoService.ValidateCustomerRegister(_workContext.CurrentCustomer);

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

        /// <summary>
        /// 验证二级密码
        /// </summary>
        /// <returns></returns>
        //[UserPassword2AuthorizeAttribute]
        public ActionResult ValidatePassword2()
        {
            WarningNotification(string.Format("验证二级密码, {0}分钟内容不用重复验证", _zhiXiaoSettings.Password2_ValidTime));
            return View();
        }

        [HttpPost]
        public ActionResult ValidatePassword2(string pwd2, string returnUrl)
        {
            if (string.IsNullOrEmpty(pwd2))
            {
                ErrorNotification("二级密码不能为空");
                return View();
            }

            bool pwdValid = _zhiXiaoService.UserPassword2Valid(_workContext.CurrentCustomer, pwd2);

            if (pwdValid)
            {
                TempData[string.Format(ZhiXiaoConstants.Password2Key, _workContext.CurrentCustomer.Id)] = true;

                var cacheManager = EngineContext.Current.ContainerManager.Resolve<ICacheManager>("nop_cache_static");

                cacheManager.Set(string.Format(ZhiXiaoConstants.Password2Key, _workContext.CurrentCustomer.Id), true, _zhiXiaoSettings.Password2_ValidTime);

                if (String.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
                    return RedirectToRoute("HomePage");

                return Redirect(returnUrl);
            }

            ErrorNotification("二级密码错误");
            return View();
        }

        #endregion

        #region News

        /// <summary>
        /// Get customer news
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult NewsList(DataSourceRequest command)
        {
            if (command.PageSize <= 0) command.PageSize = _newsSettings.NewsArchivePageSize;
            if (command.Page <= 0) command.Page = 1;

            var newsItems = _newsService.GetAllNews(_workContext.WorkingLanguage.Id, _storeContext.CurrentStore.Id,
                command.Page - 1, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = newsItems.Select(x =>
                {
                    var m = x.ToModel();
                    //little performance optimization: ensure that "Full" is not returned
                    m.Full = "";
                    m.CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc);
                    //m.LanguageName = x.Language.Name;
                    //m.ApprovedComments = _newsService.GetNewsCommentsCount(x, isApproved: true);
                    //m.NotApprovedComments = _newsService.GetNewsCommentsCount(x, isApproved: false);

                    return m;
                }),
                Total = newsItems.TotalCount
            };

            return Json(gridModel);
        }

        public virtual ActionResult NewsItem(int newsItemId)
        {
            var newsItem = _newsService.GetNewsById(newsItemId);
            if (newsItem == null ||
                !newsItem.Published ||
                (newsItem.StartDateUtc.HasValue && newsItem.StartDateUtc.Value >= DateTime.UtcNow) ||
                (newsItem.EndDateUtc.HasValue && newsItem.EndDateUtc.Value <= DateTime.UtcNow))
                return new NullJsonResult();

            var model = new NewsItemModel
            {
                Id = newsItem.Id,
                MetaTitle = newsItem.MetaTitle,
                MetaDescription = newsItem.MetaDescription,
                MetaKeywords = newsItem.MetaKeywords,
                //SeName = newsItem.GetSeName(newsItem.LanguageId, ensureTwoPublishedLanguages: false),
                Title = newsItem.Title,
                Short = newsItem.Short,
                Full = newsItem.Full,
                AllowComments = newsItem.AllowComments,
                CreatedOn = _dateTimeHelper.ConvertToUserTime(newsItem.StartDateUtc ?? newsItem.CreatedOnUtc, DateTimeKind.Utc),
                //AddNewComment.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnNewsCommentPage,
            };

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region User info

        /// <summary>
        /// User home page
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            var currentCustomer = _workContext.CurrentCustomer;

            if (!currentCustomer.IsAdmin())
                return RedirectToRoute("AdminHomePage");

            CustomerIndexModel model = new CustomerIndexModel();
            model.CustomerInfo = PrepareCustomerModelForIndex(currentCustomer);
            model.Children = new List<CustomerDiagramModel>();
            var childs = _customerService.GetCustomerChildren(currentCustomer.Id);

            foreach (var child in childs)
            {
                model.Children.Add(child.ToModel());
            }

            var team = _workContext.CurrentCustomer.CustomerTeam;
            model.TeamUsers = PrepareTeamDiagarmInfo(team);

            return View(model);
        }

        /// <summary>
        /// 个人信息
        /// </summary>
        /// <returns></returns>
        public ActionResult Info()
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return new HttpUnauthorizedResult();

            var model = new CustomerInfoModel();
            model = _customerModelFactory.PrepareCustomerInfoModel(model, _workContext.CurrentCustomer, false);

            return View(model);
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Info(CustomerInfoModel model)
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return new HttpUnauthorizedResult();

            var customer = _workContext.CurrentCustomer;

            try
            {
                if (ModelState.IsValid)
                {
                    //username 
                    if (_customerSettings.UsernamesEnabled && this._customerSettings.AllowUsersToChangeUsernames)
                    {
                        if (!customer.Username.Equals(model.Username.Trim(), StringComparison.InvariantCultureIgnoreCase))
                        {
                            //change username
                            _customerRegistrationService.SetUsername(customer, model.Username.Trim());

                            //re-authenticate
                            //do not authenticate users in impersonation mode
                            if (_workContext.OriginalCustomerIfImpersonated == null)
                                _authenticationService.SignIn(customer, true);
                        }
                    }
                    //email
                    //if (!customer.Email.Equals(model.Email.Trim(), StringComparison.InvariantCultureIgnoreCase))
                    //{
                    //    //change email
                    //    var requireValidation = _customerSettings.UserRegistrationType ==
                    //                            UserRegistrationType.EmailValidation;
                    //    _customerRegistrationService.SetEmail(customer, model.Email.Trim(), requireValidation);

                    //    //do not authenticate users in impersonation mode
                    //    if (_workContext.OriginalCustomerIfImpersonated == null)
                    //    {
                    //        //re-authenticate (if usernames are disabled)
                    //        if (!_customerSettings.UsernamesEnabled && !requireValidation)
                    //            _authenticationService.SignIn(customer, true);
                    //    }
                    //}

                    //properties
                    if (_dateTimeSettings.AllowCustomersToSetTimeZone)
                    {
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.TimeZoneId,
                            model.TimeZoneId);
                    }
                    //VAT number
                    //if (_taxSettings.EuVatEnabled)
                    //{
                    //   // delete ... 
                    //}

                    //form fields
                    if (_customerSettings.GenderEnabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Gender,
                            model.Gender);
                    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.FirstName,
                        model.FirstName);
                    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.LastName,
                        model.LastName);
                    if (_customerSettings.DateOfBirthEnabled)
                    {
                        DateTime? dateOfBirth = model.ParseDateOfBirth();
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.DateOfBirth,
                            dateOfBirth);
                    }
                    if (_customerSettings.CompanyEnabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Company,
                            model.Company);
                    if (_customerSettings.StreetAddressEnabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.StreetAddress,
                            model.StreetAddress);
                    if (_customerSettings.StreetAddress2Enabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.StreetAddress2,
                            model.StreetAddress2);
                    if (_customerSettings.ZipPostalCodeEnabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZipPostalCode,
                            model.ZipPostalCode);
                    if (_customerSettings.CityEnabled)
                    {
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.City, model.City);
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.District, model.District);
                    }
                    //if (_customerSettings.CountryEnabled)
                    //    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.CountryId,
                    //        model.CountryId);
                    if (_customerSettings.CountryEnabled && _customerSettings.StateProvinceEnabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.StateProvince, model.StateProvince);

                    if (_customerSettings.PhoneEnabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Phone, model.Phone);
                    if (_customerSettings.FaxEnabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Fax, model.Fax);

                    //newsletter (deleted ...)

                    //if (_forumSettings.ForumsEnabled && _forumSettings.SignaturesEnabled)
                    //    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Signature,
                    //        model.Signature);

                    //save customer attributes
                    //_genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
                    //    SystemCustomerAttributeNames.CustomCustomerAttributes, customerAttributesXml);

                    // 直销用户个人信息 (只在编辑时更新, Create时不显示这些信息)
                    // nickname
                    if (!customer.GetNickName().Equals(model.NickName.Trim(), StringComparison.InvariantCultureIgnoreCase))
                    {
                        //change nickname
                        _customerRegistrationService.SetNickName(customer, model.NickName.Trim());
                    }

                    //_genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZhiXiao_Password2, model.Password2);          // 二级密码
                    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZhiXiao_IdCardNum, model.ZhiXiao_IdCardNum);
                    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZhiXiao_YinHang, model.ZhiXiao_YinHang);      // 银行
                    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZhiXiao_KaiHuHang, model.ZhiXiao_KaiHuHang);  // 开户行
                    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZhiXiao_KaiHuMing, model.ZhiXiao_KaiHuMing);  // 开户名
                    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZhiXiao_BandNum, model.ZhiXiao_BandNum);      // 银行卡号

                    SuccessNotification("更新信息成功");
                }
            }
            catch (Exception exc)
            {
                ErrorNotification(exc.Message);
                //ModelState.AddModelError("", exc.Message);
            }

            //If we got this far, something failed, redisplay form
            model = _customerModelFactory.PrepareCustomerInfoModel(model, customer, false);
            return View(model);
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <returns></returns>
        public ActionResult ChangePassword()
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return new HttpUnauthorizedResult();

            var model = _customerModelFactory.PrepareChangePasswordModel();

            //display the cause of the change password 
            if (_workContext.CurrentCustomer.PasswordIsExpired())
                ModelState.AddModelError(string.Empty, _localizationService.GetResource("Account.ChangePassword.PasswordIsExpired"));

            return View(model);
        }
        [HttpPost]
        public ActionResult ChangePassword(string pwdType, ChangePasswordModel model)
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return new HttpUnauthorizedResult();

            var customer = _workContext.CurrentCustomer;

            if (ModelState.IsValid)
            {
                ChangePasswordRequest changePasswordRequest;
                ChangePasswordResult changePasswordResult;
                if (pwdType == "pwd1")
                {
                    changePasswordRequest = new ChangePasswordRequest(customer.Email,
                        true, _customerSettings.DefaultPasswordFormat, model.NewPassword, model.OldPassword);
                    changePasswordResult = _customerRegistrationService.ChangePassword(changePasswordRequest);

                }
                else if (pwdType == "pwd2")
                {
                    changePasswordRequest = new ChangePasswordRequest(customer.Email,
                        true, _customerSettings.DefaultPasswordFormat, model.NewPassword, model.OldPassword);
                    changePasswordResult = _customerRegistrationService.ChangeZhiXiaoPassword(changePasswordRequest);
                }
                else
                {
                    changePasswordResult = new ChangePasswordResult();
                    changePasswordResult.AddError("修改密码失败");
                }

                if (changePasswordResult.Success)
                {
                    SuccessNotification(_localizationService.GetResource("Account.ChangePassword.Success"));
                    //model.Result = _localizationService.GetResource("Account.ChangePassword.Success");
                    return View(model);
                }

                //errors
                foreach (var error in changePasswordResult.Errors)
                    ErrorNotification(error);
                    //ModelState.AddModelError("", error);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        #endregion

        #region User functions

        /// <summary>
        /// 提现
        /// </summary>
        /// <returns></returns>
        [UserPassword2Authorize("Withdraw money")]
        public ActionResult Withdraw()
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return new HttpUnauthorizedResult();

            CustomerWithdrawModel model = new CustomerWithdrawModel();
            model.MaxAmount = _workContext.CurrentCustomer.GetMoneyNum();
            return View(model);
        }

        [HttpPost]
        [UserPassword2Authorize("Withdraw money")]
        public ActionResult Withdraw(CustomerWithdrawModel model)
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return new HttpUnauthorizedResult();

            if (ModelState.IsValid)
            {
                try
                {
                    var acutalAmount = _zhiXiaoService.WithdrawMoney(_workContext.CurrentCustomer, model.Amount);

                    SuccessNotification(string.Format("提现成功, 手续费{0}, 实际到账{1}, 资金不久会打入您的银行卡中， 请耐心等待！",
                        model.Amount - acutalAmount,
                        acutalAmount));
                }
                catch (Exception exc)
                {
                    ErrorNotification(exc.Message);
                }
            }
            
            model.MaxAmount = _workContext.CurrentCustomer.GetMoneyNum();
            return View(model);
        }

        /// <summary>
        /// 提现列表
        /// </summary>
        /// <returns></returns>
        [UserPassword2Authorize("Withdraw list")]
        public ActionResult WithdrawList()
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return new HttpUnauthorizedResult();

            var model = new WithdrawLogSearchModel();
            return View(model);
        }
        [HttpPost]
        [UserPassword2Authorize("Withdraw list")]
        public ActionResult WithdrawList(DataSourceRequest command,
            WithdrawLogSearchModel searchModel)
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return new HttpUnauthorizedResult();

            DateTime? startDateValue = (searchModel.CreatedOnFrom == null) ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.CreatedOnFrom.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (searchModel.CreatedOnTo == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.CreatedOnTo.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            var withdraws = _customerActivityService.GetAllWithdraws(startDateValue,
                endDateValue,
                _workContext.CurrentCustomer.Id,
                searchModel.IsDone,
                command.Page - 1,
                command.PageSize);

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
        /// 资金清单
        /// </summary>
        /// <returns></returns>
        [UserPassword2Authorize("MoneyLog list")]
        public ActionResult MoneyLogList()
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return new HttpUnauthorizedResult();

            return View();
        }
        [HttpPost]
        [UserPassword2Authorize("MoneyLog list")]
        public ActionResult MoneyLogList(DataSourceRequest command, FormCollection forms)
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return new HttpUnauthorizedResult();

            var activityLog = _customerActivityService.GetAllActivitiesByTypes(new string[]
            {
                SystemZhiXiaoLogTypes.AddNewUser,
                SystemZhiXiaoLogTypes.ReGroupTeam_AddMoney,
                SystemZhiXiaoLogTypes.ReGroupTeam_ReSort,
                SystemZhiXiaoLogTypes.ReGroupTeam_UpdateLevel,
                SystemZhiXiaoLogTypes.RechargeMoney,
                SystemZhiXiaoLogTypes.Withdraw,
                SystemZhiXiaoLogTypes.ProcessWithdraw,
            },
            _workContext.CurrentCustomer.Id,
            command.Page - 1,
            command.PageSize);

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
        /// 提货中心
        /// </summary>
        /// <returns></returns>
        [UserPassword2Authorize("Customer product")]
        public ActionResult ProductInfo()
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return new HttpUnauthorizedResult();

            var productInfo = _zhiXiaoService.GetSendProductInfo(_workContext.CurrentCustomer);

            return View(productInfo);
        }
        [HttpPost]
        [UserPassword2Authorize("Customer product")]
        public ActionResult ProductInfo(bool received)
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return new HttpUnauthorizedResult();

            try
            {
                if (received)
                {
                    if (_zhiXiaoService.GetSendProductStatus(_workContext.CurrentCustomer) != SendProductStatus.Sended)
                        throw new Exception("还没有发货, 不能收货!");
                    if (_zhiXiaoService.GetSendProductStatus(_workContext.CurrentCustomer) == SendProductStatus.Received)
                        throw new Exception("已经收货, 不能重复确认!");

                    _zhiXiaoService.SetSendProductStatus(_workContext.CurrentCustomer,
                        SendProductStatus.Received);

                    // add log
                    var log = _customerActivityService.InsertActivity(
                        SystemZhiXiaoLogTypes.ReceiveProduct,
                        "确认收货");

                    _zhiXiaoService.SaveReceiveProductLog(_workContext.CurrentCustomer, log);
                }
            }
            catch (Exception ex)
            {
                ErrorNotification(ex.Message);
            }

            return RedirectToAction("ProductInfo");
        }

        /// <summary>
        /// Get current user's team diagarm
        /// </summary>
        public ActionResult Diagarm()
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return new HttpUnauthorizedResult();

            var team = _workContext.CurrentCustomer.CustomerTeam;
            if (team == null)
                return RedirectToRoute("HomePage");

            var model = PrepareTeamDiagarmModel(team);

            ViewBag.ActiveMenuItemSystemName = "Customer teams diagarm";
            ViewBag.HideReturnTip = true;
            return View("TeamDiagarm", model);
        }

        public ActionResult DiagarmTreeTest()
        {
            return View();
        }

        #endregion

        #region test

        public ActionResult ViewDateTest()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ViewDateTest(string name, bool redirect)
        {
            ViewBag.TestValue = name;
            TempData["TestValue"] = name;
            ControllerContext.HttpContext.Items["test"] = "12313";
            if (redirect)
                return RedirectToAction("ViewDateTest2");
            return View();
        }


        public ActionResult ViewDateTest2()
        {
            return View();
        }
        #endregion
    }
}