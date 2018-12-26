using System;
using Nop.Admin.Models.Localization;
using Nop.Admin.Models.Logging;
using Nop.Admin.Models.News;
using Nop.Admin.Models.Settings;
using Nop.Admin.Models.Stores;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.News;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.ZhiXiao;
using Nop.Models.Customers;
using Nop.Web.Framework.Mvc;

namespace Nop.Extensions
{
    public static class MappingExtensions
    {
        /// <summary>
        /// Execute a mapping from the source object to a new destination object. The source type is inferred from the source object
        /// </summary>
        /// <typeparam name="TDestination">Destination object type</typeparam>
        /// <param name="source">Source object to map from</param>
        /// <returns>Mapped destination object</returns>
        private static TDestination Map<TDestination>(this object source)
        {
            //use AutoMapper for mapping objects
            return Core.Infrastructure.Mapper.AutoMapperConfiguration.Mapper.Map<TDestination>(source);
        }

        public static TDestination MapTo<TSource, TDestination>(this TSource source)
        {
            return Core.Infrastructure.Mapper.AutoMapperConfiguration.Mapper.Map<TSource, TDestination>(source);
        }

        public static TDestination MapTo<TSource, TDestination>(this TSource source, TDestination destination)
        {
            return Core.Infrastructure.Mapper.AutoMapperConfiguration.Mapper.Map(source, destination);
        }

        
        #region Model-Entity mapping

        /// <summary>
        /// Execute a mapping from the entity to a new model
        /// </summary>
        /// <typeparam name="TModel">Model type</typeparam>
        /// <param name="entity">Entity to map from</param>
        /// <returns>Mapped model</returns>
        public static TModel ToModel<TModel>(this BaseEntity entity) where TModel : BaseNopEntityModel
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return entity.Map<TModel>();
        }

        /// <summary>
        /// Execute a mapping from the entity to the existing model
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <typeparam name="TModel">Model type</typeparam>
        /// <param name="entity">Entity to map from</param>
        /// <param name="model">Model to map into</param>
        /// <returns>Mapped model</returns>
        public static TModel ToModel<TEntity, TModel>(this TEntity entity, TModel model) 
            where TEntity : BaseEntity where TModel : BaseNopEntityModel
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (model == null)
                throw new ArgumentNullException(nameof(model));

