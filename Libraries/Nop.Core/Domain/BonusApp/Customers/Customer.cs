using System;

namespace Nop.Core.Domain.BonusApp.Customers
{
    /// <summary>
    /// Represents a customer
    /// </summary>
    public class BonusApp_Customer : BaseEntity
    {
        /// <summary>
        /// Ctor
        /// </summary>
        public BonusApp_Customer()
        {
            this.CustomerGuid = Guid.NewGuid();
        }

        /// <summary>
        /// Gets or sets the customer Guid
        /// </summary>
        public Guid CustomerGuid { get; set; }

        /// <summary>
        /// Gets or sets the username
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the password
        /// </summary>
        public string Password { get; set; }
        
        /// <summary>
        /// Gets or sets the avatar url
        /// </summary>
        public string AvatarFileName { get; set; }

        /// <summary>
        /// Gets or sets the nickname
        /// </summary>
        public string Nickname { get; set; }

        /// <summary>
        /// Gets or sets the password
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the customer is active
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the customer has been deleted
        /// </summary>
        public bool Deleted { get; set; }

        /// <summary>
        /// Gets or sets the last IP address
        /// </summary>
        public string LastIpAddress { get; set; }

        /// <summary>
        /// Gets or sets the date and time of entity creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the date and time of last login
        /// </summary>
        public DateTime? LastLoginDateUtc { get; set; }

        /// <summary>
        /// Gets or sets the date and time of last activity
        /// </summary>
        public DateTime LastActivityDateUtc { get; set; }

        #region Bonus properties
                  
        public decimal Money { get; set; }

        /// <summary>
        /// 如果当前用户在队列中金额返还, 
        /// 需要提示用户可提现, 保存BonusApp_MoneyLog表对应id和金额
        /// 默认为null
        /// </summary>
        public int? NotificationMoneyLogId { get; set; }
        public decimal? NotificationMoney { get; set; }

        #endregion

        #region Navigation properties

        /*
        /// <summary>
        /// Gets or sets shopping cart items
        /// </summary>
        public virtual ICollection<ShoppingCartItem> ShoppingCartItems
        {
            get { return _shoppingCartItems ?? (_shoppingCartItems = new List<ShoppingCartItem>()); }
            protected set { _shoppingCartItems = value; }            
        }

        /// <summary>
        /// Gets or sets return request of this customer
        /// </summary>
        public virtual ICollection<ReturnRequest> ReturnRequests
        {
            get { return _returnRequests ?? (_returnRequests = new List<ReturnRequest>()); }
            protected set { _returnRequests = value; }            
        }*/

        #endregion
    }
}
