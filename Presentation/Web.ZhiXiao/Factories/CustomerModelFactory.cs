using Nop.Core.Domain.Customers;
using Nop.Web.Models.Customer;

namespace Web.ZhiXiao.Factories
{
    /// <summary>
    /// Represents the customer model factory
    /// </summary>
    public class CustomerModelFactory : ICustomerModelFactory
    {
        #region Fields

        private readonly CustomerSettings _customerSettings;

        #endregion

        #region Ctor

        public CustomerModelFactory(CustomerSettings customerSettings)
        {
            this._customerSettings = customerSettings;
        }

        #endregion

        /// <summary>
        /// Prepare the login model
        /// </summary>
        /// <param name="checkoutAsGuest">Whether to checkout as guest is enabled</param>
        /// <returns>Login model</returns>
        public virtual LoginModel PrepareLoginModel(bool? checkoutAsGuest)
        {
            var model = new LoginModel();
            model.UsernamesEnabled = _customerSettings.UsernamesEnabled;
            model.CheckoutAsGuest = checkoutAsGuest.GetValueOrDefault();
            model.DisplayCaptcha = false;
            return model;
        }
    }
}