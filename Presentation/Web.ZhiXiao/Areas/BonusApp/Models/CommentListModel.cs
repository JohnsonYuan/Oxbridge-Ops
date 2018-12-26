using System;
using Nop.Web.Framework.Mvc;

namespace Web.ZhiXiao.Areas.BonusApp.Models
{
    public class CommentListModel : BasePagedListModel<CommentModel>
    {
    }

    public class CommentModel : BaseNopEntityModel
    {
        /// <summary>
        /// Gets or sets the customer identifier
        /// </summary>
        public int CustomerId { get; set; }
        public string CustomerNickName { get; set; }
        public string CustomerAvatar { get; set; }

        public string Comment { get; set; }

        public int Rate { get; set; }   // 1 - 5

        /// <summary>
        /// Gets or sets the ip address
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance creation
        /// </summary>
        //public DateTime CreatedOn { get; set; }
        public string CreatedOn { get; set; }
    }
}