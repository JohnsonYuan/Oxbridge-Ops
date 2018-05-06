using FluentValidation;
using FluentValidation.Results;
using Nop.Models.Customers;
using Nop.Web.Framework.Validators;

namespace Nop.Web.Validators.Customer
{
    public partial class CustomerWithdrawValidator: BaseNopValidator<CustomerWithdrawModel>
    {
        public CustomerWithdrawValidator()
        {
            RuleFor(x => x.Amount).GreaterThan(0).WithMessage("提取金额不能小于0");

            Custom(x =>
            {
                if (x.Amount % 100 != 0)
                    return new ValidationFailure("Amount", "提取金额必须是100的整数倍");

                return null;
            });
        }
    }
}