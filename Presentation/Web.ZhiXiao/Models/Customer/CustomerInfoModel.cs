using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation.Attributes;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.ZhiXiao;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using Nop.Web.Validators.Customer;

namespace Nop.Models.Customers
{
    [Validator(typeof(CustomerInfoValidator))]
    public partial class CustomerInfoModel : BaseNopModel
    {
        public CustomerInfoModel()
        {
            this.AvailableTimeZones = new List<SelectListItem>();
            this.AvailableCountries = new List<SelectListItem>();
            this.AvailableStates = new List<SelectListItem>();
            this.AssociatedExternalAuthRecords = new List<AssociatedExternalAuthModel>();
            this.CustomerAttributes = new List<CustomerAttributeModel>();
        }

        [NopResourceDisplayName("Account.Fields.Email")]
        [AllowHtml]
        public string Email { get; set; }
        [NopResourceDisplayName("Account.Fields.EmailToRevalidate")]
        public string EmailToRevalidate { get; set; }

        public bool CheckUsernameAvailabilityEnabled { get; set; }
        public bool AllowUsersToChangeUsernames { get; set; }
        public bool UsernamesEnabled { get; set; }
        [NopResourceDisplayName("Account.Fields.Username")]
        [AllowHtml]
        public string Username { get; set; }

        [NopResourceDisplayName("Admin.Customers.Customers.Fields.NickName")]
        public string NickName { get; set; }

        //form fields & properties
        public bool GenderEnabled { get; set; }
        [NopResourceDisplayName("Account.Fields.Gender")]
        public string Gender { get; set; }

        [NopResourceDisplayName("Account.Fields.FirstName")]
        [AllowHtml]
        public string FirstName { get; set; }
        [NopResourceDisplayName("Account.Fields.LastName")]
        [AllowHtml]
        public string LastName { get; set; }


        public bool DateOfBirthEnabled { get; set; }
        [UIHint("DateNullable")]
        [NopResourceDisplayName("Admin.Customers.Customers.Fields.DateOfBirth")]
        public DateTime? DateOfBirth { get; set; }

        [NopResourceDisplayName("Account.Fields.DateOfBirth")]
        public int? DateOfBirthDay { get; set; }
        [NopResourceDisplayName("Account.Fields.DateOfBirth")]
        public int? DateOfBirthMonth { get; set; }
        [NopResourceDisplayName("Account.Fields.DateOfBirth")]
        public int? DateOfBirthYear { get; set; }
        public bool DateOfBirthRequired { get; set; }
        public DateTime? ParseDateOfBirth()
        {
            if (!DateOfBirthYear.HasValue || !DateOfBirthMonth.HasValue || !DateOfBirthDay.HasValue)
                return null;

            DateTime? dateOfBirth = null;
            try
            {
                dateOfBirth = new DateTime(DateOfBirthYear.Value, DateOfBirthMonth.Value, DateOfBirthDay.Value);
            }
            catch { }
            return dateOfBirth;
        }

        public bool CompanyEnabled { get; set; }
        public bool CompanyRequired { get; set; }
        [NopResourceDisplayName("Account.Fields.Company")]
        [AllowHtml]
        public string Company { get; set; }

        public bool StreetAddressEnabled { get; set; }
        public bool StreetAddressRequired { get; set; }
        [NopResourceDisplayName("Account.Fields.StreetAddress")]
        [AllowHtml]
        public string StreetAddress { get; set; }

        public bool StreetAddress2Enabled { get; set; }
        public bool StreetAddress2Required { get; set; }
        [NopResourceDisplayName("Account.Fields.StreetAddress2")]
        [AllowHtml]
        public string StreetAddress2 { get; set; }

        public bool ZipPostalCodeEnabled { get; set; }
        public bool ZipPostalCodeRequired { get; set; }
        [NopResourceDisplayName("Account.Fields.ZipPostalCode")]
        [AllowHtml]
        public string ZipPostalCode { get; set; }

        [NopResourceDisplayName("Admin.Customers.Customers.Fields.District")]
        public string District { get; set; }

        public bool CityEnabled { get; set; }
        public bool CityRequired { get; set; }
        [NopResourceDisplayName("Account.Fields.City")]
        [AllowHtml]
        public string City { get; set; }

        public bool CountryEnabled { get; set; }
        public bool CountryRequired { get; set; }
        [NopResourceDisplayName("Account.Fields.Country")]
        public int CountryId { get; set; }
        public IList<SelectListItem> AvailableCountries { get; set; }

