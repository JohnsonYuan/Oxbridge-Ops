using System;
using System.Text.RegularExpressions;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.ZhiXiao;
using Nop.Services.Helpers;

namespace Nop.Services.ZhiXiao
{
    public class CustomNumberFormatter : ICustomNumberFormatter
    {
        #region Fields

        private CustomerSettings _customerSettings;
        private IDateTimeHelper _dateTimeHelper;

        #endregion

        #region Ctor

        public CustomNumberFormatter(CustomerSettings customerSettings,
            IDateTimeHelper dateTimeHelper)
        {
            this._customerSettings = customerSettings;
            this._dateTimeHelper = dateTimeHelper;
        }

        #endregion

        #region Methods

        /// <summary>
        /// 小组编号码
        /// </summary>
        /// <param name="team">小组</param>
        /// <returns>小组编号码</returns>
        /// <remarks>
        /// 默认格式为{YY}{MM}{#:000000}
        /// 1804000001
        /// </remarks>
        public virtual string GenerateTeamNumber(CustomerTeam team)
        {
            var mask = _customerSettings.TeamNumberMask;
            if (string.IsNullOrEmpty(_customerSettings.TeamNumberMask))
                mask = "{YY}{MM}{#:000000}";

            var localTime = _dateTimeHelper.ConvertToUserTime(team.CreatedOnUtc);
            var customNumber = mask
                .Replace("{ID}", team.Id.ToString())
                .Replace("{YYYY}", team.CreatedOnUtc.ToString("yyyy"))
                .Replace("{YY}", team.CreatedOnUtc.ToString("yy"))
                .Replace("{MM}", team.CreatedOnUtc.ToString("MM"))
                .Replace("{DD}", team.CreatedOnUtc.ToString("dd")).Trim();

            //if you need to use the format for the ID with leading zeros, use the following code instead of the previous one.
            //mask for Id example {#:00000000}
            var rgx = new Regex(@"{#:\d+}");
            var match = rgx.Match(customNumber);
            var maskForReplase = match.Value;

            rgx = new Regex(@"\d+");
            match = rgx.Match(maskForReplase);

            var formatValue = match.Value;
            if (!string.IsNullOrEmpty(formatValue) && !string.IsNullOrEmpty(maskForReplase))
                customNumber = customNumber.Replace(maskForReplase, team.Id.ToString(formatValue));
            else
                customNumber = customNumber.Insert(0, string.Format("{0}-", team.Id));

            return customNumber;
        }

        /// <summary>
        /// 注册码
        /// </summary>
        /// <returns>注册码</returns>
        public virtual string GenerateRegistionCode()
        {
            int length = 13;
            string result = Guid.NewGuid().ToString();
            if (result.Length > length)
                result = result.Substring(0, length);
            return result;
        }

        #endregion
    }
}
