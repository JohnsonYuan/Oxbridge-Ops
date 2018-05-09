namespace Nop.Core.Domain.ZhiXiao
{
    public static partial class SystemZhiXiaoLogTypes
    {
        /// <summary>
        /// 小组新增用户
        /// </summary>
        public static string AddNewUser { get { return "ZhiXiao_AddNewUser"; } }

        /// <summary>
        /// 小组重新分组(14个人按规则分到新的小组)
        /// </summary>
        public static string ReGroupTeam_ReSort { get { return "ZhiXiao_ReGroup_ReSort"; } }

        /// <summary>
        /// 小组人数满足15人, 重新分组. 奖金+{0}
        /// </summary>
        public static string ReGroupTeam_AddMoney { get { return "ZhiXiao_ReGroup_AddMoney"; } }

        /// <summary>
        /// 小组人数满足15人, 升级
        /// </summary>
        public static string ReGroupTeam_UpdateLevel { get { return "ZhiXiao_ReGroup_UpdateLevel"; } }

        /// <summary>
        /// 充值电子币
        /// </summary>
        public static string RechargeMoney { get { return "ZhiXiao_Recharge"; } }
        
        /// <summary>
        /// 管理员发货
        /// </summary>
        public static string SendProduct { get { return "ZhiXiao_SendProduct"; } }
        /// <summary>
        /// 用户发货
        /// </summary>
        public static string ReceiveProduct { get { return "ZhiXiao_ReceiveProduct"; } }
        
        /// <summary>
        /// 用户提现申请
        /// </summary>
        public static string Withdraw { get { return "ZhiXiao_Withdraw"; } }
        /// <summary>
        /// 管理员处理提现申请
        /// </summary>
        public static string ProcessWithdraw { get { return "ZhiXiao_ProcessWithdraw"; } }
        /// <summary>
        /// 注册新用户
        /// </summary>
        public static string RegisterNewUser { get { return "ZhiXiao_RegisterNewUser"; } }
    }
}
