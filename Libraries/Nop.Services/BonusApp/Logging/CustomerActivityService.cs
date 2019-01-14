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
using Nop.Data;
using Nop.Services.ZhiXiao.BonusApp;

namespace Nop.Services.BonusApp.Logging
{
    /// <summary>
    /// Customer activity service
    /// </summary>
    public class BonusApp_CustomerActivityService : IBonusApp_CustomerActivityService
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

        /// <summary>
        /// Key for caching
        /// </summary>
        private const string MONEYLOG_ALL_KEY = "Nop.BonusApp.moneylog.all";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string MONEYLOG_PATTERN_KEY = "Nop.BonusApp.moneylog.";

        #endregion

        #region Fields

        private readonly object _myLock = new object();

        /// <summary>
        /// Cache manager
        /// </summary>
        private readonly ICacheManager _cacheManager;
        private readonly IRepository<BonusAppStatus> _bonusAppStatusRepository;
        private readonly IRepository<BonusApp_ActivityLogType> _activityLogTypeRepository;
        private readonly IRepository<BonusApp_ActivityLog> _activityLogRepository;
        private readonly IRepository<BonusApp_MoneyLog> _moneyLogRepository;
        private readonly IRepository<BonusApp_WithdrawLog> _withdrawLogRepository;
        private readonly IWorkContext _workContext;
        private readonly IDbContext _dbContext;
        private readonly IDataProvider _dataProvider;
        private readonly BonusAppSettings _bonusAppSettings;
        private readonly IWebHelper _webHelper;
        private readonly IBonusAppService _bonusAppService;
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
        public BonusApp_CustomerActivityService(ICacheManager cacheManager,
            IRepository<BonusAppStatus> bonusAppStatusRepository,
            IRepository<BonusApp_ActivityLog> activityLogRepository,
            IRepository<BonusApp_MoneyLog> moneyLogRepository,
            IRepository<BonusApp_ActivityLogType> activityLogTypeRepository,
            IRepository<BonusApp_WithdrawLog> withdrawLogRepository,
            IWorkContext workContext,
            IDbContext dbContext, IDataProvider dataProvider,
            BonusAppSettings bonusAppSettings,
            IWebHelper webHelper,
            IBonusAppService bonusAppService)
        {
            this._cacheManager = cacheManager;
            this._bonusAppStatusRepository = bonusAppStatusRepository;
            this._activityLogRepository = activityLogRepository;
            this._moneyLogRepository = moneyLogRepository;
            this._activityLogTypeRepository = activityLogTypeRepository;
            this._withdrawLogRepository = withdrawLogRepository;
            this._workContext = workContext;
            this._dbContext = dbContext;
            this._dataProvider = dataProvider;
            this._bonusAppSettings = bonusAppSettings;
            this._webHelper = webHelper;
            this._bonusAppService = bonusAppService;
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

        protected virtual IList<BonusApp_MoneyLog> GetAllMoneyLogsCached()
        {
            //cache
            string key = string.Format(MONEYLOG_ALL_KEY);
            return _cacheManager.Get(key, () =>
            {
                var result = GetAllMoneyLogs();
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

        public virtual void ClearMoneyLogCache()
        {
            // clear cache
            _cacheManager.RemoveByPattern(MONEYLOG_PATTERN_KEY);
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
        public virtual BonusApp_MoneyLog InsertMoneyLog(BonusApp_Customer customer, decimal moneyDelta, string comment, params object[] commentParams)
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

            // 防止同时请求CreatedOnUtc时间相同 排序依赖于CreatedOnUtc
            lock (_myLock)
            {
                var moneyLog = new BonusApp_MoneyLog();
                moneyLog.Customer = customer;
                moneyLog.Comment = comment;
                moneyLog.CreatedOnUtc = DateTime.UtcNow;
                moneyLog.IpAddress = _webHelper.GetCurrentIpAddress();
                // Bonus info
                moneyLog.Money = moneyDelta;
                moneyLog.ReturnMoney = Math.Round(moneyDelta * (decimal)_bonusAppSettings.UserReturnMoneyPercent, 2);   // current return 100%
                moneyLog.MoneyReturnStatusId = (int)BonusApp_MoneyReturnStatus.Waiting;

                // Bonus pool info
                decimal moneySaveToPool = Math.Round(moneyDelta * (decimal)_bonusAppSettings.SaveToAppMoneyPercent, 2);
                moneyLog.AppMoneyBefore = bonusPoolInfo.CurrentMoney;
                moneyLog.AppMoneyDelta = moneySaveToPool;
                moneyLog.AppMoneyAfter = bonusPoolInfo.CurrentMoney + moneySaveToPool;

                // insert money log
                _moneyLogRepository.Insert(moneyLog);

                // 奖金池金额增加 log
                InsertActivity(customer,
                    BonusAppConstants.LogType_Bonus_PoolAdd,
                    "用户{0}({1}) 消费{2}, 奖金池+{3}, {4} -> {5}",
                    customer.Nickname, customer.Id,
                    moneyDelta, // 消费{2}
                    moneyLog.AppMoneyDelta, // +{3}
                    moneyLog.AppMoneyBefore,
                    moneyLog.AppMoneyAfter);

                // update pool info
                bonusPoolInfo.CurrentMoney += moneySaveToPool;
                bonusPoolInfo.WaitingUserCount += 1;
                bonusPoolInfo.AllUserMoney += moneyDelta; // all user's money
                _bonusAppStatusRepository.Update(bonusPoolInfo);

                // 是否需要返还金额
                _bonusAppService.ReturnUserMoneyIfNeeded(this);

                // clear cache
                _cacheManager.RemoveByPattern(MONEYLOG_PATTERN_KEY);
                return moneyLog;
            }
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
            // clear cache
            _cacheManager.RemoveByPattern(MONEYLOG_PATTERN_KEY);
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
            // clear cache
            _cacheManager.RemoveByPattern(MONEYLOG_PATTERN_KEY);
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
            DateTime? createdOnTo = null, int? customerId = null, int? moneyReturnStatusId = null,
            int pageIndex = 0, int pageSize = int.MaxValue, string ipAddress = null)
        {
            var query = _moneyLogRepository.Table;
            if (!String.IsNullOrEmpty(ipAddress))
                query = query.Where(al => al.IpAddress.Contains(ipAddress));
            if (createdOnFrom.HasValue)
                query = query.Where(al => createdOnFrom.Value <= al.CreatedOnUtc);
            if (createdOnTo.HasValue)
                query = query.Where(al => createdOnTo.Value >= al.CreatedOnUtc);
            if (moneyReturnStatusId.HasValue)
                query = query.Where(al => moneyReturnStatusId.Value == al.MoneyReturnStatusId);
            if (customerId.HasValue)
                query = query.Where(al => customerId.Value == al.CustomerId);
            // load customer info
            query = query.IncludeProperties(al => al.Customer);

            query = query.OrderBy(al => al.CreatedOnUtc);

            var moneyLog = new PagedList<BonusApp_MoneyLog>(query, pageIndex, pageSize);
            return moneyLog;
        }

        /// <summary>
        /// 当前log的排序id
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        public virtual int GetMoneyLogOrderNumber(BonusApp_MoneyLog log)
        {
            var allLogs = GetAllMoneyLogsCached();
            var curretTypeLogs = allLogs.Where(x => x.MoneyReturnStatus == log.MoneyReturnStatus)
                .ToList();

            // 当前在缓存中位置
            var findLog = curretTypeLogs.FirstOrDefault(x => x.Id == log.Id);

            return curretTypeLogs.IndexOf(findLog) + 1;

            // 当前log status的排序
            string tableName = _dbContext.GetTableName<BonusApp_MoneyLog>();
            /*
             * // 计算当前log的排序
             SELECT t.num FROM (
                SELECT CONVERT(INT,ROW_NUMBER() OVER(ORDER BY CreatedOnUtc asc)) AS num, Id FROM BonusApp_MoneyLog 
                ) t
                WHERE t.Id=15
             */
            var result = _dbContext.SqlQuery<int>(string.Format("SELECT t.num FROM (SELECT CONVERT(INT,ROW_NUMBER() OVER(ORDER BY CreatedOnUtc asc)) AS num, Id FROM {0} WHERE MoneyReturnStatusId={1}) t WHERE t.Id={2}", 
                tableName, 
                log.MoneyReturnStatusId,
                log.Id));
            return result.FirstOrDefault();
        }

        #region Bonus Logic

        /// <summary>
        /// Order by create time
        /// </summary>
        /// <returns></returns>
        public BonusApp_MoneyLog GetFirstWaitingLog()
        {
            // 默认orderby CreatedOnUtc asc
            //var allLogs = GetAllMoneyLogsCached();

            //return allLogs.FirstOrDefault(l => l.MoneyReturnStatus == BonusApp_MoneyReturnStatus.Waiting);
            var statusId = (int)BonusApp_MoneyReturnStatus.Waiting;
            var query = from l in _moneyLogRepository.Table
                        where l.MoneyReturnStatusId == statusId
                        orderby l.CreatedOnUtc ascending
                        select l;

            return query.FirstOrDefault();
        }

        /// <summary>
        /// Return waiting/complete users
        /// </summary>
        /// <param name="moneyStatus"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public IPagedList<BonusApp_MoneyLog> GetMoneyLogByType(
            BonusApp_MoneyReturnStatus moneyReturnStatus,
            int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var allLogs = GetAllMoneyLogsCached();

            var query = allLogs.Where(x => x.MoneyReturnStatus == moneyReturnStatus)
                .OrderBy(al => al.CreatedOnUtc);

            // Include customer
            // query = query.IncludeProperties(al => al.Customer);

            var moneyLog = new PagedList<BonusApp_MoneyLog>(query.AsQueryable(), pageIndex, pageSize);
            return moneyLog;
        }

        /// <summary>
        /// Waiting log
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public IPagedList<BonusApp_MoneyLog> GetWaitingMoneyLog(int pageIndex = 0, int pageSize = int.MaxValue)
        {
            return GetMoneyLogByType(BonusApp_MoneyReturnStatus.Waiting,
                pageIndex: pageIndex,
                pageSize: pageSize);
        }

        /// <summary>
        /// Complete log
        /// </summary>
        public IPagedList<BonusApp_MoneyLog> GetCompleteMoneyLog(int pageIndex = 0, int pageSize = int.MaxValue)
        {
            return GetMoneyLogByType(BonusApp_MoneyReturnStatus.Complete,
                pageIndex: pageIndex,
                pageSize: pageSize);
        }

        #endregion

        #region Withdraw log

        /// <summary>
        /// Inserts an withdraw log item
        /// </summary>
        /// <param name="amount">The withdraw amount</param>
        /// <param name="comment">The activity comment</param>
        /// <param name="commentParams">The activity comment parameters for string.Format() function.</param>
        /// <returns>Activity log item</returns>
        public virtual BonusApp_WithdrawLog InsertWithdraw(decimal amount, string comment, params object[] commentParams)
        {
            return InsertWithdraw(_workContext.CurrentBonusAppCustomer, amount, comment, commentParams);
        }

        /// <summary>
        /// Inserts an withdraw log item
        /// </summary>
        /// <param name="customer">The customer</param>
        /// <param name="amount">The withdraw amount</param>
        /// <param name="comment">The activity comment</param>
        /// <param name="commentParams">The activity comment parameters for string.Format() function.</param>
        /// <returns>Withdraw log item</returns>
        public virtual BonusApp_WithdrawLog InsertWithdraw(BonusApp_Customer customer, decimal amount, string comment, params object[] commentParams)
        {
            if (customer == null)
                return null;

            comment = CommonHelper.EnsureNotNull(comment);
            comment = string.Format(comment, commentParams);
            comment = CommonHelper.EnsureMaximumLength(comment, 4000);

            var withdraw = new BonusApp_WithdrawLog();
            withdraw.Amount = amount;
            withdraw.IsDone = false;
            withdraw.Customer = customer;
            withdraw.Comment = comment;
            withdraw.CreatedOnUtc = DateTime.UtcNow;
            withdraw.IpAddress = _webHelper.GetCurrentIpAddress();

            _withdrawLogRepository.Insert(withdraw);

            return withdraw;
        }

        /// <summary>
        /// Update withdraw log item
        /// </summary>
        /// <param name="BonusApp_WithdrawLog"></param>
        public virtual void UpdateWithdrawLog(BonusApp_WithdrawLog withdrawLog)
        {
            if (withdrawLog == null)
                throw new ArgumentNullException("withdrawLog");

            _withdrawLogRepository.Update(withdrawLog);
        }

        /// <summary>
        /// Deletes an withdraw log item
        /// </summary>
        /// <param name="BonusApp_WithdrawLog">Activity log type</param>
        public virtual void DeleteWithdraw(BonusApp_WithdrawLog withdrawLog)
        {
            if (withdrawLog == null)
                throw new ArgumentNullException("withdrawLog");

            _withdrawLogRepository.Delete(withdrawLog);
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
        public virtual IPagedList<BonusApp_WithdrawLog> GetAllWithdraws(DateTime? createdOnFrom = null,
            DateTime? createdOnTo = null, int? customerId = null, bool? isDone = null,
            int pageIndex = 0, int pageSize = int.MaxValue, string ipAddress = null)
        {
            var query = _withdrawLogRepository.Table;
            if (!String.IsNullOrEmpty(ipAddress))
                query = query.Where(al => al.IpAddress.Contains(ipAddress));
            if (createdOnFrom.HasValue)
                query = query.Where(al => createdOnFrom.Value <= al.CreatedOnUtc);
            if (createdOnTo.HasValue)
                query = query.Where(al => createdOnTo.Value >= al.CreatedOnUtc);
            if (isDone.HasValue)
                query = query.Where(al => al.IsDone == isDone.Value);
            if (customerId.HasValue)
                query = query.Where(al => customerId.Value == al.CustomerId);

            query = query.OrderByDescending(al => al.CreatedOnUtc);

            var withdrawLog = new PagedList<BonusApp_WithdrawLog>(query, pageIndex, pageSize);
            return withdrawLog;
        }

        /// <summary>
        /// Gets an withdraw log item
        /// </summary>
        /// <param name="withdrawLogId">withdrawLog log identifier</param>
        /// <returns>Activity log item</returns>
        public virtual BonusApp_WithdrawLog GetWithdrawById(int withdrawLogId)
        {
            if (withdrawLogId == 0)
                return null;

            return _withdrawLogRepository.GetById(withdrawLogId);
        }

        /// <summary>
        /// Clears Withdraw log
        /// </summary>
        public virtual void ClearAllWithdraws()
        {
            if (_dataProvider.StoredProceduredSupported)
            {
                //although it's not a stored procedure we use it to ensure that a database supports them
                //we cannot wait until EF team has it implemented - http://data.uservoice.com/forums/72025-entity-framework-feature-suggestions/suggestions/1015357-batch-cud-support


                //do all databases support "Truncate command"?
                string WithdrawTableName = _dbContext.GetTableName<BonusApp_WithdrawLog>();
                _dbContext.ExecuteSqlCommand(String.Format("TRUNCATE TABLE [{0}]", WithdrawTableName));
            }
            else
            {
                var withdrawLogs = _withdrawLogRepository.Table.ToList();
                foreach (var withdrawLogItem in withdrawLogs)
                    _withdrawLogRepository.Delete(withdrawLogItem);
            }
        }

        #endregion

        #endregion
    }
}
