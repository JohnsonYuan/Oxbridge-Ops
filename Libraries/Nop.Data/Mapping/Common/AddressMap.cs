using Nop.Core.Domain.Common;

namespace Nop.Data.Mapping.Common
{
    public partial class AddressMap : NopEntityTypeConfiguration<Address>
    {
        public AddressMap()
        {
            this.ToTable("Address");
            this.HasKey(a => a.Id);
        }
    }
}
