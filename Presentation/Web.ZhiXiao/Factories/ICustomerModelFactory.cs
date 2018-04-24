using Nop.Models.Customers;

namespace Web.ZhiXiao.Factories
{
    /// <summary>
    /// Represents the interface of the customer model factory
    /// </summary>
    public partial interface ICustomerModelFactory
    {/// <summary>
     /// Prepare the login model
     /// </summary>
     /// <param name="checkoutAsGuest">Whether to checkout as guest is enabled</param>
     /// <returns>Login model</returns>
        LoginModel PrepareLoginModel(bool? checkoutAsGuest);
    }
}
