using Nop.Core.Domain.ZhiXiao;

namespace Nop.Data.Mapping.Customers
{
    public partial class CustomerTeamMap : NopEntityTypeConfiguration<CustomerTeam>
    {
        public CustomerTeamMap()
        {
            this.ToTable("CustomerTeam");
            this.HasKey(t => t.Id);
            this.Property(t => t.CustomNumber).HasMaxLength(100);

            this.Ignore(l => l.TeamType);
        }
    }
}
