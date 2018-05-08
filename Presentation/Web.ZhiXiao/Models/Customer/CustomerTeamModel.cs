
using System;
using System.Collections.Generic;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.ZhiXiao;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Models.Customers
{
    public class CustomerTeamModel : BaseNopEntityModel
    {
        private ICollection<Customer> _customers;

        /// <summary>
        /// Ctor
        /// </summary>
        public CustomerTeamModel()
        {
        }

        /// <summary>
        /// Gets or sets the customer team number
        /// available placehodler: {ID} {YYYY} {YY} {MM} {DD}
        /// </summary>
        public string CustomNumber { get; set; }

        /// <summary>
        /// Gets or sets the team type. 
        /// </summary>
        public int TypeId { get; set; }

        /// <summary>
        /// Gets or sets the customer Guid
        /// </summary>
        public Guid TeamGuid { get; set; }

        /// <summary>
        /// Gets or sets the team user count
        /// </summary>
        public int UserCount { get; set; }

        [NopResourceDisplayName("Admin.System.Log.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        /// <summary>
        /// Gets or sets customer addresses
        /// </summary>
        public virtual ICollection<Customer> Customers
        {
            get { return _customers ?? (_customers = new List<Customer>()); }
            protected set { _customers = value; }
        }
        
        public string TeamDescription {
            get {
                return TeamType.GetDescription();
            }
        }

        /// <summary>
        /// Gets or sets the team type. 
        /// </summary>
        public CustomerTeamType TeamType
        {
            get
            {
                return (CustomerTeamType)this.TypeId;
            }
            set
            {
                this.TypeId = (int)value;
            }
        }
    }
}