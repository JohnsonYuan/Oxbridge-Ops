using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.ZhiXiao;
using Nop.Services.Common;
using Nop.Services.Logging;

namespace Nop.Services.ZhiXiao
{
    public partial class CustomerTeamService : ICustomerTeamService
    {
        #region Constants

        /// <summary>
        /// Key for caching
        /// </summary>
        private const string CUSTOMERTEAMS_ALL_KEY = "Nop.customerteam.all";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : topic ID
        /// </remarks>
        private const string CUSTOMERTEAMS_BY_ID_KEY = "Nop.customerteam.id-{0}";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string CUSTOMERTEAMS_PATTERN_KEY = "Nop.customerteam.";

        #endregion

        #region Fields
        
        private readonly ICacheManager _cacheManager;
        private readonly IRepository<GenericAttribute> _gaRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<CustomerTeam> _customerTeamRepository;
        
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IGenericAttributeService _genericAttributeService;

        private readonly ZhiXiaoSettings _zhiXiaoSettings;

        #endregion

        #region Ctor

        public CustomerTeamService(ICacheManager cacheManager,
            IRepository<GenericAttribute> gaRepository,
            IRepository<Customer> customerRepository,
            IRepository<CustomerTeam> customerTeamRepository,
            ICustomerActivityService customerActivityService,
            IGenericAttributeService genericAttributeService,
            ZhiXiaoSettings zhiXiaoSettings)
        {
            this._cacheManager = cacheManager;
            this._gaRepository = gaRepository;
            this._customerRepository = customerRepository;
            this._customerTeamRepository = customerTeamRepository;
            this._customerActivityService = customerActivityService;
            this._genericAttributeService = genericAttributeService;

            this._zhiXiaoSettings = zhiXiaoSettings;
        }

        #endregion

        #region Utilities

        #region 直销相关

        protected Customer FindTeamZuZhang(CustomerTeam team)
        {
            var customers = team.Customers
                          .Join(_gaRepository.Table, x => x.Id, y => y.EntityId, (x, y) => new { Customer = x, Attribute = y })
                          .Where((z => z.Attribute.KeyGroup == "Customer" &&
                            z.Attribute.Key == SystemCustomerAttributeNames.ZhiXiao_LevelId &&
                            CommonHelper.To<int>(z.Attribute.Value) == (int)CustomerLevel.ZuZhang))
                           .Select(z => z.Customer).ToList();

            if (customers.Count != _zhiXiaoSettings.Team_ZuZhangCount)
                throw new Exception(string.Format("小组{0}出现错误, 组长个数为{1}", team.CustomNumber, customers.Count));

            return customers.First();
        }

        protected IList<Customer> FindTeamFuZuZhang(CustomerTeam team)
        {
            var customers = team.Customers
                          .Join(_gaRepository.Table, x => x.Id, y => y.EntityId, (x, y) => new { Customer = x, Attribute = y })
                          .Where((z => z.Attribute.KeyGroup == "Customer" &&
                            z.Attribute.Key == SystemCustomerAttributeNames.ZhiXiao_LevelId &&
                            CommonHelper.To<int>(z.Attribute.Value) == (int)CustomerLevel.FuZuZhang))
                           .Select(z => z.Customer).ToList();

            if (customers.Count != _zhiXiaoSettings.Team_FuZuZhangCount)
                throw new Exception(string.Format("小组{0}出现错误, 副组长个数为{1}", team.CustomNumber, customers.Count));

            return customers;
        }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Inserts an activity log type item
        /// </summary>
        /// <param name="activityLogType">Activity log type item</param>
        public virtual void InsertCustomerTeam(CustomerTeam team)
        {
            if (team == null)
                throw new ArgumentNullException("team");

            _customerTeamRepository.Insert(team);
            _cacheManager.RemoveByPattern(CUSTOMERTEAMS_PATTERN_KEY);
        }

        /// <summary>
        /// Updates an customer team item
        /// </summary>
        /// <param name="customerTeam">Customer team item</param>
        public virtual void UpdateCustomerTeam(CustomerTeam customerTeam)
        {
            if (customerTeam == null)
                throw new ArgumentNullException("activityLogType");

            _customerTeamRepository.Update(customerTeam);
            _cacheManager.RemoveByPattern(CUSTOMERTEAMS_PATTERN_KEY);
        }
                
        /// <summary>
        /// Deletes an customer team item
        /// </summary>
        /// <param name="CustomerTeam">Customer team</param>
        public virtual void DeleteCustomerTeam(CustomerTeam customerTeam)
        {
            if (customerTeam == null)
                throw new ArgumentNullException("customerTeam");

            _customerTeamRepository.Delete(customerTeam);
            _cacheManager.RemoveByPattern(CUSTOMERTEAMS_PATTERN_KEY);
        }

        /// <summary>
        /// Gets all customer team items
        /// </summary>
        /// <returns>Customer team items</returns>
        public virtual IList<CustomerTeam> GetAllCustomerTeams()
        {
            string key = string.Format(CUSTOMERTEAMS_ALL_KEY);
            return _cacheManager.Get(key, () =>
            {
                var query = from ct in _customerTeamRepository.Table
                    orderby ct.CreatedOnUtc descending
                    select ct;
                var customerTeams = query.ToList();
                return customerTeams;
            });
        }

        /// <summary>
        /// Gets an customer team item
        /// </summary>
        /// <param name="customerTeamId">Customer team identifier</param>
        /// <returns>Customer teame item</returns>
        public virtual CustomerTeam GetCustomerTeamById(int customerTeamId)
        {
            if (customerTeamId == 0)
                return null;

            string key = string.Format(CUSTOMERTEAMS_BY_ID_KEY, customerTeamId);
            return _cacheManager.Get(key, () =>
            {
                return _customerTeamRepository.GetById(customerTeamId);
            });
        }

