using System;
using Nop.Core.Domain.BonusApp.Customers;

namespace Nop.Core.Domain.BonusApp
{
    /// <summary>
    /// Represents an bonus app status.
    /// </summary>
    public class BonusAppStatus : BaseEntity
    {
        // Current money in app
        public double CurrentMoney { get; set; }
    
        // User count waiting for app to return money.
        public int WaitingUserCount { get; set; }

        // User count app returned money
        public int CompleteUserCount { get; set;}
        // Money app has returned
        public double MoneyPaied { get; set; }
    }
}