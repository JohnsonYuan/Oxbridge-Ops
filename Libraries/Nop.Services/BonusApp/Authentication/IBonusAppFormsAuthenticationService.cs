using System;
using Nop.Core.Domain.BonusApp.Customers;

namespace Nop.Services.BonusApp.Authentication
{
    /// <summary>
    /// Bonus app Authentication service
    /// </summary>
    public interface IBonusAppFormsAuthenticationService
    {
        /// <summary>
        /// Sign in
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="createPersistentCookie">A value indicating whether to create a persistent cookie</param>
        void SignIn(BonusApp_Customer customer, bool createPersistentCookie);

        /// <summary>
        /// Sign out
        /// </summary>
        void SignOut();

        /// <summary>
        /// Get authenticated customer
        /// </summary>
        /// <returns>Customer</returns>
        BonusApp_Customer GetAuthenticatedCustomer();
    }
}