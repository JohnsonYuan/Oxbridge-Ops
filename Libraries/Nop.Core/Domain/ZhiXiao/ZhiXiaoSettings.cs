using System.ComponentModel;
using Nop.Core.Configuration;

namespace Nop.Core.Domain.ZhiXiao
{
    public partial class ZhiXiaoSettings : ISettings
    {
        /// <summary>
        /// 提现比例
        /// </summary>
        [DisplayName("用户提现比例")]
        public double Withdraw_Rate { get; set; } = 0.95;

        /// <summary>
        /// 注册普通用户需要钱
        /// </summary>
        [DisplayName("注册普通用户需要金币")]
        public int Register_Money_NormalUser { get; set; } = 10000;
        /// <summary>
        /// 注册高级用户需要钱
        /// </summary>
        [DisplayName("注册高级用户需要金币")]
        public int Register_Money_AdvancedUser { get; set; } = 26800;


        /// <summary>
        /// 最多下线个数
        /// </summary>
        [DisplayName("最多下线个数")]
        public int MaxChildCount { get; set; } = 2;

        /// <summary>
        /// 小组中组长个数为1
        /// </summary>
        [DisplayName("小组中组长个数")]
        public int Team_ZuZhangCount { get; set; } = 1;
        /// <summary>
        /// 小组中副组长个数为2
        /// </summary>
        [DisplayName("小组中副组长个数")]
        public int Team_FuZuZhangCount { get; set; } = 2;

        /// <summary>
        /// 小组初始人数为7人
        /// </summary>
        [DisplayName("小组初始人数")]
        public int TeamInitUserCount { get; set; } = 7;
        /// <summary>
        /// 小组满足重新分组人数 (TeamInitUserCount * 2 + 1)
        /// </summary>
        [DisplayName("小组满足重新分组人数")]
        public int TeamReGroupUserCount { get; set; } = 15;

        /// <summary>
        /// 新增用户时组长分的钱
        /// </summary>
        [DisplayName("新增用户时组长分的钱(一般用户)")]
        public int NewUserMoney_ZuZhang_Normal { get; set; } = 2000;     
        [DisplayName("新增用户时组长分的钱(高级用户)")]
        public int NewUserMoney_ZuZhang_Advanced { get; set; } = 3000;

        /// <summary>
        /// 新增用户时副组长分的钱
        /// </summary>
        [DisplayName("新增用户时副组长分的钱(一般用户)")]
        public int NewUserMoney_FuZuZhang_Normal { get; set; } = 800; 
        [DisplayName("新增用户时副组长分的钱(高级用户)")]
        public int NewUserMoney_FuZuZhang_Advanced { get; set; } = 1000;

        #region 重新分组一般和高级用户钱数
        
        /// <summary>
        /// 重新分组时组长分的钱
        /// </summary>
        [DisplayName("重新分组时组长分的钱(一般用户)")]
        public int ReGroupMoney_ZuZhang_Normal { get; set; } = 40000;
        [DisplayName("重新分组时组长分的钱(高级用户)")]
        public int ReGroupMoney_ZuZhang_Advanced { get; set; } = 50000;


        /// <summary>
        /// 五星董事出盘, 奖励27万(五星董事升级！奖金30万， 扣除3万的税)
        /// </summary>
        [DisplayName("五星董事出盘分的钱(一般用户)")]
        public int ReGroupMoney_DongShi5_ChuPan_Normal { get; set; } = 250000;
        /// <summary>
        /// 五星董事出盘, 奖励27万(五星董事升级！奖金30万， 扣除3万的税)
        /// </summary>
        [DisplayName("五星董事出盘分的钱(高级用户)")]
        public int ReGroupMoney_DongShi5_ChuPan_Advanced { get; set; } = 270000;

        /// <summary>
        /// 组长升级, 董事级别的推荐人根据级别拿提成的基数 84000 + 8000 + 1600 * 4 = 98400
        /// </summary>
        [DisplayName("组长升级, 董事级别的推荐人根据级别拿提成的基数(一般用户)")]
        public int ReGroupMoney_DongShiBase_Normal { get; set; } = 78400;
        [DisplayName("组长升级, 董事级别的推荐人根据级别拿提成的基数(高级用户) (84000 + 8000 + 1600 * 4 = 98400)")]
        public int ReGroupMoney_DongShiBase_Advanced { get; set; } = 98400;
        [DisplayName("一星董事分组提成比例")]
        public double ReGroupMoney_Rate_DongShi1 { get; set; } = 0.02;
        [DisplayName("二星董事分组提成比例")]
        public double ReGroupMoney_Rate_DongShi2 { get; set; } = 0.04;
        [DisplayName("三星董事分组提成比例")]
        public double ReGroupMoney_Rate_DongShi3 { get; set; } = 0.06;
        [DisplayName("四星董事分组提成比例")]
        public double ReGroupMoney_Rate_DongShi4 { get; set; } = 0.08;
        [DisplayName("五星董事分组提成比例")]
        public double ReGroupMoney_Rate_DongShi5 { get; set; } = 0.02;

        /// <summary>
        /// 重新分组时副组长分的钱(不使用)
        /// </summary>
        //public int ReGroupMoney_FuZuZhang_Normal { get; set; } = 40000; 
        //public int ReGroupMoney_FuZuZhang_Advanced { get; set; } = 50000;

        /// <summary>
        /// 重新分组时前x个组员分钱
        /// </summary>
        [DisplayName("重新分组时前x个组员分钱")]
        public int ReGroupMoney_ZuYuan_Count { get; set; } = 4;
        /// <summary>
        /// 重新分组时组员钱数(一般用户)
        /// </summary>
        [DisplayName("重新分组时组员钱数(一般用户)")]
        public int ReGroupMoney_ZuYuan_Normal { get; set; } = 1200;
        /// <summary>
        /// 重新分组时组员钱数(高级用户)
        /// </summary>
        [DisplayName("重新分组时组员钱数(高级用户)")]
        public int ReGroupMoney_ZuYuan_Advanced { get; set; } = 1600;

        #endregion
    }
}
