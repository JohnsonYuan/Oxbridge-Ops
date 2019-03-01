namespace Web.ZhiXiao.Areas.BonusApp.Models.Users
{
    /// <summary>
    /// 用户体现申请model
    /// </summary>
    public class TiXianModel
    {
        public int Money { get; set; }

        /// <summary>
        /// 1. 支付宝
        /// 2. 银行卡
        /// </summary>
        public int PayType { get; set; }

        #region Alipay 1. 支付宝

        public string AlipayName { get; set; }

        #endregion

        #region Bank 2. 银行卡

        /// <summary>
        /// 开户行
        /// </summary>
        public string KaiHuHang { get; set; }
        /// <summary>
        /// 开户名
        /// </summary>
        public string KaiHuMing { get; set; }
        /// <summary>
        /// 卡号
        /// </summary>
        public string CardNum { get; set; }

        #endregion
    }

    /// <summary>
    /// 提现支付方式
    /// 1. 支付宝
    /// 2. 银行卡
    /// </summary>
    public enum TiXianPayType
    {
        Alipay = 1,
        Bank = 2
    }
}