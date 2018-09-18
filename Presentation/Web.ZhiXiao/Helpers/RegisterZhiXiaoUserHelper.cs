using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Domain;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.ZhiXiao;
using Nop.Models.Customers;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Logging;
using Nop.Services.Security;
using Nop.Services.ZhiXiao;

namespace Nop.Admin.Helpers
{
    public class RegisterCustomerRequest
    {
        public RegisterCustomerRequest()
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
        public bool RegisterAdvanceUser { get; set; }
        /// <summary>
        /// 注册人是管理员?
        /// </summary>
        public bool IsManager { get; set; }

        /// <summary>
        /// 推荐人(当前用户或者选中的推荐人)
        /// </summary>
        //public Customer ParentUser { get; set; }
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

        /// <summary>
        /// Add error
        /// </summary>
        /// <param name="error">Error</param>
        public void AddError(string error)
        {
            this.Errors.Add(error);
        }

        /// <summary>
        /// Gets a value indicating whether request has been completed successfully
        /// </summary>
        public bool Success
        {
            get { return !this.Errors.Any(); }
        }
    }

    public interface IRegisterZhiXiaoUserHelper
    {
        RegisterCustomerRequest ValidateParentCustomer(Customer customer, bool isManager = false);
        CustomerRegistrationResult RegisterNewUser(CustomerModel model, Customer parentCustomer, bool isManager = false);
        void AddChildToCustomer(Customer customer, Customer parentCustomer, RegisterCustomerRequest registerRequest, bool isManager = false);
        void UpgradeCustomerToAdanced(Customer customer, Customer parentCustomer);
        void SaveCustomerAttriubteValues(Customer customer, CustomerModel model);
    }

    public class RegisterZhiXiaoUserHelper : IRegisterZhiXiaoUserHelper
    {
        #region Fields

        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ICustomerService _customerService;
        private readonly IZhiXiaoService _zhiXiaoService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICustomerRegistrationService _customerRegistrationService;

        private readonly IWebHelper _webHelper;

        private readonly ICustomerActivityService _customerActivityService;
        private readonly IPermissionService _permissionService;

        private readonly CustomerSettings _customerSettings;
        private readonly DateTimeSettings _dateTimeSettings;
        private readonly ZhiXiaoSettings _zhiXiaoSettings;
        private readonly LocalizationSettings _localizationSettings;
        private readonly StoreInformationSettings _storeInformationSettings;


        #endregion

        #region Ctor

        public RegisterZhiXiaoUserHelper(
            IDateTimeHelper dateTimeHelper,
            IWorkContext workContext,
            IStoreContext storeContext,
            ICustomerService customerService,
            IZhiXiaoService customerTeamService,
            IGenericAttributeService genericAttributeService,
            ICustomerRegistrationService customerRegistrationService,
            IWebHelper webHelper,
            ICustomerActivityService customerActivityService,
            IPermissionService permissionService,
            DateTimeSettings dateTimeSettings,
            CustomerSettings customerSettings,
            LocalizationSettings localizationSettings,
            ZhiXiaoSettings zhiXiaoSettings,
            StoreInformationSettings storeInformationSettings)
        {
            this._dateTimeHelper = dateTimeHelper;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._customerService = customerService;
            this._zhiXiaoService = customerTeamService;
            //this._customerAttributeParser = customerAttributeParser;
            //this._customerAttributeService = customerAttributeService;
            this._genericAttributeService = genericAttributeService;
            this._customerRegistrationService = customerRegistrationService;
            //this._taxService = taxService;
            this._webHelper = webHelper;
            this._customerActivityService = customerActivityService;
            this._permissionService = permissionService;

            this._dateTimeSettings = dateTimeSettings;
            this._customerSettings = customerSettings;
            this._zhiXiaoSettings = zhiXiaoSettings;
            this._localizationSettings = localizationSettings;
            //this._captchaSettings = captchaSettings;
            this._storeInformationSettings = storeInformationSettings;
        }

        #endregion

        #region Methods

        /// <summary>
        /// 验证新用户(CustomerModel)的是否重复
        /// </summary>
        /// <param name="model">Customer model</param>
        /// <returns>Errors</returns>
        protected List<string> ValidateCustomerRepeat(CustomerModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            List<string> errors = new List<string>();

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

            return errors;
        }

