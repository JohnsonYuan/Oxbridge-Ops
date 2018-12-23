using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.BonusApp;
using Nop.Core.Domain.BonusApp.Configuration;
using Nop.Core.Domain.BonusApp.Customers;
using Nop.Core.Domain.BonusApp.Logging;
using Nop.Core.Domain.Common;
using Nop.Data;
using Nop.Services.Common;
using Nop.Services.Customers;

namespace Nop.Services.BonusApp.Logging
{
    /// <summary>
    /// Customer activity service
    /// </summary>
    public class CustomerActivityService : ICustomerActivityService
    {
        #region Constants

        /// <summary>
        /// Key for caching
        /// </summary>
        private const string ACTIVITYTYPE_ALL_KEY = "Nop.BonusApp.activitytype.all";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string ACTIVITYTYPE_PATTERN_KEY = "Nop.BonusApp.activitytype.";

        #endregion

        #region Fields

        /// <summary>
        /// Cache manager
        /// </summary>
        private readonly ICacheManager _cacheManager;
        private readonly IRepository<BonusAppStatus> _bonusAppStatusRepository;
        private readonly IRepository<BonusApp_ActivityLogType> _activityLogTypeRepository;
        private readonly IRepository<BonusApp_ActivityLog> _activityLogRepository;
        private readonly IRepository<BonusApp_MoneyLog> _moneyLogRepository;
        private readonly IWorkContext _workContext;
        private readonly IDbContext _dbContext;
        private readonly IDataProvider _dataProvider;
        private readonly BonusAppSettings _bonusAppSettings;
        private readonly IWebHelper _webHelper;
        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="activityLogRepository">Activity log repository</param>
        /// <param name="moneyLogRepository">Money log repository</param>
        /// <param name="activityLogTypeRepository">Activity log type repository</param>
        /// <param name="workContext">Work context</param>
        /// <param name="dbContext">DB context</param>>
        /// <param name="dataProvider">WeData provider</param>
        /// <param name="commonSettings">Common settings</param>
        /// <param name="webHelper">Web helper</param>
        public CustomerActivityService(ICacheManager cacheManager,
            IRepository<BonusAppStatus> bonusAppStatusRepository,
            IRepository<BonusApp_ActivityLog> activityLogRepository,
            IRepository<BonusApp_MoneyLog> moneyLogRepository,
            IRepository<BonusApp_ActivityLogType> activityLogTypeRepository,
            IWorkContext workContext,
            IDbContext dbContext, IDataProvider dataProvider,
            BonusAppSettings bonusAppSettings,
            IWebHelper webHelper,
            IGenericAttributeService genericAttributeService)
        {
            this._cacheManager = cacheManager;
            this._bonusAppStatusRepository = bonusAppStatusRepository;
            this._activityLogRepository = activityLogRepository;
            this._moneyLogRepository = moneyLogRepository;
            this._activityLogTypeRepository = activityLogTypeRepository;
            this._workContext = workContext;
            this._dbContext = dbContext;
            this._dataProvider = dataProvider;
            this._bonusAppSettings = bonusAppSettings;
            this._webHelper = webHelper;
        }

        #endregion

        #region Nested classes

        [Serializable]
        public class ActivityLogTypeForCaching
        {
            public int Id { get; set; }
            public string SystemKeyword { get; set; }
            public string Name { get; set; }
            public bool Enabled { get; set; }
        }

        #endregion

        #region Utitlies

        /// <summary>
        /// Gets all activity log types (class for caching)
        /// </summary>
        /// <returns>Activity log types</returns>
        protected virtual IList<ActivityLogTypeForCaching> GetAllActivityTypesCached()
        {
            //cache
            string key = string.Format(ACTIVITYTYPE_ALL_KEY);
            return _cacheManager.Get(key, () =>
            {
                var result = new List<ActivityLogTypeForCaching>();
                var activityLogTypes = GetAllActivityTypes();
                foreach (var alt in activityLogTypes)
                {
                    var altForCaching = new ActivityLogTypeForCaching
                    {
                        Id = alt.Id,
                        SystemKeyword = alt.SystemKeyword,
                        Name = alt.Name,
                        Enabled = alt.Enabled
                    };
                    result.Add(altForCaching);
                }
                return result;
            });
        }

        #endregion

        #region Methods

        /// <summary>
        /// Inserts an activity log type item
        /// </summary>
        /// <param name="activityLogType">Activity log type item</param>
        public virtual void InsertActivityType(BonusApp_ActivityLogType activityLogType)
        {
            if (activityLogType == null)
                throw new ArgumentNullException("activityLogType");

            _activityLogTypeRepository.Insert(activityLogType);
            _cacheManager.RemoveByPattern(ACTIVITYTYPE_PATTERN_KEY);
        }

