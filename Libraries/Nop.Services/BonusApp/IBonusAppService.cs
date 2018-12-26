using Nop.Core.Domain.BonusApp;
using Nop.Services.BonusApp.Logging;

namespace Nop.Services.ZhiXiao.BonusApp
{
    public interface IBonusAppService
    {
        void InsertBonusAppStatus(BonusAppStatus bonusAppStatus);

        /// <summary>
        /// Updates an bonusAppStatus item
        /// </summary>
        /// <param name="bonusAppStatus">BonusAppStatus</param>
        void UpdateBonusAppStatus(BonusAppStatus bonusAppStatus);

        /// <summary>
        /// Deletes an activity log type item
        /// </summary>
        /// <param name="bonusAppStatus">BonusAppStatus</param>
        void DeleteActivityType(BonusAppStatus bonusAppStatus);
        BonusAppStatus GetAppStatus();
        void ReturnUserMoneyIfNeeded(IBonusApp_CustomerActivityService activityService);
    }
}