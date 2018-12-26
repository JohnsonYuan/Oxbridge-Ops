using System;
using Nop.Core.Domain.BonusApp;

namespace Nop.Data.Mapping.BonusApp.Logging
{
    /// <summary>
    /// Represents an BonusApp_ActivityLogMap record
    /// </summary>
    public partial class BonusAppStatusMap : NopEntityTypeConfiguration<BonusAppStatus>
    {
        public BonusAppStatusMap()
        {
            this.ToTable("BonusApp_Status");
            this.Property(b => b.CurrentMoney).HasPrecision(18, 2);
            this.Property(b => b.MoneyPaied).HasPrecision(18, 2);
            this.Property(b => b.AllUserMoney).HasPrecision(18, 2);
        }
    }
}