using System;
using Nop.Core.Domain.BonusApp.Logging;

namespace Nop.Data.Mapping.BonusApp.Logging
{
    /// <summary>
    /// Represents an BonusApp_ActivityLogMap record
    /// </summary>
    public class BonusApp_ActivityLogTypeMap : NopEntityTypeConfiguration<BonusApp_ActivityLogType>
    {
        public BonusApp_ActivityLogTypeMap()
        {
            this.HasKey(alt => alt.Id);

            this.Property(alt => alt.SystemKeyword).IsRequired().HasMaxLength(100);
            this.Property(alt => alt.Name).IsRequired().HasMaxLength(200);
        }
    }
}