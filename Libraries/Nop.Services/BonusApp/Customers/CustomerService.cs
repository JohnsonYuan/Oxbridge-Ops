using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.BonusApp.Configuration;
using Nop.Core.Domain.BonusApp.Customers;
using Nop.Core.Domain.Customers;
using Nop.Data;
using Nop.Services.Events;
using Nop.Services.Security;

namespace Nop.Services.BonusApp.Customers
{
    public class BonusApp_CustomerService : IBonusApp_CustomerService
    {
        #region Fields

        private readonly IRepository<BonusApp_Customer> _customerRepository;
        private readonly IRepository<BonusApp_CustomerComment> _customerCommentRepository;
        private readonly IDataProvider _dataProvider;
        private readonly IDbContext _dbContext;
        private readonly ICacheManager _cacheManager;
        private readonly IEventPublisher _eventPublisher;
        private readonly IEncryptionService _encryptionService;
        private readonly IWebHelper _webHelper;
        private readonly BonusAppSettings _bonusAppSettings;
        private readonly HttpContextBase _httpContext;

        #endregion

        #region Ctor

        public BonusApp_CustomerService(ICacheManager cacheManager,
            IRepository<BonusApp_Customer> customerRepository,
            IRepository<BonusApp_CustomerComment> customerCommentRepository,
            IDataProvider dataProvider,
            IDbContext dbContext,
            IEventPublisher eventPublisher,
            IEncryptionService encryptionService,
            IWebHelper webHelper,
            BonusAppSettings bonusAppSettings,
            HttpContextBase httpContext)
        {
            this._cacheManager = cacheManager;
            this._customerRepository = customerRepository;
            this._customerCommentRepository = customerCommentRepository;

            this._dataProvider = dataProvider;
            this._dbContext = dbContext;
            this._eventPublisher = eventPublisher;
            this._encryptionService = encryptionService;
            this._webHelper = webHelper;
            this._bonusAppSettings = bonusAppSettings;
            this._httpContext = httpContext;
        }

        #endregion

        #region Methods

        #region Customers

        public IPagedList<BonusApp_Customer> GetAllCustomers(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, string username = null, string nickname = null, string phone = null, string ipAddress = null, decimal? minMoney = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _customerRepository.Table;

            if (createdFromUtc.HasValue)
                query = query.Where(c => createdFromUtc.Value <= c.CreatedOnUtc);
            if (createdToUtc.HasValue)
                query = query.Where(c => createdToUtc.Value >= c.CreatedOnUtc);

            query = query.Where(c => !c.Deleted);

            if (!String.IsNullOrWhiteSpace(username))
                query = query.Where(c => c.Username.Contains(username));

            if (!String.IsNullOrWhiteSpace(nickname))
                query = query.Where(c => c.Nickname.Contains(nickname));

            //search by phone
            if (!String.IsNullOrWhiteSpace(phone))
                query = query.Where(c => c.PhoneNumber.Contains(phone));

            //search by IpAddress
            if (!String.IsNullOrWhiteSpace(ipAddress) && CommonHelper.IsValidIpAddress(ipAddress))
            {
                query = query.Where(w => w.LastIpAddress == ipAddress);
            }

            if (minMoney.HasValue)
            {
                query = query.Where(w => w.Money >= minMoney);
            }

            query = query.OrderByDescending(c => c.CreatedOnUtc);

            var customers = new PagedList<BonusApp_Customer>(query, pageIndex, pageSize);
            return customers;
        }

        /// <summary>
        /// Gets online customers
        /// </summary>
        /// <param name="lastActivityFromUtc">Customer last activity date (from)</param>
        /// <param name="customerRoleIds">A list of customer role identifiers to filter by (at least one match); pass null or empty list in order to load all customers; </param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Customers</returns>
        public virtual IPagedList<BonusApp_Customer> GetOnlineCustomers(DateTime lastActivityFromUtc,
            int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _customerRepository.Table;
            query = query.Where(c => lastActivityFromUtc <= c.LastActivityDateUtc);
            query = query.Where(c => !c.Deleted);

            query = query.OrderByDescending(c => c.LastActivityDateUtc);
            var customers = new PagedList<BonusApp_Customer>(query, pageIndex, pageSize);
            return customers;
        }

        /// <summary>
        /// Delete a customer
        /// </summary>
        /// <param name="customer">Customer</param>
        public virtual void DeleteCustomer(BonusApp_Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            customer.Deleted = true;

            if (!String.IsNullOrEmpty(customer.Username))
                customer.Username += "-DELETED";

            //event notification
            _eventPublisher.EntityDeleted(customer);
        }

        /// <summary>
        /// Gets a customer
        /// </summary>
        /// <param name="customerId">Customer identifier</param>
        /// <returns>A customer</returns>
        public virtual BonusApp_Customer GetCustomerById(int customerId)
        {
            if (customerId == 0)
                return null;

            return _customerRepository.GetById(customerId);
        }

