using System;

namespace Web.ZhiXiao.Areas.BonusApp.Models.Log
{
    public class WithdrawLogModel
    {
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
        /// Gets or sets the ip address
        /// </summary>
        public virtual string IpAddress { get; set; }
    }
}