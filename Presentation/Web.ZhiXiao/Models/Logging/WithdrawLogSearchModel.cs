using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Admin.Models.Logging
{
    public partial class WithdrawLogSearchModel : BaseNopEntityModel
    {
        public WithdrawLogSearchModel()
        {
            LogStatus = new List<SelectListItem>();
            
            LogStatus.Add(new SelectListItem
                {
                    Text = "全部",
                    Value = null,
                    Selected = true
                });
            LogStatus.Add(new SelectListItem
                {
                    Text = "未处理",
                    Value = "false"
                });

            LogStatus.Add(new SelectListItem
                {
                    Text = "已处理",
                    Value = "true"
                });
        }
        
        [Display(Name = "状态")]
        public bool? IsDone { get; set; }
        public IList<SelectListItem> LogStatus { get; set; }

        [NopResourceDisplayName("Admin.Configuration.ActivityLog.ActivityLog.Fields.CreatedOnFrom")]
        [UIHint("DateNullable")]
        public DateTime? CreatedOnFrom { get; set; }

        [NopResourceDisplayName("Admin.Configuration.ActivityLog.ActivityLog.Fields.CreatedOnTo")]
        [UIHint("DateNullable")]
        public DateTime? CreatedOnTo { get; set; }
    }
}