using FluentValidation;
using Nop.Web.Framework.Validators;
using Nop.Web.Models.Install;

namespace Nop.Web.Validators.Install
{
    public partial class InstallValidator : BaseNopValidator<InstallModel>
    {
        public InstallValidator()
        {
            RuleFor(x => x.AdminEmail).NotEmpty();
            RuleFor(x => x.AdminEmail).EmailAddress();
            RuleFor(x => x.AdminPassword).NotEmpty();
            RuleFor(x => x.ConfirmPassword).NotEmpty();
            RuleFor(x => x.AdminPassword).Equal(x => x.ConfirmPassword);
            RuleFor(x => x.DataProvider).NotEmpty();
        }
    }
}