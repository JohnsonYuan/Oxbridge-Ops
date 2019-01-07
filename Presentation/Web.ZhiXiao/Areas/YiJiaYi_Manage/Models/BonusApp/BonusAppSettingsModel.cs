using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Mvc;

namespace Web.ZhiXiao.Areas.YiJiaYi_Manage.Models.BonusApp
{
    public class BonusAppSettingsModel : BaseNopModel
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
        [UIHint("Double")]
        public double UserReturnMoneyPercent { get; set; } = 1;

        /// <summary>
        /// 用户充值后按照百分比存入奖金池
        /// </summary>
        [DisplayName("存入奖金池比例")]
        [UIHint("Double")]
        public double SaveToAppMoneyPercent { get; set; } = 0.2;

        /// <summary>
        /// 提现比例
        /// </summary>
        [DisplayName("用户提现比例")]
        [UIHint("Double")]
        public double Withdraw_Rate { get; set; } = 0.95;


        [DisplayName("Authentication Cookie Name")]
        public string AuthCookieName { get; set; }  // YiJiaYi.BONUS

        /// <summary>
        /// mdt salt
        /// </summary>
        [DisplayName("hash format")]
        public string HashedPasswordFormat { get; set; } = "MD5";
        /// <summary>
        /// mdt salt
        /// </summary>
        [DisplayName("md5 salt")]
        public string CustomerPasswordSalt { get; set; } = "Z3GP1bc=";
    }
}