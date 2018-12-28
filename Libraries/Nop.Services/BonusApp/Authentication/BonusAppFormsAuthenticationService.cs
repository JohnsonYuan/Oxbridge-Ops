using System;
using System.Web;
using System.Web.Security;
using Nop.Core.Domain.BonusApp;
using Nop.Core.Domain.BonusApp.Configuration;
using Nop.Core.Domain.BonusApp.Customers;
using Nop.Services.BonusApp.Customers;

namespace Nop.Services.BonusApp.Authentication
{
    /// <summary>
    /// Bonus app Authentication service
    /// </summary>
    public class BonusAppFormsAuthenticationService : IBonusAppFormsAuthenticationService
    {
        #region Fields
        
        private readonly BonusAppSettings _bonusAppSettings;
        private readonly HttpContextBase _httpContext;
        private readonly IBonusApp_CustomerService _customerService;
        private readonly TimeSpan _expirationTimeSpan;

        private BonusApp_Customer _cachedCustomer;

        #endregion


        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="httpContext">HTTP context</param>
        /// <param name="customerService">Customer service</param>
        /// <param name="customerSettings">Customer settings</param>
        public BonusAppFormsAuthenticationService(
            BonusAppSettings bonusAppSettings,
            HttpContextBase httpContext,
            IBonusApp_CustomerService customerService)
        {
            this._bonusAppSettings = bonusAppSettings;
            this._httpContext = httpContext;
            this._customerService = customerService;
            this._expirationTimeSpan = FormsAuthentication.Timeout;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Get authenticated customer
        /// </summary>
        /// <param name="ticket">Ticket</param>
        /// <returns>Customer</returns>
        protected virtual BonusApp_Customer GetAuthenticatedCustomerFromTicket(FormsAuthenticationTicket ticket)
        {
            if (ticket == null)
                throw new ArgumentNullException("ticket");

            var usernameOrEmail = ticket.UserData;

            if (String.IsNullOrWhiteSpace(usernameOrEmail))
                return null;
            var customer = _customerService.GetCustomerByUsername(usernameOrEmail);
            return customer;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sign in
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="createPersistentCookie">A value indicating whether to create a persistent cookie</param>
        public virtual void SignIn(BonusApp_Customer customer, bool createPersistentCookie)
        {
            var now = DateTime.UtcNow.ToLocalTime();

            var ticket = new FormsAuthenticationTicket(
                1 /*version*/,
                customer.Username,
                now,
                now.Add(_expirationTimeSpan),
                createPersistentCookie,
                customer.Username,
                FormsAuthentication.FormsCookiePath);

            var encryptedTicket = FormsAuthentication.Encrypt(ticket);

            // use bonus app cookie name
            var cookie = new HttpCookie(_bonusAppSettings.AuthCookieName, encryptedTicket);
            cookie.HttpOnly = true;
            if (ticket.IsPersistent)
            {
                cookie.Expires = ticket.Expiration;
            }
            cookie.Secure = FormsAuthentication.RequireSSL;
            cookie.Path = FormsAuthentication.FormsCookiePath;
            if (FormsAuthentication.CookieDomain != null)
            {
                cookie.Domain = FormsAuthentication.CookieDomain;
            }

            _httpContext.Response.Cookies.Add(cookie);
            _cachedCustomer = customer;
        }

        /// <summary>
        /// Sign out
        /// </summary>
        public virtual void SignOut()
        {
            _cachedCustomer = null;
            _httpContext.Response.Cookies.Remove(_bonusAppSettings.AuthCookieName);
        }

        /// <summary>
        /// Get authenticated customer
        /// </summary>
        /// <returns>Customer</returns>
        public virtual BonusApp_Customer GetAuthenticatedCustomer()
        {
            if (_cachedCustomer != null)
                return _cachedCustomer;

            // _httpContext.User 直销系统使用
            if (_httpContext == null ||
                _httpContext.Request == null 
                // || !_httpContext.Request.IsAuthenticated
                // || !(_httpContext.User.Identity is FormsIdentity)
                )
            {
                return null;
            }

            var ticket = ExtractTicketFromCookie(_httpContext, _bonusAppSettings.AuthCookieName);

            // See if the ticket was created: No => exit immediately
            if (ticket == null || ticket.Expired)
                return null;

            //var formsIdentity = (FormsIdentity)_httpContext.User.Identity;
            //var customer = GetAuthenticatedCustomerFromTicket(formsIdentity.Ticket);
            var customer = GetAuthenticatedCustomerFromTicket(ticket);
            if (customer != null && customer.Active && !customer.Deleted /*&& customer.IsRegistered()*/)
                _cachedCustomer = customer;
            return _cachedCustomer;
        }

        private FormsAuthenticationTicket ExtractTicketFromCookie(HttpContextBase context, String name)
        {
            FormsAuthenticationTicket ticket = null;
            string encValue = null;

            try
            {
                ////////////////////////////////////////////////////////////
                // Step 1: Check URI/cookie for ticket
                HttpCookie cookie = context.Request.Cookies[name];
                if (cookie != null)
                {
                    encValue = cookie.Value;
                }

                ////////////////////////////////////////////////////////////
                // Step 2: Decrypt encrypted ticket
                if (encValue != null && encValue.Length > 1)
                {
                    try
                    {
                        ticket = FormsAuthentication.Decrypt(encValue);
                    }
                    catch
                    {
                        // badTicket
                        context.Request.Cookies.Remove(name);
                    }

                    return ticket;
                }
            }
            catch
            {
                return null;
            }
        
            return ticket;
        }

        #endregion
    }
}