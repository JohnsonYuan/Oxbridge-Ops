using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Models.Customers
{
    public class CustomerTeamSearchModel : BaseNopModel
    {
        public CustomerTeamSearchModel()
        {
            AvailableTeamTypes = new List<SelectListItem>();
        }

        [Display(Name = "小组编号")]
        public string SearchTeamNumber { get; set; }

        [Display(Name = "小组类型")]
        public int SearchTeamType { get; set; }

        [NopResourceDisplayName("Admin.Configuration.ActivityLog.ActivityLog.Fields.CreatedOnFrom")]
        [UIHint("DateNullable")]
        public DateTime? CreatedOnFrom { get; set; }

        [NopResourceDisplayName("Admin.Configuration.ActivityLog.ActivityLog.Fields.CreatedOnTo")]
        [UIHint("DateNullable")]
        public DateTime? CreatedOnTo { get; set; }

        
        public IList<SelectListItem> AvailableTeamTypes { get; set; }
    }
}