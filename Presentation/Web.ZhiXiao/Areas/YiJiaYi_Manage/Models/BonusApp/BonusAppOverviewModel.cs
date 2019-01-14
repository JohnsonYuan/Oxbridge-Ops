using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Nop.Core.Domain.BonusApp;
using Nop.Web.Framework;

namespace Web.ZhiXiao.Areas.YiJiaYi_Manage.Models.BonusApp
{
    public class BonusAppOverviewModel
    {
        public BonusAppStatus AppStatus { get; set; }

        public BonusAppOverviewModel()
        {
            LogStatus = new List<SelectListItem>();

            //LogStatus.Add(new SelectListItem
            //{
            //    Text = "全部",
            //    Value = null,
            //    Selected = true
            //});
            LogStatus.Add(new SelectListItem
            {
                Text = "未处理",
                Value = ((int)Nop.Core.Domain.BonusApp.Logging.BonusApp_MoneyReturnStatus.Waiting).ToString()
            });

            LogStatus.Add(new SelectListItem
            {
                Text = "已处理",
                Value = ((int)Nop.Core.Domain.BonusApp.Logging.BonusApp_MoneyReturnStatus.Complete).ToString()
            });
        }

        [NopResourceDisplayName("状态")]
        public int? LogStatusId { get; set; }

        [NopResourceDisplayName("Admin.Configuration.ActivityLog.ActivityLog.Fields.ActivityLogType")]
        public IList<SelectListItem> LogStatus { get; set; }

        [NopResourceDisplayName("Admin.Configuration.ActivityLog.ActivityLog.Fields.CreatedOnFrom")]
        [UIHint("DateNullable")]
        public DateTime? CreatedOnFrom { get; set; }

        [NopResourceDisplayName("Admin.Configuration.ActivityLog.ActivityLog.Fields.CreatedOnTo")]
        [UIHint("DateNullable")]
        public DateTime? CreatedOnTo { get; set; }

        [NopResourceDisplayName("Admin.Customers.Customers.ActivityLog.IpAddress")]
        public string IpAddress { get; set; }
    }
}