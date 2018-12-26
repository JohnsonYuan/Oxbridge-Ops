using Nop.Core.Domain.BonusApp.Customers;

namespace Nop.Data.Mapping.BonusApp.Customers
{
    public partial class CustomerMap : NopEntityTypeConfiguration<BonusApp_Customer>
    {
        public CustomerMap()
        {
            this.ToTable("BonusApp_Customer");

            this.Property(c => c.Money).HasPrecision(18, 2);
        }
    }
}