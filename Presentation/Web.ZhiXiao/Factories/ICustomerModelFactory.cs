using Nop.Core.Domain.Customers;
using Nop.Models.Customers;

namespace Web.ZhiXiao.Factories
{
    /// <summary>
    /// Represents the interface of the customer model factory
    /// </summary>
    public partial interface ICustomerModelFactory
    {
        /// <summary>
        /// Prepare the login model
        /// </summary>
        /// <param name="checkoutAsGuest">Whether to checkout as guest is enabled</param>
        /// <returns>Login model</returns>
        LoginModel PrepareLoginModel(bool? checkoutAsGuest);

        /// <summary>
        /// Prepare the customer info model
        /// </summary>
        /// <param name="model">Customer info model</param>
        /// <param name="customer">Customer</param>
        /// <param name="excludeProperties">Whether to exclude populating of model properties from the entity</param>
        /// <param name="overrideCustomCustomerAttributesXml">Overridden customer attributes in XML format; pass null to use CustomCustomerAttributes of customer</param>
        /// <returns>Customer info model</returns>
        CustomerInfoModel PrepareCustomerInfoModel(CustomerInfoModel model, Customer customer,
            bool excludeProperties, string overrideCustomCustomerAttributesXml = "");

        /// <summary>
        /// Prepare the change password model
        /// </summary>
        /// <returns>Change password model</returns>
        ChangePasswordModel PrepareChangePasswordModel();
    }
}