        /// <summary>
        /// Get customers by identifiers
        /// </summary>
        /// <param name="customerIds">Customer identifiers</param>
        /// <returns>Customers</returns>
        public virtual IList<BonusApp_Customer> GetCustomersByIds(int[] customerIds)
        {
            if (customerIds == null || customerIds.Length == 0)
                return new List<BonusApp_Customer>();

            var query = from c in _customerRepository.Table
                        where customerIds.Contains(c.Id) && !c.Deleted
                        select c;
            var customers = query.ToList();
            //sort by passed identifiers
            var sortedCustomers = new List<BonusApp_Customer>();
            foreach (int id in customerIds)
            {
                var customer = customers.Find(x => x.Id == id);
                if (customer != null)
                    sortedCustomers.Add(customer);
            }
            return sortedCustomers;
        }

        /// <summary>
        /// Gets a customer by GUID
        /// </summary>
        /// <param name="customerGuid">Customer GUID</param>
        /// <returns>A customer</returns>
        public virtual BonusApp_Customer GetCustomerByGuid(Guid customerGuid)
        {
            if (customerGuid == Guid.Empty)
                return null;

            var query = from c in _customerRepository.Table
                        where c.CustomerGuid == customerGuid
                        orderby c.Id
                        select c;
            var customer = query.FirstOrDefault();
            return customer;
        }

        /// <summary>
        /// Get customer by username
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns>Customer</returns>
        public virtual BonusApp_Customer GetCustomerByUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;

            var query = from c in _customerRepository.Table
                        orderby c.Id
                        where c.Username == username
                        select c;
            var customer = query.FirstOrDefault();
            return customer;
        }

        /// <summary>
        /// Get customer by nickname
        /// </summary>
        /// <param name="nickName">NickName</param>
        /// <returns>Customer</returns>
        public virtual BonusApp_Customer GetCustomerByNickName(string nickName)
        {
            if (string.IsNullOrWhiteSpace(nickName))
                return null;

            var query = from c in _customerRepository.Table
                        orderby c.Id
                        where c.Nickname == nickName
                        select c;
            var customer = query.FirstOrDefault();
            return customer;
        }

        /// <summary>
        /// Get customer by phone number
        /// </summary>
        /// <param name="phoneNumber">PhoneNumber</param>
        /// <returns>Customer</returns>
        public virtual BonusApp_Customer GetCustomerByPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return null;

