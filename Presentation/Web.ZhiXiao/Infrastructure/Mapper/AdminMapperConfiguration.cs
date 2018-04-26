using System;
using AutoMapper;
using Nop.Admin.Models.Logging;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Logging;
using Nop.Core.Infrastructure.Mapper;
using Nop.Models.Customers;
using Nop.Core.Domain.Localization;
using Nop.Admin.Models.Localization;

/// <summary>
/// AutoMapper configuration for admin area models
/// </summary>
public class AdminMapperConfiguration : IMapperConfiguration
{
    /// <summary>
    /// Get configuration
    /// </summary>
    /// <returns>Mapper configuration action</returns>
    public Action<IMapperConfigurationExpression> GetConfiguration()
    {
        Action<IMapperConfigurationExpression> action = cfg =>
        {
            //customer roles
            cfg.CreateMap<CustomerRole, CustomerRoleModel>()
                .ForMember(dest => dest.PurchasedWithProductName, mo => mo.Ignore())
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
            cfg.CreateMap<CustomerRoleModel, CustomerRole>()
                .ForMember(dest => dest.PermissionRecords, mo => mo.Ignore());

            //logs
            cfg.CreateMap<Log, LogModel>()
                .ForMember(dest => dest.CustomerEmail, mo => mo.Ignore())
                .ForMember(dest => dest.CreatedOn, mo => mo.Ignore())
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
            cfg.CreateMap<LogModel, Log>()
                .ForMember(dest => dest.CreatedOnUtc, mo => mo.Ignore())
                .ForMember(dest => dest.LogLevelId, mo => mo.Ignore())
                .ForMember(dest => dest.Customer, mo => mo.Ignore());
            //ActivityLogType
            cfg.CreateMap<ActivityLogTypeModel, ActivityLogType>()
                .ForMember(dest => dest.SystemKeyword, mo => mo.Ignore());
            cfg.CreateMap<ActivityLogType, ActivityLogTypeModel>()
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
            cfg.CreateMap<ActivityLog, ActivityLogModel>()
                .ForMember(dest => dest.ActivityLogTypeName, mo => mo.MapFrom(src => src.ActivityLogType.Name))
                .ForMember(dest => dest.CustomerEmail, mo => mo.MapFrom(src => src.Customer.Email))
                .ForMember(dest => dest.CreatedOn, mo => mo.Ignore())
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
            //language
            cfg.CreateMap<Language, LanguageModel>()
                .ForMember(dest => dest.AvailableStores, mo => mo.Ignore())
                .ForMember(dest => dest.AvailableCurrencies, mo => mo.Ignore())
                .ForMember(dest => dest.SelectedStoreIds, mo => mo.Ignore())
                .ForMember(dest => dest.FlagFileNames, mo => mo.Ignore())
                .ForMember(dest => dest.Search, mo => mo.Ignore())
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
            cfg.CreateMap<LanguageModel, Language>()
                .ForMember(dest => dest.LocaleStringResources, mo => mo.Ignore())
                .ForMember(dest => dest.LimitedToStores, mo => mo.Ignore());
        };

        return action;
    }

    /// <summary>
    /// Order of this mapper implementation
    /// </summary>
    public int Order
    {
        get { return 0; }
    }
}