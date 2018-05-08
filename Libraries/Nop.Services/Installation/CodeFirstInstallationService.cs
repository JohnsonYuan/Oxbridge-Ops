using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.News;
using Nop.Core.Domain.Seo;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Vendors;
using Nop.Core.Domain.ZhiXiao;
using Nop.Core.Infrastructure;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.ZhiXiao;

namespace Nop.Services.Installation
{
    public partial class CodeFirstInstallationService : IInstallationService
    {
        #region Fields

        private readonly IRepository<CustomerTeam> _customerTeamRepository;
        private readonly IRepository<Store> _storeRepository;
        private readonly IRepository<Language> _languageRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<CustomerPassword> _customerPasswordRepository;
        private readonly IRepository<CustomerRole> _customerRoleRepository;
        private readonly IRepository<UrlRecord> _urlRecordRepository;
        private readonly IRepository<ActivityLogType> _activityLogTypeRepository;
        private readonly IRepository<ActivityLog> _activityLogRepository; private readonly IRepository<Address> _addressRepository;
        private readonly IRepository<Vendor> _vendorRepository;
        private readonly IRepository<SearchTerm> _searchTermRepository;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IWebHelper _webHelper;

        #endregion

        #region Ctor

        public CodeFirstInstallationService(
            IRepository<CustomerTeam> customerTeamRepository,
            IRepository<Store> storeRepository,
            IRepository<Language> languageRepository,
            IRepository<Customer> customerRepository,
            IRepository<CustomerPassword> customerPasswordRepository,
            IRepository<CustomerRole> customerRoleRepository,
            IRepository<UrlRecord> urlRecordRepository,
            IRepository<ActivityLogType> activityLogTypeRepository,
            IRepository<ActivityLog> activityLogRepository,
            IRepository<Address> addressRepository,
            IRepository<Vendor> vendorRepository,
            IRepository<SearchTerm> searchTermRepository,
            IGenericAttributeService genericAttributeService,
            IWebHelper webHelper)
        {
            this._customerTeamRepository = customerTeamRepository;
            this._storeRepository = storeRepository;
            this._languageRepository = languageRepository;
            this._customerRepository = customerRepository;
            this._customerPasswordRepository = customerPasswordRepository;
            this._customerRoleRepository = customerRoleRepository;
            this._urlRecordRepository = urlRecordRepository;
            this._activityLogTypeRepository = activityLogTypeRepository;
            this._activityLogRepository = activityLogRepository;
            this._addressRepository = addressRepository;
            this._vendorRepository = vendorRepository;
            this._searchTermRepository = searchTermRepository;
            this._genericAttributeService = genericAttributeService;
            this._webHelper = webHelper;
        }

        #endregion

        #region Utilities

        protected virtual void InstallStores()
        {
            //var storeUrl = "http://www.yourStore.com/";
            var storeUrl = _webHelper.GetStoreLocation(false);
            var stores = new List<Store>
            {
                new Store
                {
                    Name = "七上八下直销系统",
                    Url = storeUrl,
                    SslEnabled = false,
                    Hosts = "yourstore.com,www.yourstore.com",
                    DisplayOrder = 1,
                    //should we set some default company info?
                    CompanyName = "伊佳宜",
                    CompanyAddress = "your company country, state, zip, street, etc",
                    CompanyPhoneNumber = "(123) 456-78901",
                    CompanyVat = null,
                },
            };

            _storeRepository.Insert(stores);
        }

        protected virtual void InstallLanguages()
        {
            var language = new Language
            {
                Name = "English",
                LanguageCulture = "en-US",
                UniqueSeoCode = "en",
                FlagImageFileName = "us.png",
                Published = true,
                DisplayOrder = 2
            };

            var language2 = new Language
            {
                Name = "中文",
                LanguageCulture = "zh-CN",
                UniqueSeoCode = "cn",
                FlagImageFileName = "cn.png",
                Published = true,
                DisplayOrder = 1
            };
            _languageRepository.Insert(language);
            _languageRepository.Insert(language2);
        }

        protected virtual void InstallLocaleResources()
        {
            var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
            //'Chinese' language
            var language = _languageRepository.Table.Single(l => l.Name == "中文");
            //save resources
            var localesXml = File.ReadAllText(CommonHelper.MapPath("~/App_Data/Localization/zhs.nopres.xml"));
            localizationService.ImportResourcesFromXml(language, localesXml);

            //'English' language
            language = _languageRepository.Table.Single(l => l.Name == "English");

            localesXml = File.ReadAllText(CommonHelper.MapPath("~/App_Data/Localization/defaultResources.nopres.xml"));
            localizationService.ImportResourcesFromXml(language, localesXml);
        }

        protected virtual void InstallCustomerTeams()
        {
            var teams = new List<CustomerTeam>
            {
                new CustomerTeam
                {
                    UserCount = 0,
                    CreatedOnUtc = DateTime.UtcNow,
                    TypeId = (int)CustomerTeamType.Normal
                },
                new CustomerTeam
                {
                    UserCount = 0,
                    CreatedOnUtc = DateTime.UtcNow.AddDays(-5),
                    TypeId = (int)CustomerTeamType.Advanced
                }
            };

            _customerTeamRepository.Insert(teams);

            var customNumberFormatter = EngineContext.Current.Resolve<ICustomNumberFormatter>();

            //generate and set custom team number
            foreach (var team in teams)
            {
                team.CustomNumber = customNumberFormatter.GenerateTeamNumber(team);
                _customerTeamRepository.Update(team);
            }
        }

