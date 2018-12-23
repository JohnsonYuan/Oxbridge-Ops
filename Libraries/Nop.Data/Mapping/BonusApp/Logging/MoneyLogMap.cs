using System;
using Nop.Core.Domain.BonusApp.Logging;

namespace Nop.Data.Mapping.BonusApp.Logging
{
    /// <summary>
    /// Represents an BonusApp_ActivityLogMap record
    /// </summary>
    public partial class BonusApp_MoneyLogMap : NopEntityTypeConfiguration<BonusApp_MoneyLog>
    {
        public BonusApp_MoneyLogMap()
        {
            this.Property(al => al.Comment).IsRequired();
            this.Property(al => al.IpAddress).HasMaxLength(200);

            this.HasRequired(al => al.Customer)
                .WithMany()
                .HasForeignKey(al => al.CustomerId);
                
            this.Ignore(m => m.MoneyReturnStatusId);
        }
    }
}