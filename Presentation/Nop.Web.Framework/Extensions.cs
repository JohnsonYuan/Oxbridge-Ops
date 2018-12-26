using Nop.Core.Infrastructure;
using Nop.Services.Helpers;
using Nop.Web.Framework.Kendoui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Nop.Web.Framework
{
    /// <summary>
    /// Extensions
    /// </summary>
    public static class Extensions
    {
        public static IEnumerable<T> PagedForCommand<T>(this IEnumerable<T> current, DataSourceRequest command)
        {
            return current.Skip((command.Page - 1) * command.PageSize).Take(command.PageSize);
        }
        
        /// <summary>
        /// Returns a value indicating whether real selection is not possible
        /// </summary>
        /// <param name="items">Items</param>
        /// <param name="ignoreZeroValue">A value indicating whether we should ignore items with "0" value</param>
        /// <returns>A value indicating whether real selection is not possible</returns>
        public static bool SelectionIsNotPossible(this IList<SelectListItem> items, bool ignoreZeroValue = true)
        {
            if (items == null)
                throw  new ArgumentNullException("items");

            //we ignore items with "0" value? Usually it's something like "Select All", "etc
            return items.Count(x => !ignoreZeroValue || !x.Value.ToString().Equals("0")) < 2;
        }

        /// <summary>
        /// Relative formatting of DateTime (e.g. 2 hours ago, a month ago)
        /// </summary>
        /// <param name="source">Source (UTC format)</param>
        /// <returns>Formatted date and time string</returns>
        public static string RelativeFormat(this DateTime source)
        {
            return RelativeFormat(source, string.Empty);
        }
        /// <summary>
        /// Relative formatting of DateTime (e.g. 2 hours ago, a month ago)
        /// </summary>
        /// <param name="source">Source (UTC format)</param>
        /// <param name="defaultFormat">Default format string (in case relative formatting is not applied)</param>
        /// <returns>Formatted date and time string</returns>
        public static string RelativeFormat(this DateTime source, string defaultFormat)
        {
            return RelativeFormat(source, false, defaultFormat);
        }
        public static string RelativeFormat(this DateTime source,
            bool convertToUserTime, string defaultFormat)
        {
            string result = "";

            var ts = new TimeSpan(DateTime.UtcNow.Ticks - source.Ticks);
            double delta = ts.TotalSeconds;

            // 增加限制delta < 2592000(30天) 限制显示到30天前按照相对时间显示
            if (delta > 0 && delta < 2592000)
            {
                if (delta < 60) // 60 (seconds)
                {
                    result = ts.Seconds == 1 ? "1秒前" : ts.Seconds + "秒前";
                }
                else if (delta < 120) //2 (minutes) * 60 (seconds)
                {
                    result = "1分钟前";
                }
                else if (delta < 2700) // 45 (minutes) * 60 (seconds)
                {
                    result = ts.Minutes + "分钟前";
                }
                else if (delta < 5400) // 90 (minutes) * 60 (seconds)
                {
                    result = "1小时前";
                }
                else if (delta < 86400) // 24 (hours) * 60 (minutes) * 60 (seconds)
                {
                    int hours = ts.Hours;
                    if (hours == 1)
                        hours = 2;
                    result = hours + "小时前";
                }
                else if (delta < 172800) // 48 (hours) * 60 (minutes) * 60 (seconds)
                {
                    result = "昨天";
                }
                else if (delta < 2592000) // 30 (days) * 24 (hours) * 60 (minutes) * 60 (seconds)
                {
                    result = ts.Days + "天前";
                }
                //else if (delta < 31104000) // 12 (months) * 30 (days) * 24 (hours) * 60 (minutes) * 60 (seconds)
                //{
                //    int months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
                //    result = months <= 1 ? "1个月前" : months + "月前";
                //}
                //else
                //{
                //    int years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
                //    result = years <= 1 ? "1年前" : years + "年前";
                //}
            }
            else
            {
                DateTime tmp1 = source;
                if (convertToUserTime)
                {
                    tmp1 = EngineContext.Current.Resolve<IDateTimeHelper>().ConvertToUserTime(tmp1, DateTimeKind.Utc);
                }
                //default formatting
                if (!String.IsNullOrEmpty(defaultFormat))
                {
                    result = tmp1.ToString(defaultFormat);
                }
                else
                {
                    result = tmp1.ToString();
                }
            }
            return result;
        }
    }
}
