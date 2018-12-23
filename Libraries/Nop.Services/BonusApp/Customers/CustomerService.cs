using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.BonusApp.Configuration;
using Nop.Core.Domain.BonusApp.Customers;
using Nop.Data;
using Nop.Services.Events;

namespace Nop.Services.BonusApp.Customers
{
    public class BonusApp_CustomerService : IBonusApp_CustomerService
    {
        #region Fields

        private readonly IRepository<BonusApp_Customer> _customerRepository;
        private readonly IDataProvider _dataProvider;
        private readonly IDbContext _dbContext;
        private readonly ICacheManager _cacheManager;
        private readonly IEventPublisher _eventPublisher;
        private readonly BonusAppSettings _bonusAppSettings;

        #endregion

        #region Ctor

        public BonusApp_CustomerService(ICacheManager cacheManager,
            IRepository<BonusApp_Customer> customerRepository,
            IDataProvider dataProvider,
            IDbContext dbContext,
            IEventPublisher eventPublisher,
            BonusAppSettings bonusAppSettings)
        {
            this._cacheManager = cacheManager;
            this._customerRepository = customerRepository;

            this._dataProvider = dataProvider;
            this._dbContext = dbContext;
            this._eventPublisher = eventPublisher;
            this._bonusAppSettings = bonusAppSettings;
        }

        #endregion

        #region Methods

        #region Customers

        public IPagedList<BonusApp_Customer> GetAllCustomers(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, string username = null, string phone = null, string ipAddress = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _customerRepository.Table;
            if (createdFromUtc.HasValue)
                query = query.Where(c => createdFromUtc.Value <= c.CreatedOnUtc);
            if (createdToUtc.HasValue)
                query = query.Where(c => createdToUtc.Value >= c.CreatedOnUtc);

            query = query.Where(c => !c.Deleted);

            if (!String.IsNullOrWhiteSpace(username))
                query = query.Where(c => c.Username.Contains(username));

            //search by phone
            if (!String.IsNullOrWhiteSpace(phone))
                query = query.Where(c => c.PhoneNumber.Contains(phone));

            //search by IpAddress
            if (!String.IsNullOrWhiteSpace(ipAddress) && CommonHelper.IsValidIpAddress(ipAddress))
            {
                query = query.Where(w => w.LastIpAddress == ipAddress);
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
        
        #endregion

        #endregion
    }
}