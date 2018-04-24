using Nop.Web.Framework.Mvc;

namespace Nop.Web.Models.Home
{
    public partial class DashboardModel : BaseNopModel
    {
        public bool IsLoggedInAsVendor { get; set; }
    }
}