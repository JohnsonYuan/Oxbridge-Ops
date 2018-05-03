using Nop.Core.Domain.Logging;

namespace Nop.Data.Mapping.Logging
{
    /// <summary>
    /// Mapping class
    /// </summary>
    public partial class WithdrawLogMap : NopEntityTypeConfiguration<WithdrawLog>
    {
        /// <summary>
        /// Ctor
        /// </summary>
        public WithdrawLogMap()
        {
            this.ToTable("WithdrawLog");
            this.HasKey(al => al.Id);
            this.Property(al => al.Amount).IsRequired();
            this.Property(al => al.Comment).IsRequired();
            this.Property(al => al.IpAddress).HasMaxLength(200);

            this.HasRequired(al => al.Customer)
                .WithMany()
                .HasForeignKey(al => al.CustomerId);
        }
    }
}