        /// <summary>
        /// Updates an activity log type item
        /// </summary>
        /// <param name="activityLogType">Activity log type item</param>
        public virtual void UpdateActivityType(BonusApp_ActivityLogType activityLogType)
        {
            if (activityLogType == null)
                throw new ArgumentNullException("activityLogType");

            _activityLogTypeRepository.Update(activityLogType);
            _cacheManager.RemoveByPattern(ACTIVITYTYPE_PATTERN_KEY);
        }

        /// <summary>
        /// Deletes an activity log type item
        /// </summary>
        /// <param name="activityLogType">Activity log type</param>
        public virtual void DeleteActivityType(BonusApp_ActivityLogType activityLogType)
        {
            if (activityLogType == null)
                throw new ArgumentNullException("activityLogType");

            _activityLogTypeRepository.Delete(activityLogType);
            _cacheManager.RemoveByPattern(ACTIVITYTYPE_PATTERN_KEY);
        }

        /// <summary>
        /// Gets all activity log type items
        /// </summary>
        /// <returns>Activity log type items</returns>
        public virtual IList<BonusApp_ActivityLogType> GetAllActivityTypes()
        {
            var query = from alt in _activityLogTypeRepository.Table
                        orderby alt.Name
                        select alt;
            var activityLogTypes = query.ToList();
            return activityLogTypes;
        }

        /// <summary>
        /// Gets an activity log type item
        /// </summary>
        /// <param name="activityLogTypeId">Activity log type identifier</param>
        /// <returns>Activity log type item</returns>
        public virtual BonusApp_ActivityLogType GetActivityTypeById(int activityLogTypeId)
        {
            if (activityLogTypeId == 0)
                return null;

            return _activityLogTypeRepository.GetById(activityLogTypeId);
        }

        /// <summary>
        /// Inserts an activity log item
        /// </summary>
        /// <param name="systemKeyword">The system keyword</param>
        /// <param name="comment">The activity comment</param>
        /// <param name="commentParams">The activity comment parameters for string.Format() function.</param>
        /// <returns>Activity log item</returns>
        public virtual BonusApp_ActivityLog InsertActivity(string systemKeyword, string comment, params object[] commentParams)
        {
            return InsertActivity(_workContext.CurrentBonusAppCustomer, systemKeyword, comment, commentParams);
        }


        /// <summary>
        /// Inserts an activity log item
        /// </summary>
        /// <param name="customer">The customer</param>
        /// <param name="systemKeyword">The system keyword</param>
        /// <param name="comment">The activity comment</param>
        /// <param name="commentParams">The activity comment parameters for string.Format() function.</param>
        /// <returns>Activity log item</returns>
        public virtual BonusApp_ActivityLog InsertActivity(BonusApp_Customer customer, string systemKeyword, string comment, params object[] commentParams)
        {
            if (customer == null)
                return null;

            var activityTypes = GetAllActivityTypesCached();
            var activityType = activityTypes.ToList().Find(at => at.SystemKeyword == systemKeyword);
            if (activityType == null || !activityType.Enabled)
                return null;

            comment = CommonHelper.EnsureNotNull(comment);
            comment = string.Format(comment, commentParams);
            comment = CommonHelper.EnsureMaximumLength(comment, 4000);

            var activity = new BonusApp_ActivityLog();
            activity.ActivityLogTypeId = activityType.Id;
            activity.Customer = customer;
            activity.Comment = comment;
            activity.CreatedOnUtc = DateTime.UtcNow;
            activity.IpAddress = _webHelper.GetCurrentIpAddress();

            _activityLogRepository.Insert(activity);

            return activity;
        }

        /// <summary>
        /// Deletes an activity log item
        /// </summary>
        /// <param name="activityLog">Activity log type</param>
        public virtual void DeleteActivity(BonusApp_ActivityLog activityLog)
        {
            if (activityLog == null)
                throw new ArgumentNullException("activityLog");

            _activityLogRepository.Delete(activityLog);
        }

