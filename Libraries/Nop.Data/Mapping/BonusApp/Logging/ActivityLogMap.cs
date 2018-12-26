using System;
using Nop.Core.Domain.BonusApp.Logging;

namespace Nop.Data.Mapping.BonusApp.Logging
{
    /// <summary>
    /// Represents an BonusApp_ActivityLogMap record
    /// </summary>
    public partial class BonusApp_ActivityLogMap : NopEntityTypeConfiguration<BonusApp_ActivityLog>
    {
        public BonusApp_ActivityLogMap()
        {
            this.ToTable("BonusApp_ActivityLog");
            this.HasKey(al => al.Id);
            this.Property(al => al.Comment).IsRequired();
            this.Property(al => al.IpAddress).HasMaxLength(200);

            this.HasRequired(al => al.ActivityLogType)
                .WithMany()
                .HasForeignKey(al => al.ActivityLogTypeId);

            this.HasRequired(al => al.Customer)
                .WithMany()
                .HasForeignKey(al => al.CustomerId);
        }
    }
}