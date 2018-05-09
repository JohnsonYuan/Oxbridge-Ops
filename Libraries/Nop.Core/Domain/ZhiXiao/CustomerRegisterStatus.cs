namespace Nop.Core.Domain.ZhiXiao
{
    /// <summary>
    /// 用户注册下线结果
    /// </summary>
    public enum CustomerRegisterStatus
    {
        /// <summary>
        /// register successful
        /// </summary>
        Successful = 1,
        /// <summary>
        /// team == null, 高级用户 => 出盘
        /// </summary>
        OutOfTeam = 2,
        /// <summary>
        /// 已经出盘 team == null, 普通用户 => 出盘, 可以充值进入高级组
        /// </summary>
        OutOfTeam_Temp = 3,
        /// <summary>
        /// 电子币不足
        /// </summary>
        MoneyNotEnough = 4,
        /// <summary>
        /// 已有最多下线个数
        /// </summary>
        ChildFull = 5,
    }
}
