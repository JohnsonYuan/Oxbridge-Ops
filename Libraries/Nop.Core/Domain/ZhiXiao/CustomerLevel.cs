namespace Nop.Core.Domain.ZhiXiao
{

    // 一共10个等级
    // 组长， 副组长， 组员
    // 董事0　：刚进入董事，　没等级
    // 出盘　　：
    public enum CustomerLevel
    {
        ZuYuan,
        FuZuZhang,
        ZuZhang,
        DongShi0,
        DongShi1,
        DongShi2,
        DongShi3,
        DongShi4,
        DongShi5,
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
