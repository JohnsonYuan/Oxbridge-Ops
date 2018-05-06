using System;
using System.Collections.Generic;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.ZhiXiao;
using static Nop.Services.ZhiXiao.ZhiXiaoService;

namespace Nop.Services.ZhiXiao
{
    public interface IZhiXiaoService
    {
        /// <summary>
        /// 更新用户电子币并记录
        /// </summary>
        /// <param name="customer"></param>
        /// <param name="deltaMoney"></param>
        /// <param name="logType"></param>
        /// <param name="logComment"></param>
        /// <param name="logCommentParams"></param>
        void UpdateMoneyForUserAndLog(Customer customer, long deltaMoney, string logType, string logComment, params object[] logCommentParams);
        
            /// <summary>
        /// Inserts an activity log type item
        /// </summary>
        /// <param name="activityLogType">Activity log type item</param>
        void InsertCustomerTeam(CustomerTeam team);

        /// <summary>
        /// Updates an customer team item
        /// </summary>
        /// <param name="customerTeam">Customer team item</param>
        void UpdateCustomerTeam(CustomerTeam customerTeam);

        /// <summary>
        /// Deletes an customer team item
        /// </summary>
        /// <param name="CustomerTeam">Customer team</param>
        void DeleteCustomerTeam(CustomerTeam customerTeam);

        /// <summary>
        /// Gets all customer team items
        /// </summary>
        /// <returns>Customer team items</returns>
        IList<CustomerTeam> GetAllCustomerTeams();

        /// <summary>
        /// Gets all customer team items
        /// </summary>
        IPagedList<CustomerTeam> GetAllCustomerTeams(string teamNumber, DateTime? createdOnFrom = null,
            DateTime? createdOnTo = null, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Gets an customer team item
        /// </summary>
        /// <param name="customerTeamId">Customer team identifier</param>
        /// <returns>Customer teame item</returns>
        CustomerTeam GetCustomerTeamById(int customerTeamId);

        /// <summary>
        /// 小组新增加用户的sortid
        /// </summary>
        int GetNewUserSortId(CustomerTeam team);

        /// <summary>
        /// 1. 给组长， 副组长分钱, 2. 如果人数满足, 重新分组
        /// </summary>
        void AddNewUserToTeam(CustomerTeam team, Customer newCustomer);
        
        /// <summary>
        /// 得到用户收货状态
        /// </summary>
        /// <param name="customer"></param>
        SendProductInfo GetSendProductInfo(Customer customer);
        /// <summary>
        /// 得到用户收货状态
        /// </summary>
        /// <param name="customer"></param>
        /// <returns></returns>
        SendProductStatus GetSendProductStatus(Customer customer);

        /// <summary>
        /// 给用户发货, 设置发货状态属性
        /// </summary>
        /// <param name="customerId"></param>
        void SetSendProductStatus(Customer customer, SendProductStatus status);

        /// <summary>
        /// 用户二级密码是否正确
        /// </summary>
        /// <param name="customer">current user</param>
        /// <param name="password2">input password2</param>
        /// <returns>Is match</returns>
        bool UserPassword2Valid(Customer customer, string password2);

        /// <summary>
        /// 用户提现
        /// </summary>
        /// <param name="customer"></param>
        /// <param name="amount"></param>
        int WithdrawMoney(Customer customer, int amount);
    }
}
