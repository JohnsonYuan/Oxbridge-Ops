using Nop.Core.Domain.Logging;

namespace Nop.Data.Mapping.Logging
{
    /// <summary>
    /// Mapping class
    /// </summary>
    public partial class MoneyLogMap : NopEntityTypeConfiguration<MoneyLog>
    {
        /// <summary>
        /// Ctor
        /// </summary>
        public MoneyLogMap()
        {
            //this.ToTable("MoneyLog");
            //this.Map(m =>
            //{
            //    m.MapInheritedProperties();
            //    m.ToTable("MoneyLog");
            //});

            this.ToTable("MoneyLog");
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
