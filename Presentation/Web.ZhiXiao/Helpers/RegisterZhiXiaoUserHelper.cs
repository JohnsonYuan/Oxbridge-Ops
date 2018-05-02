using System;
using System.Collections.Generic;
using System.Linq;
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
using Web.ZhiXiao.Factories;

namespace Nop.Admin.Helpers
{
    public class ValidateCustomerModelResult
    {
        public ValidateCustomerModelResult()
        {
            this.Errors = new List<string>();
        }

        /// <summary>
        /// 错误
        /// </summary>
        public IList<string> Errors { get; set; }

        /// <summary>
        /// 注册高级用户?
        /// </summary>
        public bool RegisterAdvanceUser{ get; set; }
        /// <summary>
        /// 注册人是管理员?
        /// </summary>
        public bool IsManager{ get; set; }

        /// <summary>
        /// 推荐人(当前用户或者选中的推荐人)
        /// </summary>
        public Customer ParentUser { get; set; }
        /// <summary>
        /// 推荐人下线个数<=2
        /// </summary>
        public int ParentChildCount { get; set; }
        /// <summary>
        /// 推荐人金币(ParentUser MoneyNum属性)
        /// </summary>
        public long ParentUserMoney { get; set; }
        /// <summary>
        /// 注册新用户需要的钱, 高级普通用户不同
        /// </summary>
        public int RequiredMoney { get; set; }
    }

    public interface IRegisterZhiXiaoUserHelper
    {
        ValidateCustomerModelResult ValidateCustomerModel(CustomerModel model, bool registerAdvanceUser, bool isManager);
        IList<string> RegisterNewUser(CustomerModel model, ValidateCustomerModelResult validateResult);
    }

    public class RegisterZhiXiaoUserHelper : IRegisterZhiXiaoUserHelper
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

