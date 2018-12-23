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
    public interface ICustomerActivityService
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
        IPagedList<BonusApp_MoneyLog> GetAllMoneyLogs(DateTime? createdOnFrom = null, DateTime? createdOnTo = null, int? customerId = null, int moneyReturnStatusId = 0, int pageIndex = 0, int pageSize = int.MaxValue, string ipAddress = null);
        BonusApp_ActivityLog InsertActivity(BonusApp_Customer customer, string systemKeyword, string comment, params object[] commentParams);
        BonusApp_ActivityLog InsertActivity(string systemKeyword, string comment, params object[] commentParams);
        void InsertActivityType(BonusApp_ActivityLogType activityLogType);
        BonusApp_MoneyLog InsertMoneyLog(BonusApp_Customer customer, double moneyDelta, string comment, params object[] commentParams);
        BonusApp_MoneyLog InsertMoneyLog(BonusApp_Customer customer, string comment, params object[] commentParams);
        BonusApp_MoneyLog InsertMoneyLog(long moneyDelta, string comment, params object[] commentParams);
        void UpdateActivityType(BonusApp_ActivityLogType activityLogType);
        void UpdateMoneyLog(BonusApp_MoneyLog moneyLog);

    }
}