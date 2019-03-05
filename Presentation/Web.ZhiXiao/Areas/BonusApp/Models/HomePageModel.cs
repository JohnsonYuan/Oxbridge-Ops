using Nop.Core.Domain.BonusApp;
using Nop.Core.Domain.BonusApp.Logging;
using Nop.Web.Framework.Mvc;

namespace Web.ZhiXiao.Areas.BonusApp.Models
{
    public partial class HomePageModel
    {
        public string CurrentUserName { get; set; }

        /// <summary>
        /// 当前用户在派对中队列中排名(null为不在派对中)
        /// </summary>
        public int? WaitingLogOrderNum { get; set; }

        /// <summary>
        /// 首页最上方提示最近成功的记录
        /// </summary>
        public string LastSuccessLogMessage { get; set; }

        public BonusAppStatus BonusAppStatus { get; set; }

        /// <summary>
        /// Money return status
        /// </summary>
        public BonusApp_MoneyReturnStatus Status { get; set; }

        /// <summary>
        /// pool items
        /// </summary>
        public PoolItemListModel PoolItems { get; set; }
    }

    public class PoolItemListModel : BasePagedListModel<PoolItemModel>
    {

    }

    public class PoolItemModel : BaseNopEntityModel
    {
        // 第78位 
        public int OrderNumber { get; set; }

        public string CustomerAvatar { get; set; }
        public string CustomerName { get; set; }

        public double Money { get; set; }

        public string DisplayDateTime { get; set; }
    }
}