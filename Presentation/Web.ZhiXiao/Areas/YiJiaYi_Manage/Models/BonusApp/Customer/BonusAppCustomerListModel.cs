using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Web.ZhiXiao.Areas.YiJiaYi_Manage.Models.BonusApp.Customer
{
    public class BonusAppCustomerListModel : BaseNopModel
    {
        public BonusAppCustomerListModel()
        {
        }

        [NopResourceDisplayName("Admin.Customers.Customers.List.SearchNickname")]
        [AllowHtml]
        public string SearchNickname { get; set; }

        [NopResourceDisplayName("Admin.Customers.Customers.List.SearchUsername")]
        [AllowHtml]
        public string SearchUsername { get; set; }


        [NopResourceDisplayName("Admin.Customers.Customers.List.SearchPhone")]
        [AllowHtml]
        public string SearchPhone { get; set; }
        public bool PhoneEnabled { get; set; }
        
        [NopResourceDisplayName("用户金额>=")]
        [UIHint("Decimal")]
        public decimal SearchMoney { get; set; }

        [NopResourceDisplayName("Admin.Customers.Customers.List.SearchIpAddress")]
        public string SearchIpAddress { get; set; }
    }
}