        protected virtual void InstallCustomersAndUsers(string defaultUserEmail, string defaultUserPassword)
        {
            var crAdministrators = new CustomerRole
            {
                Name = "系统管理员",
                Active = true,
                IsSystemRole = true,
                SystemName = SystemCustomerRoleNames.Administrators,
            };
            var crManagers = new CustomerRole
            {
                Name = "管理员",
                Active = true,
                IsSystemRole = true,
                SystemName = SystemCustomerRoleNames.Managers,
            };
            var crRegistered_Normal = new CustomerRole
            {
                Name = "普通用户",
                Active = true,
                IsSystemRole = true,
                SystemName = SystemCustomerRoleNames.Registered,
            };
            var crRegistered_Advanced = new CustomerRole
            {
                Name = "高级用户",
                Active = true,
                IsSystemRole = true,
                SystemName = SystemCustomerRoleNames.Registered_Advanced,
            };
            var crGuests = new CustomerRole
            {
                Name = "匿名用户(未注册用户)",
                Active = true,
                IsSystemRole = true,
                SystemName = SystemCustomerRoleNames.Guests,
            };

            var customerRoles = new List<CustomerRole>
                                {
                                    crAdministrators,
                                    //crForumModerators,
                                    crRegistered_Normal,
                                    crRegistered_Advanced,
                                    //crGuests,
                                    //crVendors
                                };
            _customerRoleRepository.Insert(customerRoles);

            //default store 
            var defaultStore = _storeRepository.Table.FirstOrDefault();

            if (defaultStore == null)
                throw new Exception("No default store could be loaded");

            var storeId = defaultStore.Id;

            //default team
            var defaultTeam = _customerTeamRepository.Table.FirstOrDefault();

            if (defaultTeam == null)
                throw new Exception("No default team could be loaded");

            var teamId = defaultTeam.Id;

            //admin user
            var adminUser = new Customer
            {
                CustomerGuid = Guid.NewGuid(),
                Email = defaultUserEmail,
                Username = defaultUserEmail,
                Active = true,
                CreatedOnUtc = DateTime.UtcNow,
                LastActivityDateUtc = DateTime.UtcNow,
                RegisteredInStoreId = storeId
            };

            var defaultAdminUserAddress = new Address
            {
                FirstName = "John",
                LastName = "Smith",
                PhoneNumber = "12345678",
                Email = defaultUserEmail,
                FaxNumber = "",
                Company = "Nop Solutions Ltd",
                Address1 = "21 West 52nd Street",
                Address2 = "",
                City = "New York",
                ZipPostalCode = "10021",
                CreatedOnUtc = DateTime.UtcNow,
            };
            adminUser.Addresses.Add(defaultAdminUserAddress);

            adminUser.CustomerRoles.Add(crAdministrators);
            adminUser.CustomerRoles.Add(crManagers);
            adminUser.CustomerRoles.Add(crRegistered_Normal);
            adminUser.CustomerRoles.Add(crRegistered_Advanced);

            _customerRepository.Insert(adminUser);

            //set hashed admin password
            var customerRegistrationService = EngineContext.Current.Resolve<ICustomerRegistrationService>();
            customerRegistrationService.ChangePassword(new ChangePasswordRequest(defaultUserEmail, false,
                 PasswordFormat.Hashed, defaultUserPassword));

            //second user
            var secondUserEmail = "xiaoyuan@yourStore.com";
            var secondUser = new Customer
            {
                CustomerGuid = Guid.NewGuid(),
                Email = secondUserEmail,
                Username = secondUserEmail,
                Active = true,
                CreatedOnUtc = DateTime.UtcNow,
                LastActivityDateUtc = DateTime.UtcNow,
                RegisteredInStoreId = storeId
            };
            var defaultSecondUserAddress = new Address
            {
                FirstName = "Steve",
                LastName = "Gates",
                PhoneNumber = "87654321",
                Email = secondUserEmail,
                FaxNumber = "",
                Company = "Steve Company",
                Address1 = "750 Bel Air Rd.",
                Address2 = "",
                City = "Los Angeles",
                ZipPostalCode = "90077",
                CreatedOnUtc = DateTime.UtcNow,
            };
            secondUser.Addresses.Add(defaultSecondUserAddress);
            secondUser.BillingAddress = defaultSecondUserAddress;
            secondUser.ShippingAddress = defaultSecondUserAddress;

            secondUser.CustomerRoles.Add(crManagers);
            secondUser.CustomerRoles.Add(crRegistered_Normal);

            _customerRepository.Insert(secondUser);

            //set customer password
            _customerPasswordRepository.Insert(new CustomerPassword
            {
                Customer = secondUser,
                Password = "123456",
                PasswordFormat = PasswordFormat.Clear,
                PasswordSalt = string.Empty,
                CreatedOnUtc = DateTime.UtcNow
            });

            //search engine (crawler) built-in user
            var searchEngineUser = new Customer
            {
                Email = "builtin@search_engine_record.com",
                CustomerGuid = Guid.NewGuid(),
                AdminComment = "Built-in system guest record used for requests from search engines.",
                Active = true,
                IsSystemAccount = true,
                SystemName = SystemCustomerNames.SearchEngine,
                CreatedOnUtc = DateTime.UtcNow,
                LastActivityDateUtc = DateTime.UtcNow,
                RegisteredInStoreId = storeId
            };
            searchEngineUser.CustomerRoles.Add(crGuests);
            _customerRepository.Insert(searchEngineUser);

            //built-in user for background tasks
            var backgroundTaskUser = new Customer
            {
                Email = "builtin@background-task-record.com",
                CustomerGuid = Guid.NewGuid(),
                AdminComment = "Built-in system record used for background tasks.",
                Active = true,
                IsSystemAccount = true,
                SystemName = SystemCustomerNames.BackgroundTask,
                CreatedOnUtc = DateTime.UtcNow,
                LastActivityDateUtc = DateTime.UtcNow,
                RegisteredInStoreId = storeId
            };
            backgroundTaskUser.CustomerRoles.Add(crGuests);
            _customerRepository.Insert(backgroundTaskUser);
        }

