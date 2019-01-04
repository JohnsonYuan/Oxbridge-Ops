using Nop.Core.Domain.BonusApp.Logging;

namespace Nop.Data.Mapping.BonusApp.Logging
{
    public class BonusApp_WithdrawLogMap  : NopEntityTypeConfiguration<BonusApp_WithdrawLog>
    {
        public BonusApp_WithdrawLogMap()
        {
            this.HasKey(al => al.Id);
            this.Property(al => al.Amount).IsRequired().HasPrecision(18, 2);
            this.Property(al => al.Comment).IsRequired();
            this.Property(al => al.IpAddress).HasMaxLength(200);

            this.HasRequired(al => al.Customer)
                .WithMany()
                .HasForeignKey(al => al.CustomerId);
                
        }
    }
}
