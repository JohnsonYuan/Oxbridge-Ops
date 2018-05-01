using System;
using Nop.Core.Configuration;

namespace Nop.Core.Domain.ZhiXiao
{
    public class ZhiXiaoSettings : ISettings
    {
        /// <summary>
        /// 最多下线个数
        /// </summary>
        public int MaxChildCount { get; set; } = 2;

        /// <summary>
        /// 小组中组长个数为1
        /// </summary>
        public int Team_ZuZhangCount { get; set; } = 1;
        /// <summary>
        /// 小组中副组长个数为2
        /// </summary>
        public int Team_FuZuZhangCount { get; set; } = 2;

        /// <summary>
        /// 小组满足重新分组人数
        /// </summary>
        public int TeamReGroupCount { get; set; } = 15;

        #region 重新分组一般和高级用户钱数
      
        /// <summary>
        /// 重新分组时组长分的钱
        /// </summary>
        public int ReGroupMoney_ZuZhang_Normal { get; set; } = 50000; 
        public int ReGroupMoney_ZuZhang_Advanced { get; set; } = 50000;

        /// <summary>
        /// 重新分组时副组长分的钱
        /// </summary>
        public int ReGroupMoney_FuZuZhang_Normal { get; set; } = 50000; 
        public int ReGroupMoney_FuZuZhang_Advanced { get; set; } = 50000;

        /// <summary>
        /// 重新分组时组员分的钱
        /// </summary>
        public int ReGroupMoney_ZuYuan_Normal { get; set; } = 50000; 
        public int ReGroupMoney_ZuYuan_Advanced { get; set; } = 50000;

        #endregion
    }
}