        /// <summary>
        /// Gets all activity log items
        /// </summary>
        /// <param name="createdOnFrom">Log item creation from; null to load all activities</param>
        /// <param name="createdOnTo">Log item creation to; null to load all activities</param>
        /// <param name="customerId">Customer identifier; null to load all activities</param>
        /// <param name="activityLogTypeId">Activity log type identifier</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="ipAddress">IP address; null or empty to load all activities</param>
        /// <returns>Activity log items</returns>
        public virtual IPagedList<BonusApp_ActivityLog> GetAllActivities(DateTime? createdOnFrom = null,
            DateTime? createdOnTo = null, int? customerId = null, int activityLogTypeId = 0,
            int pageIndex = 0, int pageSize = int.MaxValue, string ipAddress = null)
        {
            var query = _activityLogRepository.Table;
            if (!String.IsNullOrEmpty(ipAddress))
                query = query.Where(al => al.IpAddress.Contains(ipAddress));
            if (createdOnFrom.HasValue)
                query = query.Where(al => createdOnFrom.Value <= al.CreatedOnUtc);
            if (createdOnTo.HasValue)
                query = query.Where(al => createdOnTo.Value >= al.CreatedOnUtc);
            if (activityLogTypeId > 0)
                query = query.Where(al => activityLogTypeId == al.ActivityLogTypeId);
            if (customerId.HasValue)
                query = query.Where(al => customerId.Value == al.CustomerId);

            query = query.OrderByDescending(al => al.CreatedOnUtc);

            var activityLog = new PagedList<BonusApp_ActivityLog>(query, pageIndex, pageSize);
            return activityLog;
        }

        /// <summary>
        /// Gets all activity log items by types.
        /// </summary>
        public virtual IPagedList<BonusApp_ActivityLog> GetAllActivitiesByTypes(string[] logTypeSystemNames,
            DateTime? createdOnFrom = null,
            DateTime? createdOnTo = null,
            int? customerId = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            string ipAddress = null)
        {
            var query = _activityLogRepository.Table;

            // filter by log type
            if (logTypeSystemNames != null)
                query = query.Join(_activityLogTypeRepository.Table, x => x.ActivityLogTypeId, y => y.Id, (x, y) => new { Log = x, LogType = y })
                    .Where(z => logTypeSystemNames.Contains(z.LogType.SystemKeyword))
                    .Select(z => z.Log);
            if (createdOnFrom.HasValue)
                query = query.Where(al => createdOnFrom.Value <= al.CreatedOnUtc);
            if (createdOnTo.HasValue)
                query = query.Where(al => createdOnTo.Value >= al.CreatedOnUtc);
            if (customerId.HasValue)
                query = query.Where(al => customerId.Value == al.CustomerId);

            if (!String.IsNullOrEmpty(ipAddress))
                query = query.Where(al => al.IpAddress.Contains(ipAddress));

            query = query.OrderByDescending(al => al.CreatedOnUtc);

            var activityLog = new PagedList<BonusApp_ActivityLog>(query, pageIndex, pageSize);
            return activityLog;
        }

        /// <summary>
        /// Gets an activity log item
        /// </summary>
        /// <param name="activityLogId">Activity log identifier</param>
        /// <returns>Activity log item</returns>
        public virtual BonusApp_ActivityLog GetActivityById(int activityLogId)
        {
            if (activityLogId == 0)
                return null;

            return _activityLogRepository.GetById(activityLogId);
        }

        /// <summary>
        /// Clears activity log
        /// </summary>
        public virtual void ClearAllActivities()
        {
            if (_dataProvider.StoredProceduredSupported)
            {
                //although it's not a stored procedure we use it to ensure that a database supports them
                //we cannot wait until EF team has it implemented - http://data.uservoice.com/forums/72025-entity-framework-feature-suggestions/suggestions/1015357-batch-cud-support


                //do all databases support "Truncate command"?
                string activityLogTableName = _dbContext.GetTableName<BonusApp_ActivityLog>();
                _dbContext.ExecuteSqlCommand(String.Format("TRUNCATE TABLE [{0}]", activityLogTableName));
            }
            else
            {
                var activityLog = _activityLogRepository.Table.ToList();
                foreach (var activityLogItem in activityLog)
                    _activityLogRepository.Delete(activityLogItem);
            }
        }

        /// <summary>
        /// Inserts an money log item
        /// </summary>
        /// <param name="systemKeyword">The system keyword</param>
        /// <param name="moneyDelta"></param>
        /// <param name="comment">The activity comment</param>
        /// <param name="commentParams">The activity comment parameters for string.Format() function.</param>
        /// <returns>Money log item</returns>
        public virtual BonusApp_MoneyLog InsertMoneyLog(long moneyDelta, string comment, params object[] commentParams)
        {
            return InsertMoneyLog(_workContext.CurrentBonusAppCustomer, moneyDelta, comment, commentParams);
        }

        /// <summary>
        /// Insert money log
        /// </summary>
        /// <param name="customer"></param>
        /// <param name="systemKeyword"></param>
        /// <param name="comment"></param>
        /// <param name="commentParams"></param>
        /// <returns></returns>
        public virtual BonusApp_MoneyLog InsertMoneyLog(BonusApp_Customer customer, string comment, params object[] commentParams)
        {
            return InsertMoneyLog(customer, 0, comment, commentParams);
        }