            var query = from c in _customerRepository.Table
                        orderby c.Id
                        where c.PhoneNumber == phoneNumber
                        select c;
            var customer = query.FirstOrDefault();
            return customer;
        }

        /// <summary>
        /// Insert a customer
        /// </summary>
        /// <param name="customer">Customer</param>
        public virtual void InsertCustomer(BonusApp_Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            _customerRepository.Insert(customer);

            //event notification
            _eventPublisher.EntityInserted(customer);
        }

        /// <summary>
        /// Updates the customer
        /// </summary>
        /// <param name="customer">Customer</param>
        public virtual void UpdateCustomer(BonusApp_Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            _customerRepository.Update(customer);

            //event notification
            _eventPublisher.EntityUpdated(customer);
        }

        /// <summary>
        /// Update password
        /// </summary>
        /// <param name="customer"></param>
        /// <param name="newPassword"></param>
        public virtual ChangePasswordResult ChangeCustomerPassword(BonusApp_Customer customer, string newPassword)
        {
            var result = new ChangePasswordResult();
            if (customer == null)
            {
                result.AddError("用户不存在");
                return result;
            }

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                result.AddError("新密码没有输入");
                return result;
            }

            customer.Password = _encryptionService.CreatePasswordHash(newPassword, _bonusAppSettings.CustomerPasswordSalt, _bonusAppSettings.HashedPasswordFormat);
            _customerRepository.Update(customer);

            //event notification
            _eventPublisher.EntityUpdated(customer);

            return result;
        }

        #endregion

        #region Login

        public virtual CustomerLoginResults ValidateCustomer(string username, string password)
        {
            var customer = GetCustomerByUsername(username);

            if (customer == null)
                return CustomerLoginResults.CustomerNotExist;
            if (customer.Deleted)
                return CustomerLoginResults.Deleted;
            if (!customer.Active)
                return CustomerLoginResults.NotActive;

            var hashedPwd = _encryptionService.CreatePasswordHash(password, _bonusAppSettings.CustomerPasswordSalt, _bonusAppSettings.HashedPasswordFormat);

            var pwdMatch = customer.Password.Equals(hashedPwd);
            if (!pwdMatch)
                return CustomerLoginResults.WrongPassword;

            return CustomerLoginResults.Successful;
        }

        #endregion

        #region Register

        /// <summary>
        /// Register new customer
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public virtual string Register(string username, string password, string nickname, string phone)
        {
            // 验证新用户(CustomerModel)的是否重复
            if (!String.IsNullOrWhiteSpace(username))
            {
                var cust2 = GetCustomerByUsername(username);
                if (cust2 != null)
                    return "用户名已经使用";
            }

            // 不验证姓名nickname是否重复
            //{
            //    var cust2 = GetCustomerByNickName(username);
            //    if (cust2 != null)
            //        return ("姓名已经使用");
            //}

            //if (!String.IsNullOrWhiteSpace(model.))
            //{
            //    var cust2 = _customerService.GetCustomerByPhoneNumber(model.Username);
            //    if (cust2 != null)
            //        return ("手机号已经使用");
            //}

            BonusApp_Customer customer = new BonusApp_Customer
            {
                CustomerGuid = Guid.NewGuid(),
                AvatarFileName = "default-avatar.jpg",
                // 邮箱默认后缀为@yourStore.com
                Username = username,
                Password = _encryptionService.CreatePasswordHash(password, _bonusAppSettings.CustomerPasswordSalt, _bonusAppSettings.HashedPasswordFormat),
                Nickname = nickname,
                PhoneNumber = phone, 
                Money = 0,
                //VendorId = model.VendorId,
                Active = true,
                Deleted = false,
                CreatedOnUtc = DateTime.UtcNow,
                LastActivityDateUtc = DateTime.UtcNow,
            };

            InsertCustomer(customer);

            return string.Empty;
        }

        #endregion

        #region Comments

        public IPagedList<BonusApp_CustomerComment> GetAllCustomerComments(DateTime? createdFromUtc = null,
            DateTime? createdToUtc = null,
            int? customerId = null,
            string ipAddress = null,
            bool? enabled = true,
            int pageIndex = 0,
            int pageSize = int.MaxValue)
        {
            var query = _customerCommentRepository.Table;

            if (createdFromUtc.HasValue)
                query = query.Where(c => createdFromUtc.Value <= c.CreatedOnUtc);
            if (createdToUtc.HasValue)
                query = query.Where(c => createdToUtc.Value >= c.CreatedOnUtc);

            if (customerId.HasValue)
                query = query.Where(al => customerId.Value == al.CustomerId);

            if (enabled.HasValue)
                query = query.Where(c => c.Enabled == enabled);

            //search by IpAddress
            if (!String.IsNullOrWhiteSpace(ipAddress) && CommonHelper.IsValidIpAddress(ipAddress))
            {
                query = query.Where(w => w.IpAddress == ipAddress);
            }

            query = query.OrderByDescending(c => c.CreatedOnUtc);
            query = query.IncludeProperties(c => c.Customer);

            var comments = new PagedList<BonusApp_CustomerComment>(query, pageIndex, pageSize);
            return comments;
        }

        /// <summary>
        /// Insert a customer
        /// </summary>
        /// <param name="customer">Customer</param>
        public virtual void InsertComment(BonusApp_CustomerComment comment)
        {
            if (comment == null)
                throw new ArgumentNullException("comment");

            _customerCommentRepository.Insert(comment);

            //event notification
            _eventPublisher.EntityInserted(comment);
        }

        public virtual void InsertComment(BonusApp_Customer customer, string content, int rate)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            if (content == null)
                throw new ArgumentNullException("content");

            if (rate <= 0)
                throw new ArgumentException("rate");

            BonusApp_CustomerComment comment = new BonusApp_CustomerComment
            {
                CustomerId = customer.Id,
                Comment = content,
                Rate = rate,
                Enabled = true,
                IpAddress = _webHelper.GetCurrentIpAddress(),
                CreatedOnUtc = DateTime.UtcNow
            };

            InsertComment(comment);
        }

        /// <summary>
        /// Updates the customer
        /// </summary>
        /// <param name="customer">Customer</param>
        public virtual void UpdateComment(BonusApp_CustomerComment comment)
        {
            if (comment == null)
                throw new ArgumentNullException("comment");

            _customerCommentRepository.Update(comment);

            //event notification
            _eventPublisher.EntityUpdated(comment);
        }

        public virtual void DisableComment(BonusApp_CustomerComment comment)
        {
            if (comment == null)
                throw new ArgumentNullException("comment");

            comment.Enabled = false;
            _customerCommentRepository.Update(comment);

            //event notification
            _eventPublisher.EntityUpdated(comment);
        }

        /// <summary>
        /// Delete a customer
        /// </summary>
        /// <param name="customer">Customer</param>
        public virtual void DeleteCustomer(BonusApp_CustomerComment comment)
        {
            if (comment == null)
                throw new ArgumentNullException("comment");

            _customerCommentRepository.Delete(comment);

            //event notification
            _eventPublisher.EntityDeleted(comment);
        }

        /// <summary>
        /// Gets a customer
        /// </summary>
        /// <param name="customerId">Customer identifier</param>
        /// <returns>A customer</returns>
        public virtual BonusApp_CustomerComment GetCommentById(int commentId)
        {
            if (commentId == 0)
                return null;

            return _customerCommentRepository.GetById(commentId);
        }

        #endregion

        #endregion
    }
}