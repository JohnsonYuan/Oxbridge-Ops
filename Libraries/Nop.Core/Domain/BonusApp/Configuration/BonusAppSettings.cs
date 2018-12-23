using System.ComponentModel;
using Nop.Core.Configuration;

namespace Nop.Core.Domain.BonusApp.Configuration
{
    /// <summary>
    /// Represents an bonus app status.
    /// </summary>
    public class BonusAppSettings : IBonusApp_Settings
    {
        /// <summary>
        /// 网站名称
        /// </summary>
        [DisplayName("网站名称")]
        public string SiteTitle { get; set; }

        /// <summary>
        /// 用户返回金额比例 目前返还100%
        /// </summary>
        [DisplayName("用户返回金额比例")]
        public double UserReturnMoneyPercent { get; set; } = 1;

        /// <summary>
        /// 用户充值后按照百分比存入奖金池
        /// </summary>
        [DisplayName("存入奖金池比例")]
        public double SaveToAppMoneyPercent { get; set; } = 0.2;

        /// <summary>
        /// 提现比例
        /// </summary>
        [DisplayName("用户提现比例")]
        public double Withdraw_Rate { get; set; } = 0.95;

        /// <summary>
        /// mdt salt
        /// </summary>
        [DisplayName("md5 salt")]
        public string CustomerPasswordSalt { get; set; } = "Z3GP1bc=";
    }
}