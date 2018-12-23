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
        private readonly IBonusApp_CustomerActivityService _activityService;

        public BonusAppService(IRepository<BonusAppStatus> bonusAppStatusRepository,
            IBonusApp_CustomerService customerService,
            IBonusApp_CustomerActivityService activityService)
        {
            this._bonusAppStatusRepository = bonusAppStatusRepository;
            this._customerService = customerService;
            this._activityService = activityService;
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
        public void ReturnUserMoneyIfNeeded()
        {
            var appStatus = GetAppStatus();

            if (appStatus.CurrentMoney <= 0)
                return;

            // has waiting request?
            var firstWaitingUser = _activityService.GetFirstWaitingLog();

            if (firstWaitingUser == null)
                return;

            // can return money to user
            if (appStatus.CurrentMoney >= firstWaitingUser.ReturnMoney)
            {
                // request complete
                firstWaitingUser.MoneyReturnStatus = Core.Domain.BonusApp.Logging.BonusApp_MoneyReturnStatus.Complete;
                // return money to user
                firstWaitingUser.Customer.Money += firstWaitingUser.ReturnMoney;
                // update
                _activityService.UpdateMoneyLog(firstWaitingUser);

                // update pool
                appStatus.CurrentMoney -= firstWaitingUser.ReturnMoney;
                appStatus.WaitingUserCount -= 1;
                appStatus.CompleteUserCount += 1;
                appStatus.MoneyPaied += firstWaitingUser.ReturnMoney;
                _bonusAppStatusRepository.Update(appStatus);

                // recursive check if can return money to next user 
                ReturnUserMoneyIfNeeded();
            }
        }
    }
}