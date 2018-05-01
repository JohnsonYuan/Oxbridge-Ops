using System;
using System.Collections.Generic;
using Nop.Core.Domain.Customers;

namespace Nop.Core.Domain.ZhiXiao
{
    /// <summary>
    /// Represents a customer
    /// </summary>
    public partial class CustomerTeam : BaseEntity
    {
        private ICollection<Customer> _customers;

        /// <summary>
        /// Ctor
        /// </summary>
        public CustomerTeam()
        {
            this.TeamGuid = Guid.NewGuid();
        }

        /// <summary>
        /// Gets or sets the customer team number
        /// available placehodler: {ID} {YYYY} {YY} {MM} {DD}
        /// </summary>
        public string CustomNumber { get; set; }

        /// <summary>
        /// Gets or sets the customer Guid
        /// </summary>
        public Guid TeamGuid { get; set; }

        /// <summary>
        /// Gets or sets the team user count
        /// </summary>
        public int UserCount { get; set; }

        /// <summary>
        /// Gets or sets the date and time of entity creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets customer addresses
        /// </summary>
        public virtual ICollection<Customer> Customers
        {
            get { return _customers ?? (_customers = new List<Customer>()); }
            protected set { _customers = value; }
        }
    }
}
