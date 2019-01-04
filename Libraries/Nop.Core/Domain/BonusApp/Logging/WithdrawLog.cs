using System;
using Nop.Core.Domain.BonusApp.Customers;

namespace Nop.Core.Domain.BonusApp.Logging
{
    /// <summary>
    /// 提现记录
    /// </summary>
    public partial class BonusApp_WithdrawLog : BaseEntity
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
        /// Gets or sets the withdraw amount
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the withdraw status
        /// </summary>
        public bool IsDone { get; set; }
        
        /// <summary>
        /// Gets or sets the date and time of instance creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the date and time when this work is done.
        /// </summary>
        public DateTime? CompleteOnUtc { get; set; }

        /// <summary>
        /// Gets the customer
        /// </summary>
        public virtual BonusApp_Customer Customer { get; set; }

        /// <summary>
        /// Gets or sets the ip address
        /// </summary>
        public virtual string IpAddress { get; set; }
    }
}