            return entity.MapTo(model);
        }

        /// <summary>
        /// Execute a mapping from the model to a new entity
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="model">Model to map from</param>
        /// <returns>Mapped entity</returns>
        public static TEntity ToEntity<TEntity>(this BaseNopEntityModel model) where TEntity : BaseEntity
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            return model.Map<TEntity>();
        }

        /// <summary>
        /// Execute a mapping from the model to the existing entity
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <typeparam name="TModel">Model type</typeparam>
        /// <param name="model">Model to map from</param>
        /// <param name="entity">Entity to map into</param>
        /// <returns>Mapped entity</returns>
        public static TEntity ToEntity<TEntity, TModel>(this TModel model, TEntity entity)
            where TEntity : BaseEntity where TModel : BaseNopEntityModel
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return model.MapTo(entity);
        }

        #endregion

        #region Customer roles

        //customer roles
        public static CustomerRoleModel ToModel(this CustomerRole entity)
        {
            return entity.MapTo<CustomerRole, CustomerRoleModel>();
        }

        public static CustomerRole ToEntity(this CustomerRoleModel model)
        {
            return model.MapTo<CustomerRoleModel, CustomerRole>();
        }

        public static CustomerRole ToEntity(this CustomerRoleModel model, CustomerRole destination)
        {
            return model.MapTo(destination);
        }

        #endregion

        #region Customer diagarm
        
        //customer diagarm
        public static CustomerDiagramModel ToModel(this Customer entity)
        {
            return entity.MapTo<Customer, CustomerDiagramModel>();
        }

        #endregion

        #region Customer team

        //customer roles
        public static CustomerTeamModel ToModel(this CustomerTeam entity)
        {
            return entity.MapTo<CustomerTeam, CustomerTeamModel>();
        }

        public static CustomerTeam ToEntity(this CustomerTeamModel model)
        {
            return model.MapTo<CustomerTeamModel, CustomerTeam>();
        }

        public static CustomerTeam ToEntity(this CustomerTeamModel model, CustomerTeam destination)
        {
            return model.MapTo(destination);
        }

        #endregion

        #region Withdraw

        public static WithdrawLogModel ToModel(this WithdrawLog entity)
        {
            return entity.MapTo<WithdrawLog, WithdrawLogModel>();
        }

        #endregion

        #region Log

        public static LogModel ToModel(this Log entity)
        {
            return entity.MapTo<Log, LogModel>();
        }

        public static Log ToEntity(this LogModel model)
        {
            return model.MapTo<LogModel, Log>();
        }

        public static Log ToEntity(this LogModel model, Log destination)
        {
            return model.MapTo(destination);
        }

        public static ActivityLogTypeModel ToModel(this ActivityLogType entity)
        {
            return entity.MapTo<ActivityLogType, ActivityLogTypeModel>();
        }

        public static ActivityLogModel ToModel(this ActivityLog entity)
        {
            return entity.MapTo<ActivityLog, ActivityLogModel>();
        }

        public static MoneyLogModel ToModel(this MoneyLog entity)
        {
            return entity.MapTo<MoneyLog, MoneyLogModel>();
        }

        #endregion

        #region Languages

        public static LanguageModel ToModel(this Language entity)
        {
            return entity.MapTo<Language, LanguageModel>();
        }

        public static Language ToEntity(this LanguageModel model)
        {
            return model.MapTo<LanguageModel, Language>();
        }

        public static Language ToEntity(this LanguageModel model, Language destination)
        {
            return model.MapTo(destination);
        }

        #endregion

        #region Stores

        public static StoreModel ToModel(this Store entity)
        {
            return entity.MapTo<Store, StoreModel>();
        }

        public static Store ToEntity(this StoreModel model)
        {
            return model.MapTo<StoreModel, Store>();
        }

        public static Store ToEntity(this StoreModel model, Store destination)
        {
            return model.MapTo(destination);
        }

        #endregion

        #region Settings

        //customer/user settings
        public static CustomerUserSettingsModel.CustomerSettingsModel ToModel(this CustomerSettings entity)
        {
            return entity.MapTo<CustomerSettings, CustomerUserSettingsModel.CustomerSettingsModel>();
        }
        public static CustomerSettings ToEntity(this CustomerUserSettingsModel.CustomerSettingsModel model, CustomerSettings destination)
        {
            return model.MapTo(destination);
        }
        //public static CustomerUserSettingsModel.AddressSettingsModel ToModel(this AddressSettings entity)
        //{
        //    return entity.MapTo<AddressSettings, CustomerUserSettingsModel.AddressSettingsModel>();
        //}
        //public static AddressSettings ToEntity(this CustomerUserSettingsModel.AddressSettingsModel model, AddressSettings destination)
        //{
        //    return model.MapTo(destination);
        //}

        public static NewsSettingsModel ToModel(this NewsSettings entity)
        {
            return entity.MapTo<NewsSettings, NewsSettingsModel>();
        }
        public static NewsSettings ToEntity(this NewsSettingsModel model, NewsSettings destination)
        {
            return model.MapTo(destination);
        }


        #endregion

        #region News

        //news items
        public static NewsItemModel ToModel(this NewsItem entity)
        {
            return entity.MapTo<NewsItem, NewsItemModel>();
        }

        public static NewsItem ToEntity(this NewsItemModel model)
        {
            return model.MapTo<NewsItemModel, NewsItem>();
        }

        public static NewsItem ToEntity(this NewsItemModel model, NewsItem destination)
        {
            return model.MapTo(destination);
        }

        #endregion
    }
}