        public RegisterZhiXiaoUserHelper(
            ICustomerModelFactory customerModelFactory,
            IAuthenticationService authenticationService,
            IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService,
            DateTimeSettings dateTimeSettings,
            IWorkContext workContext,
            IStoreContext storeContext,
            ICustomerService customerService,
            ICustomerTeamService customerTeamService,
            IGenericAttributeService genericAttributeService,
            ICustomerRegistrationService customerRegistrationService,
            CustomerSettings customerSettings,
            IWebHelper webHelper,
            ICustomerActivityService customerActivityService,
            IStoreService storeService,
            LocalizationSettings localizationSettings,
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

        #region Methods

        /// <summary>
        /// 验证CustomerModel
        /// </summary>
        /// <param name="model">Customer model</param>
        /// <param name="registerAdvanceUser">注册高级用户</param>
        /// <param name="isManager">管理员模式添加, 可以指定推荐人</param>
        /// <returns>Errors</returns>
        public ValidateCustomerModelResult ValidateCustomerModel(CustomerModel model, bool registerAdvanceUser, bool isManager)
        {
            IList<string> errors = new List<string>();

            //if (!String.IsNullOrWhiteSpace(model.Email))
            //{
            //    var cust2 = _customerService.GetCustomerByEmail(model.Email);
            //    if (cust2 != null)
            //        errors.Add("Email is already registered");
            //}

            if (!String.IsNullOrWhiteSpace(model.Username) & _customerSettings.UsernamesEnabled)
            {
                var cust2 = _customerService.GetCustomerByUsername(model.Username);
                if (cust2 != null)
                    errors.Add("用户名已经使用");
            }

            {
                var cust2 = _customerService.GetCustomerByNickName(model.NickName);
                if (cust2 != null)
                    errors.Add("昵称已经使用");
            }

            if (!String.IsNullOrWhiteSpace(model.Phone) && _customerSettings.PhoneEnabled)
            {
                var cust2 = _customerService.GetCustomerByPhoneNumber(model.Username);
                if (cust2 != null)
                    errors.Add("手机号已经使用");
            }

            // 检查推荐人
            // 如果是管理员注册用户, 需要填写推荐人
            // 如果饰普通用户注册用户, 推荐人就是该用户
            if (isManager && !_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                errors.Add("用户没有添加推荐人权限");

            Customer parentUser;
            if (isManager)
            {
                parentUser = _customerService.GetCustomerById(model.ZhiXiao_ParentId);
                if (parentUser == null)
                    errors.Add("推荐人不存在, 请重新输入!");
            }
            else
            {
                parentUser = _workContext.CurrentCustomer;
            }

            // 推荐人下线个数不能超过_zhiXiaoSettings.MaxChildCount
            int parentChildCount = parentUser.GetAttribute<int>(SystemCustomerAttributeNames.ZhiXiao_ChildCount);
            if (parentChildCount >= _zhiXiaoSettings.MaxChildCount)
                errors.Add(string.Format("该推荐人已达到{0}个下线， 不能添加!", _zhiXiaoSettings.MaxChildCount));

            // 添加用户所需钱
            int requiredMoney = _zhiXiaoSettings.Register_Money_NormalUser;
            if (registerAdvanceUser)
            {
                requiredMoney = _zhiXiaoSettings.Register_Money_AdvancedUser;
            }

            var parentUserMoney = parentUser.GetAttribute<long>(SystemCustomerAttributeNames.ZhiXiao_MoneyNum);

            if (!isManager)
            {
                if (parentUserMoney < requiredMoney)
                {
                    errors.Add("电子币不足" + requiredMoney + ", 不能注册会员, 请充值!");
                }
            }

            //custom customer attributes
            //var customerAttributesXml = ParseCustomCustomerAttributes(form);
            //if (newCustomerRoles.Any() && newCustomerRoles.FirstOrDefault(c => c.SystemName == SystemCustomerRoleNames.Registered) != null)
            //{
            //    var customerAttributeWarnings = _customerAttributeParser.GetAttributeWarnings(customerAttributesXml);
            //    foreach (var error in customerAttributeWarnings)
            //    {
            //        errors.Add(error);
            //    }
            //}
            return new ValidateCustomerModelResult
            {
                Errors = errors,
                ParentUser = parentUser,
                ParentChildCount = parentChildCount,
                ParentUserMoney = parentUserMoney,
                RequiredMoney = requiredMoney,
                IsManager = isManager,
                RegisterAdvanceUser = registerAdvanceUser
            };
        }

        /// <summary>
        /// Register new user
        /// </summary>
        /// <param name="model"></param>
        /// <param name="validateResult"></param>
        /// <returns>Errors, 仅返回生成密码时错误</returns>
        public IList<string> RegisterNewUser(CustomerModel model, ValidateCustomerModelResult validateResult)
        {
            if (validateResult == null)
                throw new ArgumentNullException("validateResult");

            if (validateResult.Errors.Count > 0)
                throw new ArgumentException("validateResult");

            validateResult.Errors.Clear();

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

            if (!validateResult.IsManager)
            {
                // 扣钱
                if (validateResult.ParentUserMoney < validateResult.RequiredMoney)
                    throw new Exception("所需电子币不足");
                _genericAttributeService.SaveAttribute(validateResult.ParentUser, SystemCustomerAttributeNames.ZhiXiao_MoneyNum, validateResult.ParentUserMoney - validateResult.RequiredMoney);
                _customerActivityService.InsertActivity(validateResult.ParentUser, SystemZhiXiaoLogTypes.AddNewUser,
                    "注册会员{0}, 电子币-{1}",
                    customer.GetNickName(),
                    validateResult.RequiredMoney);
            }
            else
            {
                _customerActivityService.InsertActivity(validateResult.ParentUser, SystemZhiXiaoLogTypes.AddNewUser,
                    "管理员{0}注册会员{1}, 推荐人为{2}",
                    _workContext.CurrentCustomer.GetNickName(), // 当前用户是管理员
                    customer.GetNickName(),
                    validateResult.ParentUser.GetNickName());
            }

            var zhiXiaoRoles = _customerService.GetAllCustomerRoles(true)
                                 .Where(cr => cr.SystemName == SystemCustomerRoleNames.Registered
                                 || cr.SystemName == SystemCustomerRoleNames.Registered_Advanced);

            var newCustomerRoles = new List<CustomerRole>();

            // user roles
            newCustomerRoles.Add(zhiXiaoRoles.Where(cr => cr.SystemName == SystemCustomerRoleNames.Registered).First());
            if (validateResult.RegisterAdvanceUser)
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
                        validateResult.Errors.Add(changePassError);
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
            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZhiXiao_ParentId, validateResult.ParentUser.Id);

            // var teamId = parentUser.GetAttribute<int>(SystemCustomerAttributeNames.ZhiXiao_TeamId);
            var parentTeam = validateResult.ParentUser.CustomerTeam;

            customer.CustomerTeam = parentTeam;
            // _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZhiXiao_TeamId, teamId);

            var sortId = _customerTeamService.GetNewUserSortId(parentTeam);
            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZhiXiao_InTeamOrder, sortId);
            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZhiXiao_InTeamTime, DateTime.UtcNow);
            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZhiXiao_LevelId, (int)CustomerLevel.ZuYuan);

            // 2. Update Parent Info
            _genericAttributeService.SaveAttribute(validateResult.ParentUser, SystemCustomerAttributeNames.ZhiXiao_ChildCount, validateResult.ParentChildCount + 1);

            // 3. 更新小组人数
            parentTeam.UserCount += 1;
            _customerTeamService.UpdateCustomerTeam(parentTeam);

            // 1. 给组长， 副组长分钱, 2. 如果人数满足, 重新分组
            _customerTeamService.AddNewUserToTeam(parentTeam, customer);

            //activity log
            _customerActivityService.InsertActivity("AddNewCustomer", _localizationService.GetResource("ActivityLog.AddNewCustomer"), customer.Id);

            return validateResult.Errors;
        }

        #endregion
    }
}