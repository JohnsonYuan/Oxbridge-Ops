using System;
using System.Collections.Generic;
using Nop.Core;
using Nop.Core.Domain.BonusApp.Customers;
using Nop.Core.Domain.BonusApp.Logging;
using Nop.Core.Domain.Customers;

namespace Nop.Services.BonusApp.Customers
{
    public interface IBonusApp_CustomerService
    {
        #region Customers

        /// <summary>
        /// Gets all customers
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="username">Username; null to load all customers</param>
        /// <param name="phone">Phone; null to load all customers</param>
        /// <param name="ipAddress">IP address; null to load all customers</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Customers</returns>
        IPagedList<BonusApp_Customer> GetAllCustomers(DateTime? createdFromUtc = null,
            DateTime? createdToUtc = null, string username = null,
            string phone = null, string ipAddress = null,
            int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Gets online customers
        /// </summary>
        /// <param name="lastActivityFromUtc">Customer last activity date (from)</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Customers</returns>
        IPagedList<BonusApp_Customer> GetOnlineCustomers(DateTime lastActivityFromUtc,
            int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Delete a customer
        /// </summary>
        /// <param name="customer">Customer</param>
        void DeleteCustomer(BonusApp_Customer customer);

        /// <summary>
        /// Gets a customer
        /// </summary>
        /// <param name="customerId">Customer identifier</param>
        /// <returns>A customer</returns>
        BonusApp_Customer GetCustomerById(int customerId);

        /// <summary>
        /// Get customers by identifiers
        /// </summary>
        /// <param name="customerIds">Customer identifiers</param>
        /// <returns>Customers</returns>
        IList<BonusApp_Customer> GetCustomersByIds(int[] customerIds);

        /// <summary>
        /// Gets a customer by GUID
        /// </summary>
        /// <param name="customerGuid">Customer GUID</param>
        /// <returns>A customer</returns>
        BonusApp_Customer GetCustomerByGuid(Guid customerGuid);

        /// <summary>
        /// Get customer by username
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns>Customer</returns>
        BonusApp_Customer GetCustomerByUsername(string username);
        /// <summary>
        /// Get customer by nickname
        /// </summary>
        /// <param name="nickName">NickName</param>
        /// <returns>Customer</returns>
        BonusApp_Customer GetCustomerByNickName(string nickName);
        /// <summary>
        /// Get customer by phone number
        /// </summary>
        /// <param name="phoneNumber">PhoneNumber</param>
        /// <returns>Customer</returns>
        BonusApp_Customer GetCustomerByPhoneNumber(string phoneNumber);

        /// <summary>
        /// Insert a customer
        /// </summary>
        /// <param name="customer">Customer</param>
        void InsertCustomer(BonusApp_Customer customer);

        /// <summary>
        /// Updates the customer
        /// </summary>
        /// <param name="customer">Customer</param>
        void UpdateCustomer(BonusApp_Customer customer);
        
        void UpdateCustomerPassword(BonusApp_Customer customer, string newPassword);
        
        #endregion

        #region Login

        CustomerLoginResults ValidateCustomer(string username, string password);

        #endregion

        #region Register

        string Register(string username, string password);

        #endregion

        #region Comment

        IPagedList<BonusApp_CustomerComment> GetAllCustomerComments(DateTime? createdFromUtc = null,
            DateTime? createdToUtc = null,
            int? customerId = null,
            string ipAddress = null,
            bool? enabled = true,
            int pageIndex = 0,
            int pageSize = int.MaxValue);

        /// <summary>
        /// Insert a customer
        /// </summary>
        /// <param name="customer">Customer</param>
        void InsertComment(BonusApp_CustomerComment comment);

        /// <summary>
        /// Updates the customer
        /// </summary>
        /// <param name="customer">Customer</param>
        void UpdateCustomer(BonusApp_CustomerComment comment);

        void DisableComment(BonusApp_CustomerComment comment);

        /// <summary>
        /// Delete a customer
        /// </summary>
        /// <param name="customer">Customer</param>
        void DeleteCustomer(BonusApp_CustomerComment comment);

        /// <summary>
        /// Gets a customer
        /// </summary>
        /// <param name="customerId">Customer identifier</param>
        /// <returns>A customer</returns>
        BonusApp_CustomerComment GetCommentById(int commentId);

        #endregion
    }
}