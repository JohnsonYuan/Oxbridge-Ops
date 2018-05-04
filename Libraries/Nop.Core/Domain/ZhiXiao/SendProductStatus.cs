using System.ComponentModel;

namespace Nop.Core.Domain.ZhiXiao
{
    public enum SendProductStatus
    {
        [Description("未发货")]
        NotYet,
        [Description("已发货")]
        Sended,
        [Description("确认收货")]
        Received,
    }
}
