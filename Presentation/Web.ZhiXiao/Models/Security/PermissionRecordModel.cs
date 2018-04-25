using Nop.Web.Framework.Mvc;

namespace Web.ZhiXiao.Models.Security
{
    public partial class PermissionRecordModel : BaseNopModel
    {
        public string Name { get; set; }
        public string SystemName { get; set; }
    }
}