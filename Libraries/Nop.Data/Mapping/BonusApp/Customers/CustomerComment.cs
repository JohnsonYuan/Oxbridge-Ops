using Nop.Core.Domain.BonusApp.Customers;

namespace Nop.Data.Mapping.BonusApp.Customers
{
    public class BonusApp_CustomerCommentMap : NopEntityTypeConfiguration<BonusApp_CustomerComment>
    {
        public BonusApp_CustomerCommentMap()
        {
            this.ToTable("BonusApp_CustomerComment");
            this.Property(al => al.Comment).HasMaxLength(300);
        }
    }
}
