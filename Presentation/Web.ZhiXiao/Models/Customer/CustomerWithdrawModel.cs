using FluentValidation.Attributes;
using Nop.Web.Framework.Mvc;
using Nop.Web.Validators.Customer;

namespace Nop.Models.Customers
{
    [Validator(typeof(CustomerWithdrawValidator))]
    public partial class CustomerWithdrawModel : BaseNopModel
    {
        public int Amount { get; set; }
        
        public long MaxAmount { get; set; }
    }
}