        /// <summary>
        /// Inserts an monety log item
        /// </summary>
        /// <param name="customer">The customer</param>
        /// <param name="systemKeyword">The system keyword</param>
        /// <param name="comment">The activity comment</param>
        /// <param name="commentParams">The activity comment parameters for string.Format() function.</param>
        /// <returns>Money log item</returns>
        public virtual BonusApp_MoneyLog InsertMoneyLog(BonusApp_Customer customer, double moneyDelta, string comment, params object[] commentParams)
        {
            // bonus pool info
            var bonusPoolInfo = _bonusAppStatusRepository.Table.FirstOrDefault();
            if (bonusPoolInfo == null)
                throw new NopException("奖金池没有数据, 程序错误");

            if (customer == null)
                return null;
            comment = CommonHelper.EnsureNotNull(comment);
            comment = string.Format(comment, commentParams);
            comment = CommonHelper.EnsureMaximumLength(comment, 4000);

            var moneyLog = new BonusApp_MoneyLog();
            moneyLog.Customer = customer;
            moneyLog.Comment = comment;
            moneyLog.CreatedOnUtc = DateTime.UtcNow;
            moneyLog.IpAddress = _webHelper.GetCurrentIpAddress();
            // Bonus info
            moneyLog.Money = moneyDelta;
            moneyLog.ReturnMoney = moneyDelta * _bonusAppSettings.UserReturnMoneyPercent;   // current return 100%
            moneyLog.MoneyReturnStatusId = (int)BonusApp_MoneyReturnStatus.Waiting;

            // Bonus pool info
            double moneySaveToPool = moneyDelta * _bonusAppSettings.SaveToAppMoneyPercent;
            moneyLog.AppMoneyBefore = bonusPoolInfo.CurrentMoney;
            moneyLog.AppMoneyDelta = moneySaveToPool;
            moneyLog.AppMoneyAfter = bonusPoolInfo.CurrentMoney + moneySaveToPool;

            // insert money log
            _moneyLogRepository.Insert(moneyLog);

            // update pool info
            bonusPoolInfo.CurrentMoney += moneySaveToPool;
            bonusPoolInfo.WaitingUserCount += 1;
            _bonusAppStatusRepository.Update(bonusPoolInfo);

            return moneyLog;
        }

        /// <summary>
        /// Updates an Money log type item
        /// </summary>
        /// <param name="activityLogType">Money log type item</param>
        public virtual void UpdateMoneyLog(BonusApp_MoneyLog moneyLog)
        {
            if (moneyLog == null)
                throw new ArgumentNullException("moneyLog");

            _moneyLogRepository.Update(moneyLog);
        }

        /// <summary>
        /// Deletes an money log item
        /// </summary>
        /// <param name="moneyLog">Money log</param>
        public virtual void DeleteMoneyLog(BonusApp_MoneyLog moneyLog)
        {
            if (moneyLog == null)
                throw new ArgumentNullException("moneyLog");

            _moneyLogRepository.Delete(moneyLog);
        }

        /// <summary>
        /// Gets all money log items
        /// </summary>
        /// <param name="createdOnFrom">Log item creation from; null to load all activities</param>
        /// <param name="createdOnTo">Log item creation to; null to load all activities</param>
        /// <param name="customerId">Customer identifier; null to load all activities</param>
        /// <param name="activityLogTypeId">Activity log type identifier</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="ipAddress">IP address; null or empty to load all activities</param>
        /// <returns>Money log items</returns>
        public virtual IPagedList<BonusApp_MoneyLog> GetAllMoneyLogs(DateTime? createdOnFrom = null,
            DateTime? createdOnTo = null, int? customerId = null, int moneyReturnStatusId = 0,
            int pageIndex = 0, int pageSize = int.MaxValue, string ipAddress = null)
        {
            var query = _moneyLogRepository.Table;
            if (!String.IsNullOrEmpty(ipAddress))
                query = query.Where(al => al.IpAddress.Contains(ipAddress));
            if (createdOnFrom.HasValue)
                query = query.Where(al => createdOnFrom.Value <= al.CreatedOnUtc);
            if (createdOnTo.HasValue)
                query = query.Where(al => createdOnTo.Value >= al.CreatedOnUtc);
            if (moneyReturnStatusId > 0)
                query = query.Where(al => moneyReturnStatusId == al.MoneyReturnStatusId);
            if (customerId.HasValue)
                query = query.Where(al => customerId.Value == al.CustomerId);

            query = query.OrderByDescending(al => al.CreatedOnUtc);

            var moneyLog = new PagedList<BonusApp_MoneyLog>(query, pageIndex, pageSize);
            return moneyLog;
        }
        #endregion
    }
}