        [NopResourceDisplayName("Admin.Customers.Customers.Fields.StateProvince")] 
        public string StateProvince { get; set; }
        public bool StateProvinceEnabled { get; set; }
        public bool StateProvinceRequired { get; set; }
        //[NopResourceDisplayName("Account.Fields.StateProvince")]
        //public int StateProvinceId { get; set; }
        public IList<SelectListItem> AvailableStates { get; set; }

        public bool PhoneEnabled { get; set; }
        public bool PhoneRequired { get; set; }
        [NopResourceDisplayName("Account.Fields.Phone")]
        [AllowHtml]
        public string Phone { get; set; }

        public bool FaxEnabled { get; set; }
        public bool FaxRequired { get; set; }
        [NopResourceDisplayName("Account.Fields.Fax")]
        [AllowHtml]
        public string Fax { get; set; }

        public bool NewsletterEnabled { get; set; }
        [NopResourceDisplayName("Account.Fields.Newsletter")]
        public bool Newsletter { get; set; }

        //preferences
        public bool SignatureEnabled { get; set; }
        [NopResourceDisplayName("Account.Fields.Signature")]
        [AllowHtml]
        public string Signature { get; set; }

        //time zone
        [NopResourceDisplayName("Account.Fields.TimeZone")]
        public string TimeZoneId { get; set; }
        public bool AllowCustomersToSetTimeZone { get; set; }
        public IList<SelectListItem> AvailableTimeZones { get; set; }

        //EU VAT
        [NopResourceDisplayName("Account.Fields.VatNumber")]
        [AllowHtml]
        public string VatNumber { get; set; }
        public string VatNumberStatusNote { get; set; }
        public bool DisplayVatNumber { get; set; }

        //external authentication
        [NopResourceDisplayName("Account.AssociatedExternalAuth")]
        public IList<AssociatedExternalAuthModel> AssociatedExternalAuthRecords { get; set; }
        public int NumberOfExternalAuthenticationProviders { get; set; }

        public IList<CustomerAttributeModel> CustomerAttributes { get; set; }

        
        //registration date
        [NopResourceDisplayName("Admin.Customers.Customers.ActivityLog.CreatedOn")]
        public DateTime CreatedOn { get; set; }
        [NopResourceDisplayName("Admin.Customers.Customers.Fields.LastActivityDate")]
        public DateTime LastActivityDate { get; set; }

        #region 直销系统需要的字段

        [Display(Name = "推荐人")]
        [AllowHtml]
        [UIHint("MultiSelect")]
        public int ZhiXiao_ParentId { get; set; }       // 推荐人id
        public Customer ParentUser { get; set; }
        public List<SelectListItem> AvailableParents { get; set; }
        
        [Display(Name = "小组")]
        public int ZhiXiao_TeamId { get; set; }       // 小组id
        public CustomerTeam CustomerTeam { get; set; }

        public string ZhiXiao_InTeamOrder { get; set; }  // 在小组中顺序
        public string ZhiXiao_InTeamTime { get; set; }   // 进入该小组时间
        public string ZhiXiao_ChildCount { get; set; }   // 下线个数
        [Display(Name = "用户级别")]
        public int ZhiXiao_LevelId { get; set; }        // 级别
        [Display(Name = "电子币")]
        public long ZhiXiao_MoneyNum { get; set; }       // 电子币(真是个数)
        [Display(Name = "历史电子币")]
        public long ZhiXiao_MoneyHistory { get; set; }   // 历史电子币(只记录增加, 不记录减少)

        [Display(Name = "身份证号")]
        public string ZhiXiao_IdCardNum { get; set; }       // 身份证号
        [Display(Name = "开户银行")]
        public string ZhiXiao_YinHang { get; set; }         // 银行
        [Display(Name = "开户行名称")]
        public string ZhiXiao_KaiHuHang { get; set; }       // 开户行
        [Display(Name = "开户名")]
        public string ZhiXiao_KaiHuMing { get; set; }       // 开户名
        [Display(Name = "银行卡号")]
        public string ZhiXiao_BankNum { get; set; }         // 银行卡号
        [Display(Name = "发货状态")]
        public int ProductStatusId { get; set; }

        [Display(Name = "级别")]
        public CustomerLevel CustomerLevel
        {
            get
            {
                return (CustomerLevel)this.ZhiXiao_LevelId;
            }
            set
            {
                this.ZhiXiao_LevelId = (int)value;
            }
        }

        public SendProductStatus ProductStatus
        {
            get
            {
                return (SendProductStatus)this.ProductStatusId;
            }
            set
            {
                this.ProductStatusId = (int)value;
            }
        }

        #endregion

        #region Nested classes

        public partial class AssociatedExternalAuthModel : BaseNopEntityModel
        {
            public string Email { get; set; }

            public string ExternalIdentifier { get; set; }

            public string AuthMethodName { get; set; }
        }
        
        #endregion
    }
}