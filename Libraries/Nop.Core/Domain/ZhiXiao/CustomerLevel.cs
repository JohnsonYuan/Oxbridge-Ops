using System.ComponentModel;

namespace Nop.Core.Domain.ZhiXiao
{

    // 一共10个等级
    // 组长， 副组长， 组员
    // 董事0　：刚进入董事，　没等级
    // 出盘　　：
    public enum CustomerLevel
    {
        [Description("组员")]
        ZuYuan,
        [Description("副组长")]
        FuZuZhang,
        [Description("组长")]
        ZuZhang,
        [Description("不在小组中, 充值3万继续")]
        PreDongShi,
        [Description("董事级别")]
        DongShi0,
        [Description("一星董事")]
        DongShi1,
        [Description("二星董事")]
        DongShi2,
        [Description("三星董事")]
        DongShi3,
        [Description("四星董事")]
        DongShi4,
        [Description("五星董事")]
        DongShi5,
        [Description("出盘")]
        ChuPan
    }

    //This is a extension class of enum
    //public string GetEnumDisplayName(this System.Enum enumType)
    //{
    //    return enumType.GetType().GetMember(enumType.ToString())
    //                   .First()
    //                   .GetCustomAttribute<DisplayAttribute>()
    //                   .Name;
    //}
}
