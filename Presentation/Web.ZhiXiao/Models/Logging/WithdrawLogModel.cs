using System;
using System.ComponentModel.DataAnnotations;
using Nop.Core.Domain.Customers;
using Nop.Models.Customers;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Admin.Models.Logging
{
    public partial class WithdrawLogModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.Configuration.ActivityLog.ActivityLog.Fields.Customer")]
        public int CustomerId { get; set; }
        public string Username { get; set; }
        public string Nickname { get; set; }
        public long MoneyNum { get; set; }
        public CustomerModel CustomerModel { get; set; }

        [NopResourceDisplayName("Admin.Configuration.ActivityLog.ActivityLog.Fields.Comment")]
        public string Comment { get; set; }
        [Display(Name ="提现金额")]
        public int Amount { get; set; }
        [Display(Name ="状态")]
        public bool IsDone { get; set; }

        [NopResourceDisplayName("Admin.Configuration.ActivityLog.ActivityLog.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }
        public DateTime? CompleteOn { get; set; }
        [NopResourceDisplayName("Admin.Customers.Customers.ActivityLog.IpAddress")]
        public string IpAddress { get; set; }
    }
}