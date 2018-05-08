using System.Collections.Generic;
using Nop.Models.Customers;
using Nop.Web.Framework.Mvc;

namespace Nop.Web.Models.Home
{
    public partial class CustomerIndexModel
    {
        /// <summary>
        /// 个人信息
        /// </summary>
        public CustomerModel CustomerInfo { get; set; }

        /// <summary>
        /// 所在小组成员
        /// </summary>
        public IList<CustomerDiagramModel> TeamUsers { get; set; }
        
        /// <summary>
        /// 下线
        /// </summary>
        public List<CustomerDiagramModel> Children { get; set; }
    }
}