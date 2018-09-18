namespace Nop.Core
{
    public static class ZhiXiaoConfig
    {
        /// <summary>
        /// email 地址后缀 Customer需要email字段, 修改密码也是根据email修改
        /// </summary>
        public static string EmailPostfix
        {
            get
            {
                return "@yijiayifr.com";
            }
        }

        public static string AppendEmailToUsername(string username)
        {
            return username + EmailPostfix;
        }
    }
}
