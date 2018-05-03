using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core.Domain.ZhiXiao;

namespace Nop.Models.Customers
{
    public partial class TeamDiagramModel
    {
        public CustomerTeam Team { get; set; }

        /// <summary>
        /// 第1到7个用户
        /// </summary>
        public IList<CustomerDiagramModel> TopHalfUsers { get; set; }

        /// <summary>
        /// 第8到14个用户
        /// </summary>
        public IList<CustomerDiagramModel> LastHalfUsers { get; set; }

        public IList<CustomerDiagramModel> AllUsers
        {
            get {
                return TopHalfUsers.Union(LastHalfUsers).ToList();      
            }
        }
    }

    public class CustomerDiagramModel
    {
        public CustomerDiagramModel()
        {
            this.Child = new List<CustomerDiagramModel>();
        }

        public string NickName { get; set; }
        public string LevelDesription { get; set; }
        public int InTeamOrder { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public IList<CustomerDiagramModel> Child { get; set; }

        public string DisplayInfo { get
            {
                return string.Format("{0} ({1})", NickName, LevelDesription);
            }
        }
    }
}