using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;

namespace Nop.Core.Domain.Orders
{
    /// <summary>
    /// Represents an order
    /// </summary>
    public partial class Order : BaseEntity
    {
        #region Properties

        /// <summary>
        /// Gets or sets the order identifier
        /// </summary>
        public Guid OrderGuid { get; set; }

        /// <summary>
        /// Gets or sets the store identifier
        /// </summary>
        public int StoreId { get; set; }

        /// <summary>
        /// Gets or sets the customer identifier
        /// </summary>
        public int CustomerId { get; set; }

        

        /// <summary>
        /// Gets or sets the customer IP address
        /// </summary>
        public string CustomerIp { get; set; }

        /// <summary>
        /// Gets or sets the paid date and time
        /// </summary>
        public DateTime? PaidDateUtc { get; set; }
        
        /// <summary>
        /// Gets or sets the shipping method
        /// </summary>
        public string ShippingMethod { get; set; }

        /// <summary>
        /// Gets or sets the serialized CustomValues (values from ProcessPaymentRequest)
        /// </summary>
        public string CustomValuesXml { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity has been deleted
        /// </summary>
        public bool Deleted { get; set; }

        /// <summary>
        /// Gets or sets the date and time of order creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the custom order number without prefix
        /// </summary>
        public string CustomOrderNumber { get; set; }


        /// <summary>
        /// Gets or sets an order status identifier
        /// </summary>
        public int OrderStatusId { get; set; }

        /// <summary>
        /// Gets or sets the shipping status identifier
        /// </summary>
        public int ShippingStatusId { get; set; }

        /// <summary>
        /// Gets or sets the payment status identifier
        /// </summary>
        public int PaymentStatusId { get; set; }

        #endregion

        #region Navigation properties

        /// <summary>
        /// Gets or sets the customer
        /// </summary>
        public virtual Customer Customer { get; set; }

        #endregion

        #region Custom properties

        /// <summary>
        /// Gets or sets the order status
        /// </summary>
        public OrderStatus OrderStatus
        {
            get
            {
                return (OrderStatus)OrderStatusId;
            }
            set
            {
                OrderStatusId = (int)value;
            }
        }

        /// <summary>
        /// Gets or sets the payment status
        /// </summary>
        public PaymentStatus PaymentStatus
        {
            get
            {
                return (PaymentStatus)PaymentStatusId;
            }
            set
            {
                PaymentStatusId = (int)value;
            }
        }
        /// <summary>
        /// Gets or sets the shipping status
        /// </summary>
        public ShippingStatus ShippingStatus
        {
            get
            {
                return (ShippingStatus)ShippingStatusId;
            }
            set
            {
                ShippingStatusId = (int)value;
            }
        }
        
        #endregion
    }



    /// <summary>
    /// Represents a payment status enumeration
    /// </summary>
    public enum PaymentStatus
    {
        /// <summary>
        /// Pending
        /// </summary>
        Pending = 10,
        /// <summary>
        /// Authorized
        /// </summary>
        Authorized = 20,
        /// <summary>
        /// Paid
        /// </summary>
        Paid = 30,
        /// <summary>
        /// Partially Refunded
        /// </summary>
        PartiallyRefunded = 35,
        /// <summary>
        /// Refunded
        /// </summary>
        Refunded = 40,
        /// <summary>
        /// Voided
        /// </summary>
        Voided = 50,
    }


    /// <summary>
    /// Represents the shipping status enumeration
    /// </summary>
    public enum ShippingStatus
    {
        /// <summary>
        /// Shipping not required
        /// </summary>
        ShippingNotRequired = 10,
        /// <summary>
        /// Not yet shipped
        /// </summary>
        NotYetShipped = 20,
        /// <summary>
        /// Partially shipped
        /// </summary>
        PartiallyShipped = 25,
        /// <summary>
        /// Shipped
        /// </summary>
        Shipped = 30,
        /// <summary>
        /// Delivered
        /// </summary>
        Delivered = 40,
    }
}
