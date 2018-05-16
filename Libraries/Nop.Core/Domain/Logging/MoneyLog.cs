using System;
using Nop.Core.Domain.Customers;

namespace Nop.Core.Domain.Logging
{
    /// <summary>
    /// Represents an customer money log record
    /// </summary>
    public class MoneyLog : BaseEntity
    {
        /// <summary>
        /// Gets or sets the activity log type identifier
        /// </summary>
        public int ActivityLogTypeId { get; set; }

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

        /// <summary>
        /// Gets the activity log type
        /// </summary>
        public virtual ActivityLogType ActivityLogType { get; set; }

        /// <summary>
        /// Gets the customer
        /// </summary>
        public virtual Customer Customer { get; set; }

        /// <summary>
        /// Gets or sets the ip address
        /// </summary>
        public virtual string IpAddress { get; set; }

        /// <summary>
        /// 改变前金额
        /// </summary>
        public long MoneyBefore { get; set; }

        /// <summary>
        /// 变化金额
        /// </summary>
        public long MoneyDelta { get; set; }

        /// <summary>
        /// 改变后金额 MoneyBefore + MoneyDelta
        /// </summary>
        public long MoneyAfter { get; set; }
    }
}
