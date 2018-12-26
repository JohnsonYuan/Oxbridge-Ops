using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using Nop.Core.Domain.BonusApp.Customers;
using Nop.Core.Infrastructure.Mapper;
using Web.ZhiXiao.Areas.BonusApp.Models;

namespace Web.ZhiXiao.Areas.BonusApp.Mapper
{
    /// <summary>
    /// AutoMapper configuration for admin area models
    /// </summary>
    public class AutoMapperConfiguration : IMapperConfiguration
    {
        /// <summary>
        /// Order of this mapper implementation
        /// </summary>
        public int Order
        {
            get { return 1; }
        }

        public Action<IMapperConfigurationExpression> GetConfiguration()
        {
            Action<IMapperConfigurationExpression> action = cfg =>
              {
                  //customer roles
                  cfg.CreateMap<BonusApp_CustomerComment, CommentModel>()
                    .ForMember(dest => dest.CustomerNickName, mo => mo.MapFrom(src => src.Customer.Nickname))
                    .ForMember(dest => dest.CustomerAvatar, mo => mo.MapFrom(src => src.Customer.AvatarUrl))
                    .ForMember(dest => dest.CreatedOn, mo => mo.Ignore());
              };

            return action;
        }
    }
}