        /// <summary>
        /// 插入普通用户
        /// </summary>
        protected virtual void InstallZhiXiaoTestUser_Normal()
        {
            //default store 
            var defaultStore = _storeRepository.Table.FirstOrDefault();

            if (defaultStore == null)
                throw new Exception("No default store could be loaded");

            var storeId = defaultStore.Id;

            //default team
            var defaultTeam = _customerTeamRepository.Table.First(x => x.TypeId == (int)CustomerTeamType.Normal);

            if (defaultTeam == null)
                throw new Exception("No default team could be loaded");

            var teamId = defaultTeam.Id;

            var crRegistered_Normal = _customerRoleRepository.Table.Where(x => x.SystemName == SystemCustomerRoleNames.Registered).FirstOrDefault();
            var crRegistered_Advanced = _customerRoleRepository.Table.Where(x => x.SystemName == SystemCustomerRoleNames.Registered_Advanced).FirstOrDefault();

            var addToTeamTime = DateTime.UtcNow;

            #region Registered user
            
            // 组长(*1) 副组长(*2) 组员 (*8)
            for (int i = 1; i <= 11; i++)
            {
                var currentUserName = "user_" + i;
                var currentUserEmail = currentUserName + "@yourStore.com";
                int currentLevel = 0;
                if (i == 1)
                {
                    currentLevel = (int)CustomerLevel.ZuZhang;
                }
                else if( i <= 3)
                {
                    currentLevel = (int)CustomerLevel.FuZuZhang;
                }
                else
                {
                    currentLevel = (int)CustomerLevel.ZuYuan;
                }

                var currentUser = new Customer
                {
                    CustomerGuid = Guid.NewGuid(),
                    Email = currentUserEmail,
                    Username = currentUserName,
                    Active = true,
                    CreatedOnUtc = DateTime.UtcNow,
                    LastActivityDateUtc = DateTime.UtcNow,
                    RegisteredInStoreId = storeId
                };
                var currentUserAddress = new Address
                {
                    FirstName = "James",
                    LastName = "Pan",
                    PhoneNumber = "369258147",
                    Email = currentUserEmail,
                    FaxNumber = "",
                    Company = "Pan Company",
                    Address1 = "St Katharine抯 West 16",
                    Address2 = "",
                    City = "St Andrews",
                    ZipPostalCode = "KY16 9AX",
                    CreatedOnUtc = DateTime.UtcNow,
                };
                currentUser.Addresses.Add(currentUserAddress);
                currentUser.BillingAddress = currentUserAddress;
                currentUser.ShippingAddress = currentUserAddress;

                currentUser.CustomerRoles.Add(crRegistered_Normal);

                _customerRepository.Insert(currentUser);

                // 更新用户team
                currentUser.CustomerTeam = defaultTeam;
                _customerRepository.Update(currentUser);

                //set default customer name
                _genericAttributeService.SaveAttribute(currentUser, SystemCustomerAttributeNames.ZhiXiao_NickName, "测试用户" + i);
                _genericAttributeService.SaveAttribute(currentUser, SystemCustomerAttributeNames.ZhiXiao_InTeamOrder, i);
                _genericAttributeService.SaveAttribute(currentUser, SystemCustomerAttributeNames.ZhiXiao_InTeamTime, addToTeamTime.AddMinutes(i + 5));
                _genericAttributeService.SaveAttribute(currentUser, SystemCustomerAttributeNames.ZhiXiao_LevelId, currentLevel);
                _genericAttributeService.SaveAttribute(currentUser, SystemCustomerAttributeNames.ZhiXiao_MoneyNum, CommonHelper.GenerateRandomInteger(2000, 50000));
                _genericAttributeService.SaveAttribute(currentUser, SystemCustomerAttributeNames.ZhiXiao_MoneyHistory, CommonHelper.GenerateRandomInteger(5000, 50000));
                _genericAttributeService.SaveAttribute(currentUser, SystemCustomerAttributeNames.ZhiXiao_IdCardNum, CommonHelper.GenerateRandomDigitCode(18));
                _genericAttributeService.SaveAttribute(currentUser, SystemCustomerAttributeNames.ZhiXiao_YinHang, "中国银行");
                _genericAttributeService.SaveAttribute(currentUser, SystemCustomerAttributeNames.ZhiXiao_KaiHuHang, "太原支行");
                _genericAttributeService.SaveAttribute(currentUser, SystemCustomerAttributeNames.ZhiXiao_KaiHuMing, currentUserName);
                _genericAttributeService.SaveAttribute(currentUser, SystemCustomerAttributeNames.ZhiXiao_BandNum, CommonHelper.GenerateRandomDigitCode(25));
                _genericAttributeService.SaveAttribute(currentUser, SystemCustomerAttributeNames.ZhiXiao_Password2, "123456");  // 二级密码

                //set customer password
                _customerPasswordRepository.Insert(new CustomerPassword
                {
                    Customer = currentUser,
                    Password = "123456",
                    PasswordFormat = PasswordFormat.Clear,
                    PasswordSalt = string.Empty,
                    CreatedOnUtc = DateTime.UtcNow
                });
            }

            defaultTeam.UserCount = 11;
            _customerTeamRepository.Update(defaultTeam);

            // 设置下线
            var firstUser = _customerRepository.Table.Where(x => x.Username == "user_1").First();
            var user_2 = _customerRepository.Table.Where(x => x.Username == "user_2").First();
            var user_3 = _customerRepository.Table.Where(x => x.Username == "user_3").First();

            var user_8 = _customerRepository.Table.Where(x => x.Username == "user_8").First();
            var user_9 = _customerRepository.Table.Where(x => x.Username == "user_9").First();
            var user_10 = _customerRepository.Table.Where(x => x.Username == "user_10").First();
            var user_11 = _customerRepository.Table.Where(x => x.Username == "user_11").First();

            // 8, 9 用户parentId为第一个用户
            _genericAttributeService.SaveAttribute(firstUser, SystemCustomerAttributeNames.ZhiXiao_ChildCount, 2);
            _genericAttributeService.SaveAttribute(user_8, SystemCustomerAttributeNames.ZhiXiao_ParentId, firstUser.Id);
            _genericAttributeService.SaveAttribute(user_9, SystemCustomerAttributeNames.ZhiXiao_ParentId, firstUser.Id);
            
            // 11 用户parentId为第2个用户
            _genericAttributeService.SaveAttribute(user_2, SystemCustomerAttributeNames.ZhiXiao_ChildCount, 1);
            _genericAttributeService.SaveAttribute(user_11, SystemCustomerAttributeNames.ZhiXiao_ParentId, user_2.Id);

            // 10 用户parentId为第3个用户
            _genericAttributeService.SaveAttribute(user_3, SystemCustomerAttributeNames.ZhiXiao_ChildCount, 1);
            _genericAttributeService.SaveAttribute(user_10, SystemCustomerAttributeNames.ZhiXiao_ParentId, user_3.Id);

            #endregion
        }

