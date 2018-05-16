using Nop.Web.Framework;

namespace Nop.Admin.Models.Logging
{
    public partial class MoneyLogModel : ActivityLogModel
    {
        /// <summary>
        /// 改变前金额
        /// </summary>
        [NopResourceDisplayName("Admin.Customers.Customers.MoneyLog.MoneyBefore")]
        public long MoneyBefore { get; set; }

        /// <summary>
        /// 变化金额
        /// </summary>
        [NopResourceDisplayName("Admin.Customers.Customers.MoneyLog.MoneyDelta")]
        public long MoneyDelta { get; set; }

        /// <summary>
        /// 改变后金额 MoneyBefore + MoneyDelta
        /// </summary>
        [NopResourceDisplayName("Admin.Customers.Customers.MoneyLog.MoneyAfter")]
        public long MoneyAfter { get; set; }
    }
}