        /// <summary>
        /// 小组新增加用户的sortid
        /// </summary>
        public virtual int GetNewUserSortId(CustomerTeam team)
        {
            if (team == null)
                throw new ArgumentNullException("team");

            var sortIds = team.Customers
                          .Join(_gaRepository.Table, x => x.Id, y => y.EntityId, (x, y) => new { Customer = x, Attribute = y })
                          .Where((z => z.Attribute.KeyGroup == "Customer" &&
                            z.Attribute.Key == SystemCustomerAttributeNames.ZhiXiao_InTeamOrder))
                           .Select(z => CommonHelper.To<int>(z.Attribute.Value));

            var maxSortId = sortIds.Max(x => x);
            return maxSortId + 1;
        }

        /// <summary>
        /// 3. 给组长， 副组长分钱
        /// </summary>
        /// <param name="team"></param>
        /// <param name="newCustomer"></param>
        public virtual void UpdateParentMoney(CustomerTeam team, Customer newCustomer)
        {
            /*
            DataTable dt = DbHelper.GetDataTable("SELECT TOP 1 * FROM Users WHERE Class = " + GroupClassHelper.ToIdString(GroupClassName.ZuZhang) + " AND TeamId = " + teamId.ToString() + "");
            string id = dt.Rows[0]["Id"].ToString();
            string msg = "新加入会员 " + userName + " . 奖金+3000.";
            SystemHelper.InsertMoneyMsg(Convert.ToInt32(id), msg, 3000);
            DbHelper.ExecSql("UPDATE Users SET MoneyNum = MoneyNum + 3000, MonenHistory = MonenHistory + 3000 WHERE Class = " + GroupClassHelper.ToIdString(GroupClassName.ZuZhang) + " AND TeamId = " + teamId.ToString() + "");

            dt = DbHelper.GetDataTable("SELECT TOP 1 * FROM Users WHERE Class = " + GroupClassHelper.ToIdString(GroupClassName.FuZuZhang) + " AND TeamId = " + teamId.ToString() + "");
            foreach (DataRow dataRow in dt.Rows)
            {
                msg = "新加入会员 " + userName + " . 奖金+1000.";
                SystemHelper.InsertMoneyMsg(Convert.ToInt32(dataRow["Id"].ToString()), msg, 1000);
            }
            DbHelper.ExecSql("UPDATE Users SET MoneyNum = MoneyNum + 1000, MonenHistory = MonenHistory + 1000 WHERE Class = " + GroupClassHelper.ToIdString(GroupClassName.FuZuZhang) + " AND TeamId = " + teamId.ToString() + "");
                */    
        }

        /// <summary>
        /// 如果需要重新分组则分组
        /// </summary>
        /// <param name="oldTeam"></param>
        public virtual void ReGroupTeamIfNeeded(CustomerTeam oldTeam)
        {
            if (oldTeam == null)
                throw new ArgumentNullException("oldTeam");

            var teamMembers = oldTeam.Customers;

            // 人数等于分组人数时才分组
            if (teamMembers.Count < _zhiXiaoSettings.TeamReGroupCount)
                return;

            // 1. add new team
            var newTeam = new CustomerTeam
            {
                CustomNumber = DateTime.UtcNow.ToString("yyyymmdd"),
                UserCount = 0,
                CreatedOnUtc = DateTime.UtcNow
            };

            _customerTeamRepository.Insert(newTeam);

            var newTeamId = newTeam.Id;

            // 2.组长升级为董事, 拿50000元
            var zuZhang = FindTeamZuZhang(oldTeam);
            
            var zuZhangMoney = zuZhang.GetAttribute<long>(SystemCustomerAttributeNames.ZhiXiao_MoneyNum);
            var zuZhangMoneyHistory = zuZhang.GetAttribute<long>(SystemCustomerAttributeNames.ZhiXiao_MoneyHistory);

            // 增加的钱数
            var addMoney = zuZhang.IsRegistered_Advanced() ? 
                _zhiXiaoSettings.ReGroupMoney_ZuZhang_Advanced
                : _zhiXiaoSettings.ReGroupMoney_ZuZhang_Normal;

            // 钱数
            _genericAttributeService.SaveAttribute(
                zuZhang,
                SystemCustomerAttributeNames.ZhiXiao_MoneyNum,
                zuZhangMoney + addMoney);
      
            // 钱历史记录
            _genericAttributeService.SaveAttribute(
                zuZhang,
                SystemCustomerAttributeNames.ZhiXiao_MoneyHistory,
                zuZhangMoneyHistory + addMoney);

            // add log
            _customerActivityService.InsertActivity(zuZhang, 
                SystemZhiXiaoLogTypes.ReGroupTeam, 
                "小组{0}重新分组, 奖金+{0}",
                oldTeam.CustomNumber,
                addMoney);

            // 组长出局, 删除teamId 属性
            _genericAttributeService.SaveAttribute(zuZhang,
                SystemCustomerAttributeNames.ZhiXiao_TeamId, 
                "");
            
            // 组长进入董事级别
            _genericAttributeService.SaveAttribute(zuZhang,
                SystemCustomerAttributeNames.ZhiXiao_LevelId, 
                (int)CustomerLevel.DongShi0);

            _customerActivityService.InsertActivity(zuZhang, 
                SystemZhiXiaoLogTypes.ReGroupTeam_Update, 
                "小组{0}重新分组, 由{0}升级为{1}",
                CustomerLevel.ZuZhang.GetDescription(),
                CustomerLevel.DongShi0.GetDescription());

            // 3. 升级董事的钱

            // 4. 判断该组长的上线是否满足升级资格 => 看该上线的另一个下线是否满足
        }

        #endregion
    }
}
