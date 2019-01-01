using Nop.Core.Domain.BonusApp.Customers;
using Nop.Services.Media;
using Web.ZhiXiao.Areas.BonusApp.Models;

namespace Web.ZhiXiao.Areas.BonusApp.Factories
{
    public class CustomerModelFactory
    {
        private IPictureService _pictureService;

        public CustomerModelFactory(IPictureService pictureService)
        {
            this._pictureService = pictureService;
        }

        public CustomerModel PrepareCustomerModel(BonusApp_Customer customer)
        {
            var model = new CustomerModel
            {
                Money = customer.Money,
                NickName = customer.Nickname,
                Avatar = _pictureService.GetPictureUrl(customer.AvatarFileName)
            };

            return model;
        }
    }
}