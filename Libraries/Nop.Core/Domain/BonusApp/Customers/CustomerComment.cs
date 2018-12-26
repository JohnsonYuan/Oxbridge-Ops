using System;
using System.ComponentModel;

namespace Nop.Core.Domain.BonusApp.Customers
{
    public class BonusApp_CustomerComment : BaseEntity
    {
        /// <summary>
        /// Gets or sets the customer identifier
        /// </summary>
        public int CustomerId { get; set; }

        public string Comment { get; set; }

        public int Rate { get; set; }   // 1 - 5

        [DefaultValue(true)]
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the ip address
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets the customer
        /// </summary>
        public virtual BonusApp_Customer Customer { get; set; }
    }
}
