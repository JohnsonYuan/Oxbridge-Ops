using System;
using AutoMapper;
using Nop.Admin.Models.Localization;
using Nop.Admin.Models.Logging;
using Nop.Admin.Models.News;
using Nop.Admin.Models.Settings;
using Nop.Admin.Models.Stores;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.News;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.ZhiXiao;
using Nop.Core.Infrastructure.Mapper;
using Nop.Models.Customers;
using Nop.Services.Customers;

namespace Nop.Admin.Infrastructure.Mapper
{
    /// <summary>
    /// AutoMapper configuration for admin area models
    /// </summary>
    public class AutoMapperConfiguration : IMapperConfiguration
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
                
                //customer diagarm
                cfg.CreateMap<Customer, CustomerDiagramModel>()
                    .ForMember(dest => dest.Child, mo => mo.Ignore())
                    .ForMember(dest => dest.NickName, mo => mo.MapFrom(src => src.GetNickName()))
                    .ForMember(dest => dest.InTeamOrder, mo => mo.MapFrom(src => src.GetInTeamOrder()))
                    .ForMember(dest => dest.LevelDesription, mo => mo.MapFrom(src => src.GetLevelDescription()));

                //customer teams
                cfg.CreateMap<CustomerTeam, CustomerTeamModel>()
                    .ForMember(dest => dest.CreatedOn, mo => mo.Ignore())
                    .ForMember(dest => dest.Customers, mo => mo.Ignore());
                cfg.CreateMap<CustomerTeamModel, CustomerTeam>()
                    .ForMember(dest => dest.CreatedOnUtc, mo => mo.Ignore())
                    .ForMember(dest => dest.Customers, mo => mo.Ignore());

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
                //stores
                cfg.CreateMap<Store, StoreModel>()
                    .ForMember(dest => dest.AvailableLanguages, mo => mo.Ignore())
                    .ForMember(dest => dest.Locales, mo => mo.Ignore())
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<StoreModel, Store>();

                //Settings
                cfg.CreateMap<NewsSettings, NewsSettingsModel>()
                    .ForMember(dest => dest.ActiveStoreScopeConfiguration, mo => mo.Ignore())
                    .ForMember(dest => dest.Enabled_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.AllowNotRegisteredUsersToLeaveComments_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.NotifyAboutNewNewsComments_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.ShowNewsOnMainPage_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.MainPageNewsCount_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.NewsArchivePageSize_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.ShowHeaderRssUrl_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.NewsCommentsMustBeApproved_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<NewsSettingsModel, NewsSettings>();


                cfg.CreateMap<CustomerSettings, CustomerUserSettingsModel.CustomerSettingsModel>()
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<CustomerUserSettingsModel.CustomerSettingsModel, CustomerSettings>()
                    .ForMember(dest => dest.HashedPasswordFormat, mo => mo.Ignore())
                    .ForMember(dest => dest.AvatarMaximumSizeBytes, mo => mo.Ignore())
                    .ForMember(dest => dest.DownloadableProductsValidateUser, mo => mo.Ignore())
                    .ForMember(dest => dest.OnlineCustomerMinutes, mo => mo.Ignore())
                    .ForMember(dest => dest.SuffixDeletedCustomers, mo => mo.Ignore())
                    .ForMember(dest => dest.DeleteGuestTaskOlderThanMinutes, mo => mo.Ignore());

                //news
                cfg.CreateMap<NewsItem, NewsItemModel>()
                    // .ForMember(dest => dest.SeName, mo => mo.MapFrom(src => src.GetSeName(src.LanguageId, true, false)))
                    .ForMember(dest => dest.ApprovedComments, mo => mo.Ignore())
                    .ForMember(dest => dest.NotApprovedComments, mo => mo.Ignore())
                    .ForMember(dest => dest.StartDate, mo => mo.Ignore())
                    .ForMember(dest => dest.EndDate, mo => mo.Ignore())
                    .ForMember(dest => dest.CreatedOn, mo => mo.Ignore())
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore())
                    .ForMember(dest => dest.AvailableLanguages, mo => mo.Ignore());
                cfg.CreateMap<NewsItemModel, NewsItem>()
                    .ForMember(dest => dest.NewsComments, mo => mo.Ignore())
                    .ForMember(dest => dest.Language, mo => mo.Ignore())
                    .ForMember(dest => dest.StartDateUtc, mo => mo.Ignore())
                    .ForMember(dest => dest.EndDateUtc, mo => mo.Ignore())
                    .ForMember(dest => dest.CreatedOnUtc, mo => mo.Ignore())
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
}