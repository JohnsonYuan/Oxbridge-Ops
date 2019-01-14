using System;
using Nop.Core.Domain.BonusApp.Customers;

namespace Nop.Core.Domain.BonusApp.Logging
{
    /// <summary>
    /// Represents an customer money log record
    /// </summary>
    public class BonusApp_MoneyLog : BaseEntity
    {
        /// <summary>
        /// Gets or sets the customer identifier
        /// </summary>
        public int CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the activity comment
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }
        public DateTime? CompleteOnUtc { get; set; }

        public string IpAddress { get; set; }

#region Bonus info

        // 用户消费金额
        public decimal Money { get; set; }
        // 返还用户金额(目前设置为 用户消费金额)
        public decimal ReturnMoney { get; set; }
        // 返还状态
        public int MoneyReturnStatusId { get; set; }

        /// <summary>
        /// 奖金池改变前金额
        /// </summary>
        public decimal AppMoneyBefore { get; set; }

        /// <summary>
        /// 奖金池增加金额 Money * 20%
        /// </summary>
        public decimal AppMoneyDelta { get; set; }

        /// <summary>
        /// 奖金池改变后金额 AppMoneyBefore + AppMoneyDelta
        /// </summary>
        public decimal AppMoneyAfter { get; set; }

#endregion

        /// <summary>
        /// Gets the 返还状态
        /// </summary>
        public BonusApp_MoneyReturnStatus MoneyReturnStatus
        {
            get
            {
                return (BonusApp_MoneyReturnStatus)MoneyReturnStatusId;
            }
            set
            {
                this.MoneyReturnStatusId = (int)value;
            }
        }

        public virtual BonusApp_Customer Customer { get; set; }
    }
}