        /// <summary>
        /// 插入高级用户
        /// </summary>
        protected virtual void InstallZhiXiaoTestUser_Advanced()
        {
            //default store 
            var defaultStore = _storeRepository.Table.FirstOrDefault();

            if (defaultStore == null)
                throw new Exception("No default store could be loaded");

            var storeId = defaultStore.Id;

            //default team
            var defaultTeam = _customerTeamRepository.Table.First(x => x.TypeId == (int)CustomerTeamType.Advanced);

            if (defaultTeam == null)
                throw new Exception("No default team could be loaded");

            var teamId = defaultTeam.Id;

            var crRegistered_Normal = _customerRoleRepository.Table.Where(x => x.SystemName == SystemCustomerRoleNames.Registered).FirstOrDefault();
            var crRegistered_Advanced = _customerRoleRepository.Table.Where(x => x.SystemName == SystemCustomerRoleNames.Registered_Advanced).FirstOrDefault();

            var addToTeamTime = DateTime.UtcNow;

            #region Registered user
            
            // 组长(*1) 副组长(*2) 组员 (*4)
            for (int i = 1; i <= 7; i++)
            {
                var currentUserName = "USER_" + i;
                var currentUserEmail = currentUserName + "@yourStore.com";
                int currentLevel = 0;
                if (i == 1)
                {
                    currentLevel = (int)CustomerLevel.ZuZhang;
                }
                else if( i <= 3)
                {
                    currentLevel = (int)CustomerLevel.FuZuZhang;
                }
                else
                {
                    currentLevel = (int)CustomerLevel.ZuYuan;
                }

                var currentUser = new Customer
                {
                    CustomerGuid = Guid.NewGuid(),
                    Email = currentUserEmail,
                    Username = currentUserName,
                    Active = true,
                    CreatedOnUtc = DateTime.UtcNow,
                    LastActivityDateUtc = DateTime.UtcNow,
                    RegisteredInStoreId = storeId
                };
                var currentUserAddress = new Address
                {
                    FirstName = "James",
                    LastName = "Pan",
                    PhoneNumber = "369258147",
                    Email = currentUserEmail,
                    FaxNumber = "",
                    Company = "Pan Company",
                    Address1 = "St Katharine抯 West 16",
                    Address2 = "",
                    City = "St Andrews",
                    ZipPostalCode = "KY16 9AX",
                    CreatedOnUtc = DateTime.UtcNow,
                };
                currentUser.Addresses.Add(currentUserAddress);
                currentUser.BillingAddress = currentUserAddress;
                currentUser.ShippingAddress = currentUserAddress;
                
                currentUser.CustomerRoles.Add(crRegistered_Normal);
                currentUser.CustomerRoles.Add(crRegistered_Advanced);

                _customerRepository.Insert(currentUser);

                // 更新用户team
                currentUser.CustomerTeam = defaultTeam;

                //set default customer name
                _genericAttributeService.SaveAttribute(currentUser, SystemCustomerAttributeNames.ZhiXiao_NickName, "测试用户" + i);
                _genericAttributeService.SaveAttribute(currentUser, SystemCustomerAttributeNames.ZhiXiao_InTeamOrder, i);
                _genericAttributeService.SaveAttribute(currentUser, SystemCustomerAttributeNames.ZhiXiao_InTeamTime, addToTeamTime.AddMinutes(i + 5));
                _genericAttributeService.SaveAttribute(currentUser, SystemCustomerAttributeNames.ZhiXiao_LevelId, currentLevel);
                _genericAttributeService.SaveAttribute(currentUser, SystemCustomerAttributeNames.ZhiXiao_MoneyNum, CommonHelper.GenerateRandomInteger(2000, 50000));
                _genericAttributeService.SaveAttribute(currentUser, SystemCustomerAttributeNames.ZhiXiao_MoneyHistory, CommonHelper.GenerateRandomInteger(5000, 50000));
                _genericAttributeService.SaveAttribute(currentUser, SystemCustomerAttributeNames.ZhiXiao_IdCardNum, CommonHelper.GenerateRandomDigitCode(18));
                _genericAttributeService.SaveAttribute(currentUser, SystemCustomerAttributeNames.ZhiXiao_YinHang, "中国银行");
                _genericAttributeService.SaveAttribute(currentUser, SystemCustomerAttributeNames.ZhiXiao_KaiHuHang, "太原支行");
                _genericAttributeService.SaveAttribute(currentUser, SystemCustomerAttributeNames.ZhiXiao_KaiHuMing, currentUserName);
                _genericAttributeService.SaveAttribute(currentUser, SystemCustomerAttributeNames.ZhiXiao_BandNum, CommonHelper.GenerateRandomDigitCode(25));
                _genericAttributeService.SaveAttribute(currentUser, SystemCustomerAttributeNames.ZhiXiao_Password2, "123456");  // 二级密码

                //set customer password
                _customerPasswordRepository.Insert(new CustomerPassword
                {
                    Customer = currentUser,
                    Password = "123456",
                    PasswordFormat = PasswordFormat.Clear,
                    PasswordSalt = string.Empty,
                    CreatedOnUtc = DateTime.UtcNow
                });
            }

            defaultTeam.UserCount = 7;
            _customerTeamRepository.Update(defaultTeam);

            #endregion
        }

