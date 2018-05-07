using System.ComponentModel;

namespace Nop.Core.Domain.ZhiXiao
{
    /// <summary>
    /// Represents an activity log type record
    /// </summary>
    public enum CustomerTeamType
    {
        [Description("10000小组")]
        Normal = 1,
        [Description("26800小组")]
        Advanced = 2
    }
}
