using System;
using System.Collections.Generic;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Logging;

namespace Nop.Services.Logging
{
    /// <summary>
    /// Customer activity service interface
    /// </summary>
    public partial interface ICustomerActivityService
    {
        /// <summary>
        /// Inserts an activity log type item
        /// </summary>
        /// <param name="activityLogType">Activity log type item</param>
        void InsertActivityType(ActivityLogType activityLogType);

        /// <summary>
        /// Updates an activity log type item
        /// </summary>
        /// <param name="activityLogType">Activity log type item</param>
        void UpdateActivityType(ActivityLogType activityLogType);

        /// <summary>
        /// Deletes an activity log type item
        /// </summary>
        /// <param name="activityLogType">Activity log type</param>
        void DeleteActivityType(ActivityLogType activityLogType);

        /// <summary>
        /// Gets all activity log type items
        /// </summary>
        /// <returns>Activity log type items</returns>
        IList<ActivityLogType> GetAllActivityTypes();

        /// <summary>
        /// Gets an activity log type item
        /// </summary>
        /// <param name="activityLogTypeId">Activity log type identifier</param>
        /// <returns>Activity log type item</returns>
        ActivityLogType GetActivityTypeById(int activityLogTypeId);

        /// <summary>
        /// Inserts an activity log item
        /// </summary>
        /// <param name="systemKeyword">The system keyword</param>
        /// <param name="comment">The activity comment</param>
        /// <param name="commentParams">The activity comment parameters for string.Format() function.</param>
        /// <returns>Activity log item</returns>
        ActivityLog InsertActivity(string systemKeyword, string comment, params object[] commentParams);

        /// <summary>
        /// Inserts an activity log item
        /// </summary>
        /// <param name="customer">The customer</param>
        /// <param name="systemKeyword">The system keyword</param>
        /// <param name="comment">The activity comment</param>
        /// <param name="commentParams">The activity comment parameters for string.Format() function.</param>
        /// <returns>Activity log item</returns>
        ActivityLog InsertActivity(Customer customer, string systemKeyword, string comment, params object[] commentParams);

        /// <summary>
        /// Deletes an activity log item
        /// </summary>
        /// <param name="activityLog">Activity log</param>
        void DeleteActivity(ActivityLog activityLog);

        /// <summary>
        /// Gets all activity log items
        /// </summary>
        /// <param name="createdOnFrom">Log item creation from; null to load all customers</param>
        /// <param name="createdOnTo">Log item creation to; null to load all customers</param>
        /// <param name="customerId">Customer identifier; null to load all customers</param>
        /// <param name="activityLogTypeId">Activity log type identifier</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="ipAddress">IP address; null or empty to load all customers</param>
        /// <returns>Activity log items</returns>
        IPagedList<ActivityLog> GetAllActivities(DateTime? createdOnFrom = null,
            DateTime? createdOnTo = null, int? customerId = null, int activityLogTypeId = 0,
            int pageIndex = 0, int pageSize = int.MaxValue, string ipAddress = null);

        /// <summary>
        /// Gets all activity log items by types.
        /// </summary>
        IPagedList<ActivityLog> GetAllActivitiesByTypes(string[] logTypeSystemNames,
            DateTime? createdOnFrom = null,
            DateTime? createdOnTo = null,
            int? customerId = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            string ipAddress = null);

        /// <summary>
        /// Gets an activity log item
        /// </summary>
        /// <param name="activityLogId">Activity log identifier</param>
        /// <returns>Activity log item</returns>
        ActivityLog GetActivityById(int activityLogId);

        /// <summary>
        /// Clears activity log
        /// </summary>
        void ClearAllActivities();

        /// <summary>
        /// Inserts an money log item
        /// </summary>
        /// <param name="systemKeyword">The system keyword</param>
        /// <param name="moneyDelta"></param>
        /// <param name="comment">The activity comment</param>
        /// <param name="commentParams">The activity comment parameters for string.Format() function.</param>
        /// <returns>Money log item</returns>
        MoneyLog InsertMoneyLog(string systemKeyword, long moneyDelta, string comment, params object[] commentParams);

        /// <summary>
        /// Insert money log with delta 0.
        /// </summary>
        /// <param name="customer"></param>
        /// <param name="systemKeyword"></param>
        /// <param name="comment"></param>
        /// <param name="commentParams"></param>
        /// <returns></returns>
        MoneyLog InsertMoneyLog(Customer customer, string systemKeyword, string comment, params object[] commentParams);
        /// <summary>
        /// Inserts an monety log item
        /// </summary>
        /// <param name="customer">The customer</param>
        /// <param name="systemKeyword">The system keyword</param>
        /// <param name="comment">The activity comment</param>
        /// <param name="commentParams">The activity comment parameters for string.Format() function.</param>
        /// <returns>Activity log item</returns>
        MoneyLog InsertMoneyLog(Customer customer, string systemKeyword, long moneyDelta, string comment, params object[] commentParams);

        /// <summary>
        /// Deletes an money log item
        /// </summary>
        /// <param name="activityLog">Money log</param>
        void DeleteMoneyLog(MoneyLog moneyLog);

        /// <summary>
        /// Gets all money log items
        /// </summary>
        /// <param name="createdOnFrom">Log item creation from; null to load all customers</param>
        /// <param name="createdOnTo">Log item creation to; null to load all customers</param>
        /// <param name="customerId">Customer identifier; null to load all customers</param>
        /// <param name="activityLogTypeId">Activity log type identifier</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="ipAddress">IP address; null or empty to load all customers</param>
        /// <returns>Activity log items</returns>
        IPagedList<MoneyLog> GetAllMoneyLogs(DateTime? createdOnFrom = null,
            DateTime? createdOnTo = null, int? customerId = null, int activityLogTypeId = 0,
            int pageIndex = 0, int pageSize = int.MaxValue, string ipAddress = null);

        #region withdraw

        WithdrawLog InsertWithdraw(Customer customer, int amount, string comment, params object[] commentParams);
        WithdrawLog InsertWithdraw(int amount, string comment, params object[] commentParams);
        void UpdateWithdrawLog(WithdrawLog withdrawLog);
        void DeleteWithdraw(WithdrawLog withdrawLog);
        WithdrawLog GetWithdrawById(int withdrawLogId);
        IPagedList<WithdrawLog> GetAllWithdraws(DateTime? createdOnFrom = null, DateTime? createdOnTo = null, int? customerId = null, bool? isDone = false, int pageIndex = 0, int pageSize = int.MaxValue, string ipAddress = null);

        void ClearAllWithdraws();
        #endregion
    }
}