        protected virtual void InstallActivityLogTypes()
        {
            var activityLogTypes = new List<ActivityLogType>
            {
                // 直销
                new ActivityLogType
                {
                    SystemKeyword = SystemZhiXiaoLogTypes.AddNewUser,
                    Enabled = true,
                    Name = "小组新增用户"
                },
                new ActivityLogType
                {
                    SystemKeyword = SystemZhiXiaoLogTypes.ReGroupTeam_ReSort,
                    Enabled = true,
                    Name = "重新分组"
                },
                new ActivityLogType
                {
                    SystemKeyword = SystemZhiXiaoLogTypes.ReGroupTeam_AddMoney,
                    Enabled = true,
                    Name = "分组增加奖金"
                },
                new ActivityLogType
                {
                    SystemKeyword = SystemZhiXiaoLogTypes.ReGroupTeam_UpdateLevel,
                    Enabled = true,
                    Name = "分组提升级别"
                },
                new ActivityLogType
                {
                    SystemKeyword = SystemZhiXiaoLogTypes.ReGroupTeam_UpdateLevel,
                    Enabled = true,
                    Name = "分组提升级别"
                },
                new ActivityLogType
                {
                    SystemKeyword = SystemZhiXiaoLogTypes.RechargeMoney,
                    Enabled = true,
                    Name = "充值电子币"
                },
                new ActivityLogType
                {
                    SystemKeyword = SystemZhiXiaoLogTypes.SendProduct,
                    Enabled = true,
                    Name = "管理员发货"
                },
                new ActivityLogType
                {
                    SystemKeyword = SystemZhiXiaoLogTypes.ReceiveProduct,
                    Enabled = true,
                    Name = "用户收货"
                },
                new ActivityLogType
                {
                    SystemKeyword = SystemZhiXiaoLogTypes.Withdraw,
                    Enabled = true,
                    Name = "提现申请"
                },
                new ActivityLogType
                {
                    SystemKeyword = SystemZhiXiaoLogTypes.ProcessWithdraw,
                    Enabled = true,
                    Name = "管理员处理提现申请"
                },

                //admin area activities
                new ActivityLogType
                {
                    SystemKeyword = "AddNewCustomer",
                    Enabled = true,
                    Name = "Add a new customer"
                },
                new ActivityLogType
                {
                    SystemKeyword = "AddNewCustomerAttribute",
                    Enabled = true,
                    Name = "Add a new customer attribute"
                },
                new ActivityLogType
                {
                    SystemKeyword = "AddNewCustomerAttributeValue",
                    Enabled = true,
                    Name = "Add a new customer attribute value"
                },
                new ActivityLogType
                {
                    SystemKeyword = "AddNewCustomerRole",
                    Enabled = true,
                    Name = "Add a new customer role"
                },
                new ActivityLogType
                {
                    SystemKeyword = "AddNewDiscount",
                    Enabled = true,
                    Name = "Add a new discount"
                },
                new ActivityLogType
                {
                    SystemKeyword = "AddNewEmailAccount",
                    Enabled = true,
                    Name = "Add a new email account"
                },
                new ActivityLogType
                {
                    SystemKeyword = "AddNewGiftCard",
                    Enabled = true,
                    Name = "Add a new gift card"
                },
                new ActivityLogType
                {
                    SystemKeyword = "AddNewLanguage",
                    Enabled = true,
                    Name = "Add a new language"
                },
                new ActivityLogType
                {
                    SystemKeyword = "AddNewNews",
                    Enabled = true,
                    Name = "Add a new news"
                },
                new ActivityLogType
                {
                    SystemKeyword = "AddNewProduct",
                    Enabled = true,
                    Name = "Add a new product"
                },
                new ActivityLogType
                {
                    SystemKeyword = "AddNewProductAttribute",
                    Enabled = true,
                    Name = "Add a new product attribute"
                },
                new ActivityLogType
                {
                    SystemKeyword = "AddNewSetting",
                    Enabled = true,
                    Name = "Add a new setting"
                },
                new ActivityLogType
                {
                    SystemKeyword = "AddNewSpecAttribute",
                    Enabled = true,
                    Name = "Add a new specification attribute"
                },
                new ActivityLogType
                {
                    SystemKeyword = "AddNewStateProvince",
                    Enabled = true,
                    Name = "Add a new state or province"
                },
                new ActivityLogType
                {
                    SystemKeyword = "AddNewStore",
                    Enabled = true,
                    Name = "Add a new store"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteActivityLog",
                    Enabled = true,
                    Name = "Delete activity log"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteAddressAttribute",
                    Enabled = true,
                    Name = "Delete an address attribute"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteAddressAttributeValue",
                    Enabled = true,
                    Name = "Delete an address attribute value"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteCategory",
                    Enabled = true,
                    Name = "Delete category"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteCustomer",
                    Enabled = true,
                    Name = "Delete a customer"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteCustomerAttribute",
                    Enabled = true,
                    Name = "Delete a customer attribute"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteCustomerAttributeValue",
                    Enabled = true,
                    Name = "Delete a customer attribute value"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteCustomerRole",
                    Enabled = true,
                    Name = "Delete a customer role"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteDiscount",
                    Enabled = true,
                    Name = "Delete a discount"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteEmailAccount",
                    Enabled = true,
                    Name = "Delete an email account"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteGiftCard",
                    Enabled = true,
                    Name = "Delete a gift card"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteLanguage",
                    Enabled = true,
                    Name = "Delete a language"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteMessageTemplate",
                    Enabled = true,
                    Name = "Delete a message template"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteNews",
                    Enabled = true,
                    Name = "Delete a news"
                },
                 new ActivityLogType
                {
                    SystemKeyword = "DeleteNewsComment",
                    Enabled = true,
                    Name = "Delete a news comment"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeletePlugin",
                    Enabled = true,
                    Name = "Delete a plugin"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteProduct",
                    Enabled = true,
                    Name = "Delete a product"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteProductAttribute",
                    Enabled = true,
                    Name = "Delete a product attribute"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteProductReview",
                    Enabled = true,
                    Name = "Delete a product review"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteReturnRequest",
                    Enabled = true,
                    Name = "Delete a return request"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteSetting",
                    Enabled = true,
                    Name = "Delete a setting"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteStateProvince",
                    Enabled = true,
                    Name = "Delete a state or province"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteStore",
                    Enabled = true,
                    Name = "Delete a store"
                },
                new ActivityLogType
                {
                    SystemKeyword = "DeleteSystemLog",
                    Enabled = true,
                    Name = "Delete system log"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditActivityLogTypes",
                    Enabled = true,
                    Name = "Edit activity log types"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditAddressAttribute",
                    Enabled = true,
                    Name = "Edit an address attribute"
                },
                 new ActivityLogType
                {
                    SystemKeyword = "EditAddressAttributeValue",
                    Enabled = true,
                    Name = "Edit an address attribute value"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditCategory",
                    Enabled = true,
                    Name = "Edit category"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditCheckoutAttribute",
                    Enabled = true,
                    Name = "Edit a checkout attribute"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditCustomer",
                    Enabled = true,
                    Name = "Edit a customer"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditCustomerAttribute",
                    Enabled = true,
                    Name = "Edit a customer attribute"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditCustomerAttributeValue",
                    Enabled = true,
                    Name = "Edit a customer attribute value"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditCustomerRole",
                    Enabled = true,
                    Name = "Edit a customer role"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditEmailAccount",
                    Enabled = true,
                    Name = "Edit an email account"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditGiftCard",
                    Enabled = true,
                    Name = "Edit a gift card"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditLanguage",
                    Enabled = true,
                    Name = "Edit a language"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditNews",
                    Enabled = true,
                    Name = "Edit a news"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditOrder",
                    Enabled = true,
                    Name = "Edit an order"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditProduct",
                    Enabled = true,
                    Name = "Edit a product"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditProductAttribute",
                    Enabled = true,
                    Name = "Edit a product attribute"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditProductReview",
                    Enabled = true,
                    Name = "Edit a product review"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditReturnRequest",
                    Enabled = true,
                    Name = "Edit a return request"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditSettings",
                    Enabled = true,
                    Name = "Edit setting(s)"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditStateProvince",
                    Enabled = true,
                    Name = "Edit a state or province"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditStore",
                    Enabled = true,
                    Name = "Edit a store"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditVendor",
                    Enabled = true,
                    Name = "Edit a vendor"
                },
                new ActivityLogType
                {
                    SystemKeyword = "EditTopic",
                    Enabled = true,
                    Name = "Edit a topic"
                },
                new ActivityLogType
                {
                    SystemKeyword = "Impersonation.Started",
                    Enabled = true,
                    Name = "Customer impersonation session. Started"
                },
                new ActivityLogType
                {
                    SystemKeyword = "Impersonation.Finished",
                    Enabled = true,
                    Name = "Customer impersonation session. Finished"
                },
                new ActivityLogType
                {
                    SystemKeyword = "ImportCategories",
                    Enabled = true,
                    Name = "Categories were imported"
                },
                new ActivityLogType
                {
                    SystemKeyword = "ImportProducts",
                    Enabled = true,
                    Name = "Products were imported"
                },
                new ActivityLogType
                {
                    SystemKeyword = "ImportStates",
                    Enabled = true,
                    Name = "States were imported"
                },
                //public store activities
                new ActivityLogType
                {
                    SystemKeyword = "PublicStore.ViewCategory",
                    Enabled = false,
                    Name = "Public store. View a category"
                },
                new ActivityLogType
                {
                    SystemKeyword = "PublicStore.ViewProduct",
                    Enabled = false,
                    Name = "Public store. View a product"
                },
                new ActivityLogType
                {
                    SystemKeyword = "PublicStore.PlaceOrder",
                    Enabled = false,
                    Name = "Public store. Place an order"
                },
                new ActivityLogType
                {
                    SystemKeyword = "PublicStore.AddToShoppingCart",
                    Enabled = false,
                    Name = "Public store. Add to shopping cart"
                },
                new ActivityLogType
                {
                    SystemKeyword = "PublicStore.Login",
                    Enabled = false,
                    Name = "Public store. Login"
                },
                new ActivityLogType
                {
                    SystemKeyword = "PublicStore.AddNewsComment",
                    Enabled = false,
                    Name = "Public store. Add news comment"
                }
            };
            _activityLogTypeRepository.Insert(activityLogTypes);
        }

