using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.ZhiXiao;

namespace Nop.Services.ZhiXiao
{
    public interface ICustomerTeamService
    {
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
    }
}
