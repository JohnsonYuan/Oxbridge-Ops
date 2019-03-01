using System;
using System.Collections.Generic;
using Nop.Core;
using Nop.Core.Domain.BonusApp.Customers;
using Nop.Core.Domain.BonusApp.Logging;

namespace Nop.Services.BonusApp.Logging
{
    /// <summary>
    /// Customer activity service
    /// </summary>
    public interface IBonusApp_CustomerActivityService
    {
        void ClearAllActivities();
        void DeleteActivity(BonusApp_ActivityLog activityLog);
        void DeleteActivityType(BonusApp_ActivityLogType activityLogType);
        void DeleteMoneyLog(BonusApp_MoneyLog moneyLog);
        BonusApp_ActivityLog GetActivityById(int activityLogId);
        BonusApp_ActivityLogType GetActivityTypeById(int activityLogTypeId);
        IPagedList<BonusApp_ActivityLog> GetAllActivities(DateTime? createdOnFrom = null, DateTime? createdOnTo = null, int? customerId = null, int activityLogTypeId = 0, int pageIndex = 0, int pageSize = int.MaxValue, string ipAddress = null);
        IPagedList<BonusApp_ActivityLog> GetAllActivitiesByTypes(string[] logTypeSystemNames, DateTime? createdOnFrom = null, DateTime? createdOnTo = null, int? customerId = null, int pageIndex = 0, int pageSize = int.MaxValue, string ipAddress = null);
        IList<BonusApp_ActivityLogType> GetAllActivityTypes();

        /// <summary>
        /// 当前log的排序id
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        int GetMoneyLogOrderNumber(BonusApp_MoneyLog log);
        void ClearMoneyLogCache();
        BonusApp_ActivityLog InsertActivity(BonusApp_Customer customer, string systemKeyword, string comment, params object[] commentParams);
        BonusApp_ActivityLog InsertActivity(string systemKeyword, string comment, params object[] commentParams);
        void InsertActivityType(BonusApp_ActivityLogType activityLogType);
        /// <summary>
        /// 奖金放入奖金池(MoneeyLog, AppStatus)
        /// </summary>
        /// <param name="customer"></param>
        /// <param name="moneyDelta"></param>
        /// <param name="comment"></param>
        /// <param name="commentParams"></param>
        /// <returns></returns>
        BonusApp_MoneyLog InsertMoneyLog(BonusApp_Customer customer, decimal moneyDelta, string comment, params object[] commentParams);
        BonusApp_MoneyLog InsertMoneyLog(BonusApp_Customer customer, string comment, params object[] commentParams);
        BonusApp_MoneyLog InsertMoneyLog(long moneyDelta, string comment, params object[] commentParams);
        void UpdateActivityType(BonusApp_ActivityLogType activityLogType);
        void UpdateMoneyLog(BonusApp_MoneyLog moneyLog);

        /// <summary>
        /// 返回最近一次成功的记录, 显示在首页消息提醒
        /// </summary>
        /// <returns></returns>
        BonusApp_MoneyLog GetLastCompleteLog();

        /// <summary>
        /// Return first waiting log
        /// </summary>
        /// <returns></returns>
        BonusApp_MoneyLog GetFirstWaitingLog();

        IPagedList<BonusApp_MoneyLog> GetAllMoneyLogs(DateTime? createdOnFrom = null, 
            DateTime? createdOnTo = null, 
            int? customerId = null, 
            int? moneyReturnStatusId = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue, 
            string ipAddress = null);
       
        IPagedList<BonusApp_MoneyLog> GetMoneyLogByType(
            BonusApp_MoneyReturnStatus moneyReturnStatus,
            int pageIndex = 0, int pageSize = int.MaxValue);
        /// <summary>
        /// Waiting log
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        IPagedList<BonusApp_MoneyLog> GetWaitingMoneyLog(int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Complete log
        /// </summary>
        IPagedList<BonusApp_MoneyLog> GetCompleteMoneyLog(int pageIndex = 0, int pageSize = int.MaxValue);

        #region Withdraw

        BonusApp_WithdrawLog InsertWithdraw(decimal amount, string comment, params object[] commentParams);

        /// <summary>
        /// Inserts an withdraw log item
        /// </summary>
        /// <param name="customer">The customer</param>
        /// <param name="amount">The withdraw amount</param>
        /// <param name="comment">The activity comment</param>
        /// <param name="commentParams">The activity comment parameters for string.Format() function.</param>
        /// <returns>Withdraw log item</returns>
        BonusApp_WithdrawLog InsertWithdraw(BonusApp_Customer customer, decimal amount, string comment, params object[] commentParams);

        /// <summary>
        /// Update withdraw log item
        /// </summary>
        /// <param name="BonusApp_WithdrawLog"></param>
        void UpdateWithdrawLog(BonusApp_WithdrawLog withdrawLog);

        /// <summary>
        /// Deletes an withdraw log item
        /// </summary>
        /// <param name="BonusApp_WithdrawLog">Activity log type</param>
        void DeleteWithdraw(BonusApp_WithdrawLog withdrawLog);

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
        IPagedList<BonusApp_WithdrawLog> GetAllWithdraws(DateTime? createdOnFrom = null,
            DateTime? createdOnTo = null, int? customerId = null, bool? isDone = null,
            int pageIndex = 0, int pageSize = int.MaxValue, string ipAddress = null);

        /// <summary>
        /// Gets an withdraw log item
        /// </summary>
        /// <param name="withdrawLogId">withdrawLog log identifier</param>
        /// <returns>Activity log item</returns>
        BonusApp_WithdrawLog GetWithdrawById(int withdrawLogId);

        /// <summary>
        /// Clears Withdraw log
        /// </summary>
        void ClearAllWithdraws();

        #endregion
    }
}