        protected virtual void InstallActivityLog(string defaultUserEmail)
        {
            //default customer/user
            var defaultCustomer = _customerRepository.Table.FirstOrDefault(x => x.Email == defaultUserEmail);
            if (defaultCustomer == null)
                throw new Exception("Cannot load default customer");

            _activityLogRepository.Insert(new ActivityLog()
            {
                ActivityLogType = _activityLogTypeRepository.Table.First(alt => alt.SystemKeyword.Equals("EditCategory")),
                Comment = "Edited a category ('Computers')",
                CreatedOnUtc = DateTime.UtcNow,
                Customer = defaultCustomer,
                IpAddress = "127.0.0.1"
            });
            _activityLogRepository.Insert(new ActivityLog()
            {
                ActivityLogType = _activityLogTypeRepository.Table.First(alt => alt.SystemKeyword.Equals("DeleteLanguage")),
                Comment = "Edited a discount ('Sample discount with coupon code')",
                CreatedOnUtc = DateTime.UtcNow,
                Customer = defaultCustomer,
                IpAddress = "127.0.0.1"
            });
            _activityLogRepository.Insert(new ActivityLog()
            {
                ActivityLogType = _activityLogTypeRepository.Table.First(alt => alt.SystemKeyword.Equals("AddNewCustomer")),
                Comment = "Edited a specification attribute ('CPU Type')",
                CreatedOnUtc = DateTime.UtcNow,
                Customer = defaultCustomer,
                IpAddress = "127.0.0.1"
            });
            _activityLogRepository.Insert(new ActivityLog()
            {
                ActivityLogType = _activityLogTypeRepository.Table.First(alt => alt.SystemKeyword.Equals("AddNewCustomerAttributeValue")),
                Comment = "Added a new product attribute ('Some attribute')",
                CreatedOnUtc = DateTime.UtcNow,
                Customer = defaultCustomer,
                IpAddress = "127.0.0.1"
            });
            _activityLogRepository.Insert(new ActivityLog()
            {
                ActivityLogType = _activityLogTypeRepository.Table.First(alt => alt.SystemKeyword.Equals("AddNewCustomerRole")),
                Comment = "Deleted a gift card ('bdbbc0ef-be57')",
                CreatedOnUtc = DateTime.UtcNow,
                Customer = defaultCustomer,
                IpAddress = "127.0.0.1"
            });
        }

