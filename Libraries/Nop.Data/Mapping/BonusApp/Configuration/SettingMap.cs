using System;
using Nop.Core.Domain.BonusApp.Configuration;

namespace Nop.Data.Mapping.BonusApp.Logging
{
    /// <summary>
    /// Represents an BBonusAppStatusMap
    /// </summary>
    public partial class BonusAppSettingMap : NopEntityTypeConfiguration<BonusApp_Setting>
    {
        public BonusAppSettingMap()
        {
            this.ToTable("BonusApp_Setting");
        }
    }
}