        /// <summary>
        /// 检查用户是否可以注册新用户
        /// </summary>
        /// <param name="customer">parent</param>
        /// <param name="isManager">当前操作是否是管理员, 管理员注册不需要扣钱</param>
        /// <returns></returns>
        public RegisterCustomerRequest ValidateParentCustomer(Customer customer, bool isManager = false)
        {
            RegisterCustomerRequest result = new RegisterCustomerRequest();

            if (customer == null)
            {
                result.AddError("推荐人不存在");
                return result;
            }

            if (!customer.Active)
            {
                result.AddError("推荐人被禁用");
                return result;
            }

            if (customer.Deleted)
            {
                result.AddError("推荐人被删除");
                return result;
            }

            // 如果是管理员注册用户, 需要填写推荐人
            // 如果是普通用户注册用户, 推荐人就是该用户
            if (isManager && !_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
            {
                result.AddError("用户没有添加推荐人权限");
                return result;
            }

            //CustomerRegisterStatus resultStatus;

            bool isAdvancedUser = customer.IsRegistered_Advanced();

            // 是否已经出盘
            if (customer.CustomerTeam == null)
            {
                if (isAdvancedUser)
                {
                    // team == null, 高级用户 => 出盘
                    //resultStatus = CustomerRegisterStatus.OutOfTeam;
                    result.AddError("已进入董事级别, 不能添加!");
                    return result;
                }
                else if (customer.IsRegistered())
                {
                    // team == null, 普通用户 => 出盘, 可以充值进入高级组
                    // resultStatus = CustomerRegisterStatus.OutOfTeam_Temp;
                    result.AddError("已不在小组中, 请联系管理员充值进入26800小组!");
                    return result;
                }
            }

            // 推荐人下线个数不能超过_zhiXiaoSettings.MaxChildCount
            int childCount = customer.GetAttribute<int>(SystemCustomerAttributeNames.ZhiXiao_ChildCount);
            if (childCount >= _zhiXiaoSettings.MaxChildCount)
            {
                result.AddError(string.Format("已达到{0}个下线， 不能添加!", _zhiXiaoSettings.MaxChildCount));
                return result;
                //resultStatus = CustomerRegisterStatus.ChildFull;    
            }

            // 添加用户所需钱
            if (!isManager)
            {
                int requiredMoney = isAdvancedUser ?
                    _zhiXiaoSettings.Register_Money_AdvancedUser : _zhiXiaoSettings.Register_Money_NormalUser;

                var userMoney = customer.GetMoneyNum();

                if (userMoney < requiredMoney)
                {
                    //resultStatus = CustomerRegisterStatus.MoneyNotEnough;
                    result.AddError("电子币不足" + requiredMoney + ", 不能注册会员, 请充值!");
                    return result;
                }

                result.RequiredMoney = requiredMoney;
                result.ParentUserMoney = userMoney;
            }

            result.RegisterAdvanceUser = isAdvancedUser;
            result.ParentChildCount = childCount;
            result.IsManager = isManager;
            return result;
        }

        /// <summary>
        /// Register new user
        /// </summary>
        /// <param name="model"></param>
        /// <param name="validateResult"></param>
        /// <returns>Errors, 仅返回生成密码时错误</returns>
        public CustomerRegistrationResult RegisterNewUser(CustomerModel model, Customer parentCustomer, bool isManager = false)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            var result = new CustomerRegistrationResult();
            // 1. validate parent customer whether can add child
            RegisterCustomerRequest registerRequest = ValidateParentCustomer(parentCustomer, isManager);

            if (!registerRequest.Success)
                return new CustomerRegistrationResult
                {
                    Errors = registerRequest.Errors
                };

            // 2. validate model
            var modelErrors = ValidateCustomerRepeat(model);
            if (modelErrors.Any())
            {
                return new CustomerRegistrationResult
                {
                    Errors = modelErrors
                };
            }

            // 3. add new user
            var customer = new Customer
            {
                CustomerGuid = Guid.NewGuid(),
                // 邮箱默认后缀为@yourStore.com
                Email = CommonHelper.IsValidEmail(model.Email) ? model.Email : ZhiXiaoConfig.AppendEmailToUsername(model.Username),
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

            // password
            if (!String.IsNullOrWhiteSpace(model.Password))
            {
                var changePassRequest = new ChangePasswordRequest(customer.Email, false, _customerSettings.DefaultPasswordFormat, model.Password);
                var changePassResult = _customerRegistrationService.ChangePassword(changePassRequest);
                if (!changePassResult.Success)
                {
                    foreach (var changePassError in changePassResult.Errors)
                        result.AddError(changePassError);
                }
            }
            
            // 4. save form fields
            SaveCustomerAttriubteValues(customer, model);

            // 添加用户为下线
            AddChildToCustomer(customer, parentCustomer, registerRequest, isManager);
            return result;
        }

        /// <summary>
        /// 添加用户为下线
        /// </summary>
        public void AddChildToCustomer(Customer customer, Customer parentCustomer, RegisterCustomerRequest registerRequest, bool isManager = false)
        {
            // 5. save customer's parnet id
            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZhiXiao_ParentId, parentCustomer.Id);

            // 6. Update Parent child count, 
            _genericAttributeService.SaveAttribute(parentCustomer, SystemCustomerAttributeNames.ZhiXiao_ChildCount, registerRequest.ParentChildCount + 1);

            if (!isManager)
            {
                // 扣钱
                //if (registerRequest.ParentUserMoney < registerRequest.RequiredMoney)
                //    throw new Exception("所需电子币不足");

                // add log
                _customerActivityService.InsertMoneyLog(parentCustomer,
                    SystemZhiXiaoLogTypes.RegisterNewUser,
                    -registerRequest.RequiredMoney,
                    "注册新用户: {0}, 扣除电子币 {1}",
                    customer.GetNickNameAndUserName(),
                    registerRequest.RequiredMoney);
            }
            else
            {
                //activity log
                _customerActivityService.InsertActivity(SystemZhiXiaoLogTypes.RegisterNewUser,
                    "注册新用户 {0}, 推荐人 {1}, 小组编号 {2}, 用户级别 {3}",
                    customer.GetNickNameAndUserName(),
                    parentCustomer.GetNickNameAndUserName(),
                    parentCustomer.CustomerTeam.CustomNumber,
                    parentCustomer.IsRegistered_Advanced() ?
                        _zhiXiaoSettings.Register_Money_AdvancedUser : _zhiXiaoSettings.Register_Money_NormalUser);
            }

            // 7. add roles
            var registeredRole = _customerService.GetCustomerRoleBySystemName(SystemCustomerRoleNames.Registered);
            if (registeredRole == null)
                throw new NopException("'Registered' role could not be loaded");

            customer.CustomerRoles.Add(registeredRole);

            if (registerRequest.RegisterAdvanceUser)
            {
                var registered_advancedRole = _customerService.GetCustomerRoleBySystemName(SystemCustomerRoleNames.Registered_Advanced);
                if (registered_advancedRole == null)
                    throw new NopException("'Registered_Advanced' role could not be loaded");
                customer.CustomerRoles.Add(registered_advancedRole);
            }

            // 8. update customer team
            var parentTeam = parentCustomer.CustomerTeam;

            customer.CustomerTeam = parentTeam;

            _customerService.UpdateCustomer(customer);
            // 9. save team related info
            var sortId = _zhiXiaoService.GetNewUserSortId(parentTeam);
            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZhiXiao_InTeamOrder, sortId);
            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZhiXiao_InTeamTime, DateTime.UtcNow);
            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZhiXiao_LevelId, (int)CustomerLevel.ZuYuan);

            // 3. 更新小组人数
            parentTeam.UserCount += 1;
            _zhiXiaoService.UpdateCustomerTeam(parentTeam);

            // 1. 给组长， 副组长分钱, 2. 如果人数满足, 重新分组
            _zhiXiaoService.AddNewUserToTeam(parentTeam, customer);
        }

        /// <summary>
        /// 升级普通用户为高级用户, 设置RegisterCustomerRequest需要的参数
        /// </summary>
        /// <param name="customer"></param>
        /// <param name="parentCustomer"></param>
        public void UpgradeCustomerToAdanced(Customer customer, Customer parentCustomer)
        {
            // 设置RegisterCustomerRequest需要的参数
            AddChildToCustomer(customer,
                parentCustomer, 
                new RegisterCustomerRequest() {
                    ParentChildCount = parentCustomer.GetAttribute<int>(SystemCustomerAttributeNames.ZhiXiao_ChildCount),
                    ParentUserMoney = parentCustomer.GetMoneyNum(),
                    RequiredMoney = _zhiXiaoSettings.Register_Money_AdvancedUser,
                    RegisterAdvanceUser = true,
                }, 
                true);
            
            _customerActivityService.InsertMoneyLog(customer, SystemZhiXiaoLogTypes.UpgradeUser,
                        "管理员分至小组{0}(26800级别)",
                        parentCustomer.CustomerTeam.CustomNumber);

            _customerActivityService.InsertMoneyLog(parentCustomer, SystemZhiXiaoLogTypes.UpgradeUser,
                        "管理员把用户{0}分到你的下线",
                        customer.GetNickName());

            _customerActivityService.InsertActivity(SystemZhiXiaoLogTypes.UpgradeUser,
                        "把用户 {0} 分至小组 {1} (26800级别)",
                        customer.GetNickNameAndUserName(),
                        parentCustomer.CustomerTeam.CustomNumber);
        }

        /// <summary>
        /// 保存用户的generic attribute 值
        /// </summary>
        /// <param name="customer"></param>
        /// <param name="model"></param>
        public void SaveCustomerAttriubteValues(Customer customer, CustomerModel model)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");
            if (model == null)
                throw new ArgumentNullException("model");

            //form fields
            if (_dateTimeSettings.AllowCustomersToSetTimeZone)
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.TimeZoneId, model.TimeZoneId);
            if (_customerSettings.GenderEnabled)
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Gender, model.Gender);
            //_genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.FirstName, model.FirstName);
            //_genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.LastName, model.LastName);
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
            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZhiXiao_BandNum, model.ZhiXiao_BankNum);      // 银行卡号
        }

        #endregion
    }
}