        protected virtual void InstallZhiXiaoActivityLog(string defaultUserEmail)
        {
            //default customer/user
            var defaultCustomer = _customerRepository.Table.FirstOrDefault(x => x.Email == defaultUserEmail);
            if (defaultCustomer == null)
                throw new Exception("Cannot load default customer");

            
            _activityLogRepository.Insert(new ActivityLog()
            {
                ActivityLogType = _activityLogTypeRepository.Table.First(alt => alt.SystemKeyword.Equals(SystemZhiXiaoLogTypes.AddNewUser)),
                Comment = "小组新增用户",
                CreatedOnUtc = DateTime.UtcNow,
                Customer = defaultCustomer,
                IpAddress = "127.0.0.1"
            });
            _activityLogRepository.Insert(new ActivityLog()
            {
                ActivityLogType = _activityLogTypeRepository.Table.First(alt => alt.SystemKeyword.Equals(SystemZhiXiaoLogTypes.ReGroupTeam_ReSort)),
                Comment = "重新分组",
                CreatedOnUtc = DateTime.UtcNow,
                Customer = defaultCustomer,
                IpAddress = "127.0.0.1"
            });
            _activityLogRepository.Insert(new ActivityLog()
            {
                ActivityLogType = _activityLogTypeRepository.Table.First(alt => alt.SystemKeyword.Equals(SystemZhiXiaoLogTypes.ReGroupTeam_AddMoney)),
                Comment = "分组增加奖金",
                CreatedOnUtc = DateTime.UtcNow,
                Customer = defaultCustomer,
                IpAddress = "127.0.0.1"
            });
            _activityLogRepository.Insert(new ActivityLog()
            {
                ActivityLogType = _activityLogTypeRepository.Table.First(alt => alt.SystemKeyword.Equals(SystemZhiXiaoLogTypes.ReGroupTeam_UpdateLevel)),
                Comment = "分组提升级别",
                CreatedOnUtc = DateTime.UtcNow,
                Customer = defaultCustomer,
                IpAddress = "127.0.0.1"
            });
        }

