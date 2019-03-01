using System;
using System.Linq;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.BonusApp;
using Nop.Services.BonusApp.Customers;
using Nop.Services.BonusApp.Logging;

namespace Nop.Services.ZhiXiao.BonusApp
{
    public class BonusAppService : IBonusAppService
    {
        private readonly IRepository<BonusAppStatus> _bonusAppStatusRepository;
        private readonly IBonusApp_CustomerService _customerService;

        public BonusAppService(IRepository<BonusAppStatus> bonusAppStatusRepository,
            IBonusApp_CustomerService customerService)
        {
            this._bonusAppStatusRepository = bonusAppStatusRepository;
            this._customerService = customerService;
        }

        /// <summary>
        /// Insert bonus app status
        /// </summary>
        /// <param name="bonusAppStatus"></param>
        public virtual void InsertBonusAppStatus(BonusAppStatus bonusAppStatus)
        {
            if (bonusAppStatus == null)
                throw new ArgumentNullException("bonusAppStatus");

            _bonusAppStatusRepository.Insert(bonusAppStatus);
        }

        /// <summary>
        /// Updates an bonusAppStatus item
        /// </summary>
        /// <param name="bonusAppStatus">BonusAppStatus</param>
        public virtual void UpdateBonusAppStatus(BonusAppStatus bonusAppStatus)
        {
            if (bonusAppStatus == null)
                throw new ArgumentNullException("bonusAppStatus");

            _bonusAppStatusRepository.Update(bonusAppStatus);
        }

        /// <summary>
        /// Deletes an activity log type item
        /// </summary>
        /// <param name="bonusAppStatus">BonusAppStatus</param>
        public virtual void DeleteActivityType(BonusAppStatus bonusAppStatus)
        {
            if (bonusAppStatus == null)
                throw new ArgumentNullException("bonusAppStatus");

            _bonusAppStatusRepository.Delete(bonusAppStatus);
        }

        public BonusAppStatus GetAppStatus()
        {
            var firstItem = _bonusAppStatusRepository.Table.FirstOrDefault();

            if (firstItem == null)
                throw new NopException("BonusAppStatus No Data");

            return firstItem;
        }

        /// <summary>
        /// Return money if needed.
        /// </summary>
        public void ReturnUserMoneyIfNeeded(IBonusApp_CustomerActivityService activityService)
        {
            var appStatus = GetAppStatus();

            if (appStatus.CurrentMoney <= 0)
                return;

            // has waiting request?
            var firstWaitingLog = activityService.GetFirstWaitingLog();

            if (firstWaitingLog == null)
                return;

            // can return money to user
            if (appStatus.CurrentMoney >= firstWaitingLog.ReturnMoney)
            {
                // request complete
                firstWaitingLog.MoneyReturnStatus = Core.Domain.BonusApp.Logging.BonusApp_MoneyReturnStatus.Complete;
                firstWaitingLog.CompleteOnUtc = DateTime.UtcNow;
                // return money to user, and save notification info(log id, money)
                firstWaitingLog.Customer.Money += firstWaitingLog.ReturnMoney;
                // 提示用户已经奖励
                firstWaitingLog.Customer.NotificationMoneyLogId = firstWaitingLog.Id;
                firstWaitingLog.Customer.NotificationMoney = firstWaitingLog.ReturnMoney;
                // 当前用户可以评论(null, false时更新)
                if (!firstWaitingLog.Customer.CanComment.HasValue
                    || !firstWaitingLog.Customer.CanComment.Value)
                {
                    firstWaitingLog.Customer.CanComment = true;
                }

                // complete time
                firstWaitingLog.CompleteOnUtc = DateTime.UtcNow;
                // update
                activityService.UpdateMoneyLog(firstWaitingLog);

                // 奖金池log
                activityService.InsertActivity(firstWaitingLog.Customer,
                    BonusAppConstants.LogType_Bonus_PoolMinus,
                    "用户{0}({1}) 返回金额{2}, 奖金池-{3}, {4} -> {5}",
                    firstWaitingLog.Customer.Nickname, firstWaitingLog.Customer.Id,
                    firstWaitingLog.ReturnMoney, // 返回金额{2}
                    appStatus.CurrentMoney, // 奖金池-{3}
                    appStatus.CurrentMoney,
                    appStatus.CurrentMoney - firstWaitingLog.ReturnMoney);

                // update pool
                appStatus.CurrentMoney -= firstWaitingLog.ReturnMoney;
                appStatus.WaitingUserCount -= 1;
                appStatus.CompleteUserCount += 1;
                appStatus.MoneyPaied += firstWaitingLog.ReturnMoney;
                _bonusAppStatusRepository.Update(appStatus);

                // recursive check if can return money to next user 
                ReturnUserMoneyIfNeeded(activityService);
            }
        }
    }
}