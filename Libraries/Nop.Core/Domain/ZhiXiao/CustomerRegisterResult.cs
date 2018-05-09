namespace Nop.Core.Domain.ZhiXiao
{
    /// <summary>
    /// 用户注册下线结果
    /// </summary>
    public enum CustomerRegisterResult
    {
        /// <summary>
        /// register successful
        /// </summary>
        Successful = 1,
        /// <summary>
        /// 已经出盘
        /// </summary>
        OutOfTeam = 2,
        /// <summary>
        /// 电子币不足
        /// </summary>
        MoneyNotEnough = 3,
        /// <summary>
        /// 已有最多下线个数
        /// </summary>
        ChildFull = 4,
    }
}