        protected virtual void InstallSettings(bool installSampleData)
        {
            var settingService = EngineContext.Current.Resolve<ISettingService>();
            settingService.SaveSetting(new CommonSettings
            {
                UseSystemEmailForContactUsForm = true,
                UseStoredProceduresIfSupported = false, // 不使用存储过程 
                UseStoredProcedureForLoadingCategories = false,
                SitemapEnabled = true,
                SitemapIncludeCategories = true,
                SitemapIncludeManufacturers = true,
                SitemapIncludeProducts = false,
                DisplayJavaScriptDisabledWarning = false,
                UseFullTextSearch = false,
                FullTextMode = FulltextSearchMode.ExactMatch,
                Log404Errors = true,
                BreadcrumbDelimiter = "/",
                RenderXuaCompatible = false,
                XuaCompatibleValue = "IE=edge",
                BbcodeEditorOpenLinksInNewWindow = false
            });

            settingService.SaveSetting(new SeoSettings
            {
                PageTitleSeparator = ". ",
                PageTitleSeoAdjustment = PageTitleSeoAdjustment.PagenameAfterStorename,
                DefaultTitle = "直销系统",
                DefaultMetaKeywords = "",
                DefaultMetaDescription = "",
                GenerateProductMetaDescription = true,
                ConvertNonWesternChars = false,
                AllowUnicodeCharsInUrls = true,
                CanonicalUrlsEnabled = false,
                WwwRequirement = WwwRequirement.NoMatter,
                //we disable bundling out of the box because it requires a lot of server resources
                EnableJsBundling = false,
                EnableCssBundling = false,
                TwitterMetaTags = true,
                OpenGraphMetaTags = true,
                ReservedUrlRecordSlugs = new List<string>
                {
                    "admin",
                    "install",
                    "recentlyviewedproducts",
                    "newproducts",
                    "compareproducts",
                    "clearcomparelist",
                    "setproductreviewhelpfulness",
                    "login",
                    "register",
                    "logout",
                    "cart",
                    "wishlist",
                    "emailwishlist",
                    "checkout",
                    "onepagecheckout",
                    "contactus",
                    "passwordrecovery",
                    "subscribenewsletter",
                    "blog",
                    "boards",
                    "inboxupdate",
                    "sentupdate",
                    "news",
                    "sitemap",
                    "search",
                    "config",
                    "eucookielawaccept",
                    "page-not-found",
                    //system names are not allowed (anyway they will cause a runtime error),
                    "con",
                    "lpt1",
                    "lpt2",
                    "lpt3",
                    "lpt4",
                    "lpt5",
                    "lpt6",
                    "lpt7",
                    "lpt8",
                    "lpt9",
                    "com1",
                    "com2",
                    "com3",
                    "com4",
                    "com5",
                    "com6",
                    "com7",
                    "com8",
                    "com9",
                    "null",
                    "prn",
                    "aux"
                },
                CustomHeadTags = ""
            });

            settingService.SaveSetting(new AdminAreaSettings
            {
                DefaultGridPageSize = 15,
                PopupGridPageSize = 10,
                GridPageSizes = "10, 15, 20, 50, 100",
                RichEditorAdditionalSettings = null,
                RichEditorAllowJavaScript = false,
                UseRichEditorInMessageTemplates = false,
                UseIsoDateTimeConverterInJson = true
            });

            settingService.SaveSetting(new LocalizationSettings
            {
                DefaultAdminLanguageId = _languageRepository.Table.Single(l => l.Name == "English").Id,
                UseImagesForLanguageSelection = false,
                SeoFriendlyUrlsForLanguagesEnabled = false,
                AutomaticallyDetectLanguage = false,
                LoadAllLocaleRecordsOnStartup = true,
                LoadAllLocalizedPropertiesOnStartup = true,
                LoadAllUrlRecordsOnStartup = false,
                IgnoreRtlPropertyForAdminArea = false
            });

            settingService.SaveSetting(new CustomerSettings
            {
                UsernamesEnabled = true,
                CheckUsernameAvailabilityEnabled = false,
                AllowUsersToChangeUsernames = false,
                DefaultPasswordFormat = PasswordFormat.Hashed,
                HashedPasswordFormat = "SHA1",
                PasswordMinLength = 6,
                UnduplicatedPasswordsNumber = 1,
                PasswordRecoveryLinkDaysValid = 7,
                PasswordLifetime = 90,
                FailedPasswordAllowedAttempts = 0,
                FailedPasswordLockoutMinutes = 30,
                UserRegistrationType = UserRegistrationType.Standard,
                AllowCustomersToUploadAvatars = false,
                AvatarMaximumSizeBytes = 20000,
                DefaultAvatarEnabled = true,
                ShowCustomersLocation = false,
                ShowCustomersJoinDate = false,
                AllowViewingProfiles = false,
                NotifyNewCustomerRegistration = false,
                HideDownloadableProductsTab = false,
                HideBackInStockSubscriptionsTab = false,
                DownloadableProductsValidateUser = false,
                CustomerNameFormat = CustomerNameFormat.ShowFirstName,
                GenderEnabled = true,
                DateOfBirthEnabled = false,
                DateOfBirthRequired = false,
                DateOfBirthMinimumAge = null,
                CompanyEnabled = true,
                StreetAddressEnabled = true,
                StreetAddress2Enabled = false,
                ZipPostalCodeEnabled = false,
                CityEnabled = true,
                CountryEnabled = false,
                CountryRequired = false,
                StateProvinceEnabled = true,
                StateProvinceRequired = false,
                PhoneEnabled = true,
                FaxEnabled = false,
                AcceptPrivacyPolicyEnabled = false,
                NewsletterEnabled = true,
                NewsletterTickedByDefault = true,
                HideNewsletterBlock = false,
                NewsletterBlockAllowToUnsubscribe = false,
                OnlineCustomerMinutes = 20,
                StoreLastVisitedPage = false,
                SuffixDeletedCustomers = false,
                EnteringEmailTwice = false,
                RequireRegistrationForDownloadableProducts = false,
                DeleteGuestTaskOlderThanMinutes = 1440,
                // phone number regex
                PhoneNumberRegex = "^1[34578]\\d{9}$",
                // Team number mask
                TeamNumberMask = "{YY}{MM}{#:0000}"
            });

            settingService.SaveSetting(new AddressSettings
            {
                CompanyEnabled = true,
                StreetAddressEnabled = true,
                StreetAddressRequired = true,
                StreetAddress2Enabled = true,
                ZipPostalCodeEnabled = true,
                ZipPostalCodeRequired = true,
                CityEnabled = true,
                CityRequired = true,
                CountryEnabled = true,
                StateProvinceEnabled = true,
                PhoneEnabled = true,
                PhoneRequired = true,
                FaxEnabled = true,
            });

            settingService.SaveSetting(new StoreInformationSettings
            {
                StoreClosed = false,
                DefaultStoreTheme = "DefaultClean",
                AllowCustomerToSelectTheme = false,
                DisplayMiniProfilerInPublicStore = false,
                DisplayMiniProfilerForAdminOnly = false,
                DisplayEuCookieLawWarning = false,
                FacebookLink = "http://www.facebook.com/nopCommerce",
                TwitterLink = "https://twitter.com/nopCommerce",
                YoutubeLink = "http://www.youtube.com/user/nopCommerce",
                GooglePlusLink = "https://plus.google.com/+nopcommerce",
                HidePoweredByNopCommerce = true
            });

            settingService.SaveSetting(new RewardPointsSettings
            {
                Enabled = true,
                ExchangeRate = 1,
                PointsForRegistration = 0,
                PointsForPurchases_Amount = 10,
                PointsForPurchases_Points = 1,
                ActivationDelay = 0,
                ActivationDelayPeriodId = 0,
                DisplayHowMuchWillBeEarned = true,
                PointsAccumulatedForAllStores = true,
                PageSize = 10
            });

            settingService.SaveSetting(new NewsSettings
            {
                Enabled = true,
                AllowNotRegisteredUsersToLeaveComments = true,
                NotifyAboutNewNewsComments = false,
                ShowNewsOnMainPage = true,
                MainPageNewsCount = 3,
                NewsArchivePageSize = 10,
                ShowHeaderRssUrl = false,
                NewsCommentsMustBeApproved = false,
                ShowNewsCommentsPerStore = false
            });

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
                Withdraw_Rate = 0.90,
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
                NewUserMoney_ZuZhang_Normal = 1000,
                NewUserMoney_ZuZhang_Advanced = 3000,

                /// <summary>
                /// 新增用户时副组长分的钱
                /// </summary>
                NewUserMoney_FuZuZhang_Normal = 300,
                NewUserMoney_FuZuZhang_Advanced = 800,

                /// <summary>
                /// 新增用户时组员分的钱
                /// </summary>
                NewUserMoney_ZuYuan_Normal = 100,
                NewUserMoney_ZuYuan_Advanced = 200,

                /// <summary>
                /// 重新分组时组长分的钱
                /// </summary>
                ReGroupMoney_ZuZhang_Normal = 22000,
                ReGroupMoney_ZuZhang_Advanced = 80000,
                /// <summary>
                /// 重新分组时前x个组员分钱
                /// </summary>
                ReGroupMoney_ZuYuan_Count = 4,
                /// <summary>
                /// 重新分组时组员钱数(一般用户)
                /// </summary>
                ReGroupMoney_ZuYuan_Normal = 800,
                /// <summary>
                /// 重新分组时组员钱数(高级用户)
                /// </summary>
                ReGroupMoney_ZuYuan_Advanced = 1600,

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
            });
        }

        #endregion

        #region Methods

        public virtual void InstallData(string defaultUserEmail,
            string defaultUserPassword, bool installSampleData = true)
        {
            InstallStores();
            InstallLanguages();
            InstallSettings(installSampleData);
            InstallLocaleResources();
            InstallCustomerTeams();
            InstallCustomersAndUsers(defaultUserEmail, defaultUserPassword);

            InstallZhiXiaoTestUser_Normal();
            InstallZhiXiaoTestUser_Advanced();

            InstallActivityLogTypes();

            if (installSampleData)
            {
                // no data
                InstallActivityLog(defaultUserEmail);

                InstallZhiXiaoActivityLog("user_1@yourStore.com");
            }
        }

        #endregion
    }
}