using System.Collections.Generic;
using System.Web.Mvc;

namespace Nop.Models.Customers
{
    public partial  class UpgradeCustomerModel
    {
        public UpgradeCustomerModel()
        {
            AvailableTeams = new List<SelectListItem>();
        }
        
        /// <summary>
        /// 26800的小组
        /// </summary>
        public int SelectedTeamId { get; set; }
        public IList<SelectListItem> AvailableTeams { get; set; }
        
        /// <summary>
        /// 推荐人
        /// </summary>
        public int ParentId { get; set; }
        /// <summary>
        /// 下组可供选择的parent
        /// </summary>
        public IList<SelectListItem> AvailableParents { get; set; }
    }
}