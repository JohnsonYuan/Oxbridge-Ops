using System;
using FluentValidation;
using FluentValidation.Results;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using Nop.Models.Customers;

namespace Nop.Web.Validators.Customer
{
    public partial class CustomerInfoValidator : BaseNopValidator<CustomerInfoModel>
    {
        public CustomerInfoValidator(ILocalizationService localizationService,
            CustomerSettings customerSettings)
        {
            //RuleFor(x => x.Email).NotEmpty().WithMessage(localizationService.GetResource("Account.Fields.Email.Required"));
            //RuleFor(x => x.Email).EmailAddress().WithMessage(localizationService.GetResource("Common.WrongEmail"));
            //RuleFor(x => x.FirstName).NotEmpty().WithMessage(localizationService.GetResource("Account.Fields.FirstName.Required"));
            //RuleFor(x => x.LastName).NotEmpty().WithMessage(localizationService.GetResource("Account.Fields.LastName.Required"));

            //if (customerSettings.UsernamesEnabled && customerSettings.AllowUsersToChangeUsernames)
            //{
            RuleFor(x => x.Username).NotEmpty().WithMessage(localizationService.GetResource("Account.Fields.Username.Required"));
            //}

            //form fields
            if (/*customerSettings.CountryEnabled &&*/
                customerSettings.StateProvinceEnabled &&
                customerSettings.StateProvinceRequired)
            {
                //RuleFor(x => x.CountryId)
                //    .NotEqual(0)
                //    .WithMessage(localizationService.GetResource("Account.Fields.Country.Required"));
                RuleFor(x => x.StateProvince).NotEmpty()
                    .WithMessage(localizationService.GetResource("Admin.Customers.Customers.Fields.StateProvince.Required"));
            }
            if (customerSettings.DateOfBirthEnabled && customerSettings.DateOfBirthRequired)
            {
                Custom(x =>
                {
                    var dateOfBirth = x.ParseDateOfBirth();
                    //entered?
                    if (!dateOfBirth.HasValue)
                    {
                        return new ValidationFailure("DateOfBirthDay", localizationService.GetResource("Account.Fields.DateOfBirth.Required"));
                    }
                    //minimum age
                    if (customerSettings.DateOfBirthMinimumAge.HasValue &&
                        CommonHelper.GetDifferenceInYears(dateOfBirth.Value, DateTime.Today) < customerSettings.DateOfBirthMinimumAge.Value)
                    {
                        return new ValidationFailure("DateOfBirthDay", string.Format(localizationService.GetResource("Account.Fields.DateOfBirth.MinimumAge"), customerSettings.DateOfBirthMinimumAge.Value));
                    }
                    return null;
                });
            }
            if (customerSettings.CompanyRequired && customerSettings.CompanyEnabled)
            {
                RuleFor(x => x.Company).NotEmpty().WithMessage(localizationService.GetResource("Account.Fields.Company.Required"));
            }
            if (customerSettings.StreetAddressRequired && customerSettings.StreetAddressEnabled)
            {
                RuleFor(x => x.StreetAddress).NotEmpty().WithMessage(localizationService.GetResource("Account.Fields.StreetAddress.Required"));
            }
            if (customerSettings.StreetAddress2Required && customerSettings.StreetAddress2Enabled)
            {
                RuleFor(x => x.StreetAddress2).NotEmpty().WithMessage(localizationService.GetResource("Account.Fields.StreetAddress2.Required"));
            }
            if (customerSettings.ZipPostalCodeRequired && customerSettings.ZipPostalCodeEnabled)
            {
                RuleFor(x => x.ZipPostalCode).NotEmpty().WithMessage(localizationService.GetResource("Account.Fields.ZipPostalCode.Required"));
            }
            if (customerSettings.CityRequired && customerSettings.CityEnabled)
            {
                RuleFor(x => x.City).NotEmpty().WithMessage(localizationService.GetResource("Account.Fields.City.Required"));

                RuleFor(x => x.District).NotEmpty()
                    .WithMessage(localizationService.GetResource("Admin.Customers.Customers.Fields.District.Required"));
            }
            if (customerSettings.PhoneRequired && customerSettings.PhoneEnabled)
            {
                RuleFor(x => x.Phone)
                    .NotEmpty()
                    .WithMessage(localizationService.GetResource("Admin.Customers.Customers.Fields.Phone.Required"));

                Custom(x =>
                {
                    bool match = true;
                    if (!string.IsNullOrEmpty(customerSettings.PhoneNumberRegex))
                        match = System.Text.RegularExpressions.Regex.IsMatch(x.Phone, customerSettings.PhoneNumberRegex);
                    //if yes, then ensure that a state is selected
                    if (!match)
                    {
                        return new ValidationFailure("Phone", localizationService.GetResource("Account.Fields.Phone.FormatWrong"));
                    }

                    return null;
                });
            }

            if (customerSettings.FaxRequired && customerSettings.FaxEnabled)
            {
                RuleFor(x => x.Fax).NotEmpty().WithMessage(localizationService.GetResource("Account.Fields.Fax.Required"));
            }
        }
    }
}