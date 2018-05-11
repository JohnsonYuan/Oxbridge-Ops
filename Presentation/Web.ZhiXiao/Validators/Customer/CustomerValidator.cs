using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using Nop.Core.Domain.Customers;
using Nop.Data;
using Nop.Models.Customers;
using Nop.Services.Customers;
//using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Validators.Customers
{
    public partial class CustomerValidator : BaseNopValidator<CustomerModel>
    {
        public CustomerValidator(ILocalizationService localizationService,
            //IStateProvinceService stateProvinceService,
            ICustomerService customerService,
            CustomerSettings customerSettings,
            IDbContext dbContext)
        {
            //if (customerSettings.UsernamesEnabled)
            //{
            RuleFor(x => x.Username).NotEmpty().WithMessage(localizationService.GetResource("Account.Fields.Username.Required"));
            //}

            RuleFor(x => x.NickName).NotEmpty().WithMessage(localizationService.GetResource("Admin.Customers.Customers.Fields.NickName.Required"));

            // 当编辑用户时不需要输入密码
            RuleFor(x => x.Password).NotEmpty().WithMessage(localizationService.GetResource("Account.Fields.Password.Required"))
                .When(x => x.Id == 0);
            RuleFor(x => x.Password).Length(customerSettings.PasswordMinLength, 999).WithMessage(string.Format(localizationService.GetResource("Account.Fields.Password.LengthValidation"), customerSettings.PasswordMinLength))
                .When(x => x.Id == 0);

            #region 直销系统用户注册需要的字段
            
            // 密码确认
            RuleFor(x => x.ConfirmPassword).NotEmpty().WithMessage(localizationService.GetResource("Account.Fields.ConfirmPassword.Required"))
                .When(x => IsRegisteredCustomerRoleChecked(x, customerService));
            RuleFor(x => x.ConfirmPassword).Equal(x => x.Password).WithMessage(localizationService.GetResource("Account.Fields.Password.EnteredPasswordsDoNotMatch"))
                .When(x => IsRegisteredCustomerRoleChecked(x, customerService));
            
            // 二级密码
            RuleFor(x => x.Password2).NotEmpty().WithMessage(localizationService.GetResource("Account.Fields.Password2.Required"))
                .When(x => IsRegisteredCustomerRoleChecked(x, customerService));;
            RuleFor(x => x.Password2).Length(customerSettings.PasswordMinLength, 999).WithMessage(string.Format(localizationService.GetResource("Account.Fields.Password2.LengthValidation"), customerSettings.PasswordMinLength))
                .When(x => IsRegisteredCustomerRoleChecked(x, customerService));;
            RuleFor(x => x.ConfirmPassword2).NotEmpty().WithMessage(localizationService.GetResource("Account.Fields.ConfirmPassword2.Required"))
                .When(x => IsRegisteredCustomerRoleChecked(x, customerService));;
            RuleFor(x => x.ConfirmPassword2).Equal(x => x.Password2).WithMessage(localizationService.GetResource("Account.Fields.Password2.EnteredPasswordsDoNotMatch"))
                .When(x => IsRegisteredCustomerRoleChecked(x, customerService));;

            RuleFor(x => x.ZhiXiao_IdCardNum).NotEmpty()
                .When(x => IsRegisteredCustomerRoleChecked(x, customerService));;
            RuleFor(x => x.ZhiXiao_YinHang).NotEmpty()
                .When(x => IsRegisteredCustomerRoleChecked(x, customerService));;
            RuleFor(x => x.ZhiXiao_KaiHuHang).NotEmpty()
                .When(x => IsRegisteredCustomerRoleChecked(x, customerService));;
            RuleFor(x => x.ZhiXiao_KaiHuMing).NotEmpty()
                .When(x => IsRegisteredCustomerRoleChecked(x, customerService));;
            RuleFor(x => x.ZhiXiao_BandNum).NotEmpty()
                .When(x => IsRegisteredCustomerRoleChecked(x, customerService));;

            #endregion

            //ensure that valid email address is entered if Registered role is checked to avoid registered customers with empty email address
            //RuleFor(x => x.Email)
            //    .NotEmpty()
            //    .EmailAddress()
            //    //.WithMessage("Valid Email is required for customer to be in 'Registered' role")
            //    .WithMessage(localizationService.GetResource("Admin.Common.WrongEmail"))
            //    //only for registered users
            //    .When(x => IsRegisteredCustomerRoleChecked(x, customerService));

            //form fields
            //if (customerSettings.CountryEnabled && customerSettings.CountryRequired)
            //{
            //    RuleFor(x => x.CountryId)
            //        .NotEqual(0)
            //        .WithMessage(localizationService.GetResource("Account.Fields.Country.Required"))
            //        //only for registered users
            //        .When(x => IsRegisteredCustomerRoleChecked(x, customerService));
            //}
            if (/*customerSettings.CountryEnabled &&*/
                customerSettings.StateProvinceEnabled &&
                customerSettings.StateProvinceRequired)
            {
                RuleFor(x => x.StateProvince).NotEmpty()
                    .WithMessage(localizationService.GetResource("Admin.Customers.Customers.Fields.StateProvince.Required"))
                    //only for registered users
                    .When(x => IsRegisteredCustomerRoleChecked(x, customerService));;
            }
            if (customerSettings.CityRequired && customerSettings.CityEnabled)
            {
                RuleFor(x => x.City)
                    .NotEmpty()
                    .WithMessage(localizationService.GetResource("Admin.Customers.Customers.Fields.City.Required"))
                    //only for registered users
                    .When(x => IsRegisteredCustomerRoleChecked(x, customerService));

                RuleFor(x => x.District).NotEmpty()
                    .WithMessage(localizationService.GetResource("Admin.Customers.Customers.Fields.District.Required"))
                    //only for registered users
                    .When(x => IsRegisteredCustomerRoleChecked(x, customerService));;
            }
            if (customerSettings.CompanyRequired && customerSettings.CompanyEnabled)
            {
                //RuleFor(x => x.Company)
                //    .NotEmpty()
                //    .WithMessage(localizationService.GetResource("Admin.Customers.Customers.Fields.Company.Required"))
                //    //only for registered users
                //    .When(x => IsRegisteredCustomerRoleChecked(x, customerService));
            }
            if (customerSettings.StreetAddressRequired && customerSettings.StreetAddressEnabled)
            {
                RuleFor(x => x.StreetAddress)
                    .NotEmpty()
                    .WithMessage(localizationService.GetResource("Admin.Customers.Customers.Fields.StreetAddress.Required"))
                    //only for registered users
                    .When(x => IsRegisteredCustomerRoleChecked(x, customerService));
            }
            if (customerSettings.StreetAddress2Required && customerSettings.StreetAddress2Enabled)
            {
                //RuleFor(x => x.StreetAddress2)
                //    .NotEmpty()
                //    .WithMessage(localizationService.GetResource("Admin.Customers.Customers.Fields.StreetAddress2.Required"))
                //    //only for registered users
                //    .When(x => IsRegisteredCustomerRoleChecked(x, customerService));
            }
            if (customerSettings.ZipPostalCodeRequired && customerSettings.ZipPostalCodeEnabled)
            {
                //RuleFor(x => x.ZipPostalCode)
                //    .NotEmpty()
                //    .WithMessage(localizationService.GetResource("Admin.Customers.Customers.Fields.ZipPostalCode.Required"))
                //    //only for registered users
                //    .When(x => IsRegisteredCustomerRoleChecked(x, customerService));
            }
            if (customerSettings.PhoneRequired && customerSettings.PhoneEnabled)
            {
                RuleFor(x => x.Phone)
                    .NotEmpty()
                    .WithMessage(localizationService.GetResource("Admin.Customers.Customers.Fields.Phone.Required"))
                    //only for registered users
                    .When(x => IsRegisteredCustomerRoleChecked(x, customerService));

                Custom(x =>
                {
                    bool isInRegisteredRole = IsRegisteredCustomerRoleChecked(x, customerService);
                    if (isInRegisteredRole)
                    {
                        bool match = true;
                        if (!string.IsNullOrEmpty(customerSettings.PhoneNumberRegex))
                            match = System.Text.RegularExpressions.Regex.IsMatch(x.Phone, customerSettings.PhoneNumberRegex);
                        //if yes, then ensure that a state is selected
                        if (!match)
                        {
                            return new ValidationFailure("Phone", localizationService.GetResource("Account.Fields.Phone.FormatWrong"));
                        }
                    }
                    return null;
                });
            }
            if (customerSettings.FaxRequired && customerSettings.FaxEnabled)
            {
                //RuleFor(x => x.Fax)
                //    .NotEmpty()
                //    .WithMessage(localizationService.GetResource("Admin.Customers.Customers.Fields.Fax.Required"))
                //    //only for registered users
                //    .When(x => IsRegisteredCustomerRoleChecked(x, customerService));
            }

            SetDatabaseValidationRules<Customer>(dbContext);
        }

        /// <summary>
        /// 用户类型是普通或者高级是需要输入直销系统相关信息
        /// </summary>
        /// <param name="model"></param>
        /// <param name="customerService"></param>
        /// <returns></returns>
        private bool IsRegisteredCustomerRoleChecked(CustomerModel model, ICustomerService customerService)
        {
            var allCustomerRoles = customerService.GetAllCustomerRoles(true);
            var newCustomerRoles = new List<CustomerRole>();
            foreach (var customerRole in allCustomerRoles)
                if (model.SelectedCustomerRoleIds.Contains(customerRole.Id))
                    newCustomerRoles.Add(customerRole);

            bool isManager = newCustomerRoles.FirstOrDefault(cr => 
                cr.SystemName == SystemCustomerRoleNames.Administrators ||
                cr.SystemName == SystemCustomerRoleNames.Managers ) != null;

            // 管理员不需要验证这些信息
            if (isManager)
                return false;

            bool isInRegisteredRole = newCustomerRoles.FirstOrDefault(cr => 
                cr.SystemName == SystemCustomerRoleNames.Registered ||
                cr.SystemName == SystemCustomerRoleNames.Registered_Advanced ) != null;
            return isInRegisteredRole;
        }
    }
}