using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.ZhiXiao;
using Nop.Core.Infrastructure;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Logging;

namespace Nop.Services.ZhiXiao
{
    public partial class ZhiXiaoService : IZhiXiaoService
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

        private readonly ICustomerService _customerService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IGenericAttributeService _genericAttributeService;

        private readonly ZhiXiaoSettings _zhiXiaoSettings;

        #endregion

        #region Ctor

        public ZhiXiaoService(ICacheManager cacheManager,
            IRepository<GenericAttribute> gaRepository,
            IRepository<Customer> customerRepository,
            IRepository<CustomerTeam> customerTeamRepository,
            ICustomerService customerService,
            ICustomerActivityService customerActivityService,
            IGenericAttributeService genericAttributeService,
            ZhiXiaoSettings zhiXiaoSettings)
        {
            this._cacheManager = cacheManager;
            this._gaRepository = gaRepository;
            this._customerRepository = customerRepository;
            this._customerService = customerService;
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
        /// 更新用户钱, 并且加入log
        /// </summary>
        /// <param name="customer"></param>
        /// <param name="deltaMoney"></param>
        /// <param name="logComment"></param>
        /// <param name="logCommentParams"></param>
        public void UpdateMoneyForUserAndLog(Customer customer, long deltaMoney, string logType, string logComment, params object[] logCommentParams)
        {
            var money = customer.GetAttribute<long>(SystemCustomerAttributeNames.ZhiXiao_MoneyNum);
            var moneyHistory = customer.GetAttribute<long>(SystemCustomerAttributeNames.ZhiXiao_MoneyHistory);

            // 实际钱
            _genericAttributeService.SaveAttribute(
                customer,
                SystemCustomerAttributeNames.ZhiXiao_MoneyNum,
                money + deltaMoney);

            // 钱历史记录
            _genericAttributeService.SaveAttribute(
                customer,
                SystemCustomerAttributeNames.ZhiXiao_MoneyHistory,
                moneyHistory + deltaMoney);

            // add log
            _customerActivityService.InsertActivity(customer,
                logType,
                logComment,
                logCommentParams);
        }

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
        /// Gets all customer team items
        /// </summary>
        /// <param name="teamNumber">Team number</param>
        /// <param name="createdOnFrom">Log item creation from; null to load all activities</param>
        /// <param name="createdOnTo">Log item creation to; null to load all activities</param>
        /// <returns>Customer team items items</returns>
        public virtual IPagedList<CustomerTeam> GetAllCustomerTeams(string teamNumber = null, CustomerTeamType? teamType = null, DateTime? createdOnFrom = null,
            DateTime? createdOnTo = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _customerTeamRepository.Table;
            if (!String.IsNullOrEmpty(teamNumber))
                query = query.Where(al => al.CustomNumber.Contains(teamNumber));
            if (teamType.HasValue)
            {
                var teamTypeId = (int)teamType.Value;
                query = query.Where(al => teamTypeId == al.TypeId);
            }
            if (createdOnFrom.HasValue)
                query = query.Where(al => createdOnFrom.Value <= al.CreatedOnUtc);
            if (createdOnTo.HasValue)
                query = query.Where(al => createdOnTo.Value >= al.CreatedOnUtc);

            query = query.OrderByDescending(al => al.CreatedOnUtc);

            var customerTeams = new PagedList<CustomerTeam>(query, pageIndex, pageSize);
            return customerTeams;
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

        #region Regroup

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
        /// 3. 给小组内组长, 副组长分钱
        /// </summary>
        /// <param name="team"></param>
        /// <param name="newCustomer"></param>
        protected virtual void UpdateTeamMemberMoney(CustomerTeam team, Customer newCustomer)
        {
            var zuZhang = FindTeamZuZhang(team);

            // 增加的钱数
            var addMoney = team.TypeId == (int)CustomerTeamType.Advanced ?
                _zhiXiaoSettings.NewUserMoney_ZuZhang_Advanced
                : _zhiXiaoSettings.NewUserMoney_ZuZhang_Normal;

            // 记录组长奖金钱数
            UpdateMoneyForUserAndLog(zuZhang,
                addMoney,
                SystemZhiXiaoLogTypes.AddNewUser,
                "小组新加入会员{0}, 奖金+{1}",
                newCustomer.GetNickName(),
                addMoney);

            var fuZuZhangs = FindTeamFuZuZhang(team);

            addMoney = team.TypeId == (int)CustomerTeamType.Advanced ?
                _zhiXiaoSettings.NewUserMoney_FuZuZhang_Advanced
                : _zhiXiaoSettings.NewUserMoney_FuZuZhang_Normal;

            foreach (var user in fuZuZhangs)
            {
                // 记录组长奖金钱数
                UpdateMoneyForUserAndLog(user,
                    addMoney,
                    SystemZhiXiaoLogTypes.AddNewUser,
                    "小组新加入会员{0}, 奖金+{1}",
                    newCustomer.GetNickName(),
                    addMoney);
            }
        }

        #region Regroup

        /// <summary>
        /// 如果需要重新分组则分组
        /// </summary>
        /// <param name="oldTeam"></param>
        protected virtual void ReGroupTeamIfNeeded(CustomerTeam oldTeam)
        {
            if (oldTeam == null)
                throw new ArgumentNullException("oldTeam");

            var teamMembers = oldTeam.Customers;

            // 人数等于分组人数时才分组
            if (teamMembers.Count < _zhiXiaoSettings.TeamReGroupUserCount)
                return;

            // 1.组长升级为董事, 拿22000/80000元
            var zuZhang = FindTeamZuZhang(oldTeam);

            // 增加的钱数
            var addMoney = zuZhang.IsRegistered_Advanced() ?
                _zhiXiaoSettings.ReGroupMoney_ZuZhang_Advanced
                : _zhiXiaoSettings.ReGroupMoney_ZuZhang_Normal;

            // 记录组长奖金钱数
            UpdateMoneyForUserAndLog(zuZhang,
                addMoney,
                SystemZhiXiaoLogTypes.ReGroupTeam_AddMoney,
                "小组{0}重新分组, 奖金+{1}",
                oldTeam.CustomNumber,
                addMoney);

            if (zuZhang.IsRegistered_Advanced())
            {
                // 当是高级小组是才向上检查升级
                // 组长进入董事级别
                _genericAttributeService.SaveAttribute(zuZhang,
                    SystemCustomerAttributeNames.ZhiXiao_LevelId,
                    (int)CustomerLevel.DongShi0);

                _customerActivityService.InsertActivity(zuZhang,
                    SystemZhiXiaoLogTypes.ReGroupTeam_UpdateLevel,
                    "小组{0}重新分组, 由{1}升级为{2}",
                    oldTeam.CustomNumber,
                    CustomerLevel.ZuZhang.GetDescription(),
                    CustomerLevel.DongShi0.GetDescription());

                // 2. 组长升级为董事级别, 递归升级组长上线的钱
                ReGroup_UpdateZuZhangParentMoney(zuZhang);

                // 3. 判断该组长的上线是否满足升级资格 => 看该上线的另一个下线是否满足
                ReGroup_UpdateZuZhangParentClass(zuZhang);
            }
            else
            {
                // 当是普通小组清空child parent
                var childs = _customerService.GetCustomerChildren(zuZhang);
                foreach (var child in childs)
                {
                    _genericAttributeService.SaveAttribute<string>(child, SystemCustomerAttributeNames.ZhiXiao_ParentId, null);
                }

                // 当是高级小组是才向上检查升级
                // 组长进入董事级别
                _genericAttributeService.SaveAttribute(zuZhang,
                    SystemCustomerAttributeNames.ZhiXiao_LevelId,
                    (int)CustomerLevel.PreDongShi);

                _customerActivityService.InsertActivity(zuZhang,
                    SystemZhiXiaoLogTypes.ReGroupTeam_UpdateLevel,
                    "小组{0}重新分组, 离开小组",
                    oldTeam.CustomNumber);
            }

            // 组长进入董事级别, 删除teamId 属性
            //_genericAttributeService.SaveAttribute<string>(zuZhang,
            //    SystemCustomerAttributeNames.ZhiXiao_TeamId,
            //    null);
            zuZhang.CustomerTeam = null;
            _customerService.UpdateCustomer(zuZhang);

            // 4.原来小组的组员(SortId < 7)每人1600
            ReGroup_UpdateZuYuanMoney(oldTeam);

            var customNumberFormatter = EngineContext.Current.Resolve<ICustomNumberFormatter>();

            // 5. 开始分组
            var newTeam = new CustomerTeam
            {
                //CustomNumber = DateTime.UtcNow.ToString("yyyymmdd"),
                UserCount = _zhiXiaoSettings.TeamInitUserCount,
                CreatedOnUtc = DateTime.UtcNow
            };

            _customerTeamRepository.Insert(newTeam);

            // 更新编号
            newTeam.CustomNumber = customNumberFormatter.GenerateTeamNumber(newTeam);
            newTeam.TeamType = oldTeam.TeamType;
            _customerTeamRepository.Update(newTeam);

            // 重新分组, 小组人数都是7人
            oldTeam.UserCount = _zhiXiaoSettings.TeamInitUserCount;
            _customerTeamRepository.Update(oldTeam);

            //var oldTeamid = oldTeam.Id;
            //var newTeamId = newTeam.Id;

            // 按照下线个数(desc), 时间(asc) 排序, 来决定加入哪个小组
            var sortedUsers = oldTeam.Customers
                            .OrderByDescending(x => x.GetAttribute<int>(SystemCustomerAttributeNames.ZhiXiao_ChildCount))
                            .ThenBy(x => x.GetAttribute<int>(SystemCustomerAttributeNames.ZhiXiao_InTeamOrder))
                            .ToList();

            if (sortedUsers.Count != _zhiXiaoSettings.TeamReGroupUserCount - 1)
                throw new Exception("小组用户个数不等于" + (_zhiXiaoSettings.TeamReGroupUserCount - 1));

            for (int i = 0; i < sortedUsers.Count; i++)
            {
                // 一共14个人, 按照拍好的顺序, 更新team
                // 偶数分到原来的team, 奇数分到新生成的team
                var currentUserTeam = ((i % 2) == 0) ? oldTeam : newTeam;

                // 2个组长(前两个), 4个副组长(第3-6个), 其余均为组员
                // old team: 0(组长), 2(副组长), 4(副组长), 6, 8, 10, 12
                // new team: 1(组长), 3(副组长), 5(副组长), 7, 9, 11, 13
                var currentUserLevel = CustomerLevel.ZuYuan;
                if (i <= _zhiXiaoSettings.Team_ZuZhangCount)                    // i <= 1   (0, 1)
                {
                    currentUserLevel = CustomerLevel.ZuZhang;
                }
                else if (i <= _zhiXiaoSettings.Team_FuZuZhangCount * 2 + 1)      // i <= 5 (2, 3, 4, 5)
                {
                    currentUserLevel = CustomerLevel.FuZuZhang;
                }
                else
                {
                    currentUserLevel = CustomerLevel.ZuYuan;
                }

                var currentUser = sortedUsers[i];

                //var currentUserOldTeam = currentUser.GetAttribute<int>(SystemCustomerAttributeNames.ZhiXiao_TeamId);

                var currentUserOldLevel = currentUser.GetAttribute<int>(SystemCustomerAttributeNames.ZhiXiao_LevelId);
                _customerActivityService.InsertActivity(currentUser,
                        SystemZhiXiaoLogTypes.ReGroupTeam_ReSort,
                        "{0} 小组重新分组, 原先级别为{1}, 当前级别为{2}, 分至小组{3}",
                        oldTeam.CustomNumber,
                        ((CustomerLevel)currentUserOldLevel).GetDescription(),
                        ((CustomerLevel)currentUserLevel).GetDescription(),
                        newTeam.CustomNumber);

                int sortId = i / 2;
                // 更新teamid, class, inTeamTime
                currentUser.CustomerTeam = currentUserTeam;
                //_genericAttributeService.SaveAttribute(currentUser, SystemCustomerAttributeNames.ZhiXiao_TeamId, currentUserTeamId);
                _genericAttributeService.SaveAttribute(currentUser, SystemCustomerAttributeNames.ZhiXiao_LevelId, (int)currentUserLevel);
                _genericAttributeService.SaveAttribute(currentUser, SystemCustomerAttributeNames.ZhiXiao_InTeamOrder, (int)sortId + 1); // 排序从1开始
                _genericAttributeService.SaveAttribute(currentUser, SystemCustomerAttributeNames.ZhiXiao_InTeamTime, DateTime.UtcNow);
            }
        }

        /// <summary>
        /// 组长升级为董事级别, 升级组长的上线(董事)的钱
        /// </summary>
        /// <param name="zuZhang"></param>
        private void ReGroup_UpdateZuZhangParentMoney(Customer zuZhang)
        {
            if (zuZhang == null)
                throw new ArgumentNullException("zuZhang");
            var parentId = zuZhang.GetAttribute<int>(SystemCustomerAttributeNames.ZhiXiao_ParentId);

            var parentUser = _customerService.GetCustomerById(parentId);

            if (parentUser == null)
                return;

            // TODO
            var parentLevel = (CustomerLevel)parentUser.GetAttribute<int>(SystemCustomerAttributeNames.ZhiXiao_LevelId);

            // TODO: 1星董事才能分成 
            if (parentLevel < CustomerLevel.DongShi1)
                return;

            int addMoney = 0;
            var baseMoney = parentUser.IsRegistered_Advanced() ?
                _zhiXiaoSettings.ReGroupMoney_DongShiBase_Advanced
                : _zhiXiaoSettings.ReGroupMoney_DongShiBase_Normal;

            switch (parentLevel)
            {
                case CustomerLevel.DongShi1:
                    addMoney = (int)(baseMoney * _zhiXiaoSettings.ReGroupMoney_Rate_DongShi1);
                    break;
                case CustomerLevel.DongShi2:
                    addMoney = (int)(baseMoney * _zhiXiaoSettings.ReGroupMoney_Rate_DongShi2);
                    break;
                //case CustomerLevel.DongShi3:
                //    addMoney = (int)(baseMoney * _zhiXiaoSettings.ReGroupMoney_Rate_DongShi3);
                //    break;
                //case CustomerLevel.DongShi4:
                //    addMoney = (int)(baseMoney * _zhiXiaoSettings.ReGroupMoney_Rate_DongShi4);
                //    break;
                //case CustomerLevel.DongShi5:
                //    addMoney = (int)(baseMoney * _zhiXiaoSettings.ReGroupMoney_Rate_DongShi5);
                //    break;
            }

            UpdateMoneyForUserAndLog(parentUser,
                addMoney,
                "{0} 下小组重新分组, 奖金+{0}",
                SystemZhiXiaoLogTypes.ReGroupTeam_AddMoney,
                zuZhang.GetNickName(),
                addMoney);

            ReGroup_UpdateZuZhangParentMoney(parentUser);
        }

        /// <summary>
        /// 组长升级为董事级别, 判断该组长的上线是否满足升级资格
        /// (看该上线的另一个下线是否满足)
        /// </summary>
        /// <param name="zuZhang"></param>
        private void ReGroup_UpdateZuZhangParentClass(Customer zuZhang)
        {
            if (zuZhang == null)
                throw new ArgumentNullException("zuZhang");
            var parentId = zuZhang.GetAttribute<int>(SystemCustomerAttributeNames.ZhiXiao_ParentId);

            var parentUser = _customerService.GetCustomerById(parentId);

            if (parentUser == null)
                return;

            // 等于下线个数才能升级
            int subCount = parentUser.GetAttribute<int>(SystemCustomerAttributeNames.ZhiXiao_ChildCount);
            if (subCount < _zhiXiaoSettings.MaxChildCount)
            {
                return;
            }

            var childs = _customerRepository.Table
                .Where(x => x.GetAttribute<int>(SystemCustomerAttributeNames.ZhiXiao_ParentId, 0) == parentUser.Id);

            // 找到另一个下线(最多两个下线)
            var otherChild = childs.Where(x => x.Id != zuZhang.Id).FirstOrDefault();

            if (otherChild == null)
                return;

            var firstChildLevel = (CustomerLevel)zuZhang.GetAttribute<int>(SystemCustomerAttributeNames.ZhiXiao_LevelId);
            var otherChildLevel = (CustomerLevel)otherChild.GetAttribute<int>(SystemCustomerAttributeNames.ZhiXiao_LevelId);

            var parentLevel = (CustomerLevel)parentUser.GetAttribute<int>(SystemCustomerAttributeNames.ZhiXiao_LevelId);

            // 如果下线级别都达到 >= 上线级别-1, 则该上线升1级
            if (firstChildLevel >= parentLevel - 1 &&
                otherChildLevel >= parentLevel - 1)
            {
                if (parentLevel == CustomerLevel.ChuPan)
                {
                    // 已经出盘 不需要计算
                    return;
                }
                else if (parentLevel == CustomerLevel.DongShi2)
                {
                    //该上线已经达到5星董事 => 该出盘了！！
                    _genericAttributeService.SaveAttribute(parentUser,
                        SystemCustomerAttributeNames.ZhiXiao_LevelId,
                        (int)(CustomerLevel.ChuPan));

                    var addMoney = parentUser.IsRegistered_Advanced() ?
                        _zhiXiaoSettings.ReGroupMoney_DongShi5_ChuPan_Advanced
                        : _zhiXiaoSettings.ReGroupMoney_DongShi5_ChuPan_Normal;

                    // 五星董事升级！奖金30万
                    UpdateMoneyForUserAndLog(parentUser,
                        addMoney,
                        SystemZhiXiaoLogTypes.ReGroupTeam_UpdateLevel,
                        "{0} 下小组重新分组, 出盘, 奖金+{0}",
                        zuZhang.GetNickName(),
                        addMoney);

                    // update level log
                    //_customerActivityService.InsertActivity(zuZhang,
                    //    SystemZhiXiaoLogTypes.ReGroupTeam_UpdateLevel,
                    //    "{0} 下小组重新分组, 五星董事升级, 出盘",
                    //    zuZhang.GetNickName());

                    //两个下线的parent清空
                    //_genericAttributeService.SaveAttribute<string>(zuZhang,
                    //    SystemCustomerAttributeNames.ZhiXiao_ParentId, 
                    //    null);
                    //_genericAttributeService.SaveAttribute<string>(otherChild,
                    //    SystemCustomerAttributeNames.ZhiXiao_ParentId, 
                    //    null);
                }
                else
                {
                    // 组长上线升1级
                    _genericAttributeService.SaveAttribute(parentUser,
                        SystemCustomerAttributeNames.ZhiXiao_LevelId,
                        (int)(parentLevel + 1));

                    _customerActivityService.InsertActivity(zuZhang,
                        SystemZhiXiaoLogTypes.ReGroupTeam_UpdateLevel,
                        "{0} 下小组重新分组, 由{1}升级为{2}",
                        zuZhang.GetNickName(),
                        parentLevel.GetDescription(),
                        (parentLevel + 1).GetDescription());

                    // 递归， 依次看上线是否满足升级条件
                    ReGroup_UpdateZuZhangParentClass(parentUser);
                }
            }
        }

        /// <summary>
        /// 原来小组的组员每人1600(只给前4个组员)
        /// </summary>
        /// <param name="team"></param>
        private void ReGroup_UpdateZuYuanMoney(CustomerTeam team)
        {
            if (team == null)
                throw new ArgumentNullException("team");

            // 按照加入该小组时间来排序(使用orderid, 有可能时间在分组时值一样), 只有前4个组员才能分钱
            var team_zuYuans = team.Customers
                             .Where(x => x.GetAttribute<CustomerLevel>(SystemCustomerAttributeNames.ZhiXiao_LevelId) == CustomerLevel.ZuYuan)
                             .OrderBy(x => x.GetAttribute<int>(SystemCustomerAttributeNames.ZhiXiao_InTeamOrder))
                             .Take(_zhiXiaoSettings.ReGroupMoney_ZuYuan_Count)
                             .ToList();

            // 增加的钱数
            var addMoney = team.TypeId == (int)CustomerTeamType.Advanced ?
                _zhiXiaoSettings.ReGroupMoney_ZuYuan_Advanced
                : _zhiXiaoSettings.ReGroupMoney_ZuYuan_Normal;

            foreach (var member in team_zuYuans)
            {
                UpdateMoneyForUserAndLog(member,
                    addMoney,
                    SystemZhiXiaoLogTypes.ReGroupTeam_AddMoney,
                    "小组{0}重新分组, 奖金+{1}",
                    team.CustomNumber,
                    addMoney);
            }
        }

        #endregion

        /// <summary>
        /// 新增用户到小组
        /// 1. 给组长， 副组长分钱
        /// 2. 如果人数满足, 重新分组
        /// </summary>
        public virtual void AddNewUserToTeam(CustomerTeam team, Customer newCustomer)
        {
            // 3. 给组长， 副组长分钱
            UpdateTeamMemberMoney(team, newCustomer);

            // 4. 如果人数满足, 重新分组
            ReGroupTeamIfNeeded(team);
        }

        #endregion

        #region Send product

        /// <summary>
        /// 得到用户收货状态和对应log
        /// </summary>
        /// <param name="customer"></param>
        public virtual SendProductInfo GetSendProductInfo(Customer customer)
        {
            if (customer == null)
                throw new ArgumentException("customer");

            SendProductInfo info = new SendProductInfo();
            info.Status = (SendProductStatus)customer.GetAttribute<int>(SystemCustomerAttributeNames.ZhiXiao_SendProductStatus);
            var sendLogId = customer.GetAttribute<int>(SystemCustomerAttributeNames.ZhiXiao_SendProductLogId);
            if (sendLogId > 0)
            {
                info.SendLog = _customerActivityService.GetActivityById(sendLogId);
            }

            var receiedLogId = customer.GetAttribute<int>(SystemCustomerAttributeNames.ZhiXiao_ReceiveProductLogId);
            if (receiedLogId > 0)
            {
                info.ReceiveLog = _customerActivityService.GetActivityById(receiedLogId);
            }
            return info;
        }

        /// <summary>
        /// 得到用户收货状态
        /// </summary>
        /// <param name="customer"></param>
        /// <returns></returns>
        public virtual SendProductStatus GetSendProductStatus(Customer customer)
        {
            if (customer == null)
                throw new ArgumentException("customer");
            return (SendProductStatus)customer.GetAttribute<int>(SystemCustomerAttributeNames.ZhiXiao_SendProductStatus);
        }

        /// <summary>
        /// 给用户发货, 设置发货状态属性
        /// </summary>
        /// <param name="customerId"></param>
        public virtual void SetSendProductStatus(Customer customer, SendProductStatus status)
        {
            if (customer == null)
                throw new ArgumentException("customer");
            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZhiXiao_SendProductStatus, (int)status);
        }

        /// <summary>
        /// 得到用户收货状态
        /// </summary>
        /// <param name="customer"></param>
        /// <returns></returns>
        public virtual ActivityLog GetReceiveProductLog(Customer customer)
        {
            if (customer == null)
                throw new ArgumentException("customer");

            var logId = customer.GetAttribute<int>(SystemCustomerAttributeNames.ZhiXiao_ReceiveProductLogId);
            return _customerActivityService.GetActivityById(logId);
        }
        /// <summary>
        /// 用户收货, 记录对应logid
        /// </summary>
        public virtual void SaveReceiveProductLog(Customer customer, ActivityLog log)
        {
            if (customer == null)
                throw new ArgumentException("customer");

            if (log == null)
                throw new ArgumentException("log");

            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZhiXiao_ReceiveProductLogId, log.Id);
        }

        #endregion

        #region Use infos

        /// <summary>
        /// 用户二级密码是否正确
        /// </summary>
        /// <param name="customer">current user</param>
        /// <param name="password2">input password2</param>
        /// <returns>Is match</returns>
        public virtual bool UserPassword2Valid(Customer customer, string password2)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            var userPassword2 = customer.GetAttribute<string>(SystemCustomerAttributeNames.ZhiXiao_Password2);
            return string.Equals(userPassword2, password2, StringComparison.Ordinal);
        }

        /// <summary>
        /// 用户提现
        /// </summary>
        /// <param name="customer"></param>
        /// <param name="amount"></param>
        public virtual int WithdrawMoney(Customer customer, int amount)
        {
            // make sure rate range from (0, 1]
            var rate = _zhiXiaoSettings.Withdraw_Rate;
            if (rate <= 0 || rate > 1)
                rate = 1;

            // 实际提取金额会扣除手续费
            var actualAmount = Convert.ToInt32(_zhiXiaoSettings.Withdraw_Rate * amount);

            var totalMoney = customer.GetMoneyNum();

            if (actualAmount > totalMoney)
                throw new ArgumentException("提现金额超出当前电子币余额");

            // 实际扣除输入的金额
            _genericAttributeService.SaveAttribute(
                customer,
                SystemCustomerAttributeNames.ZhiXiao_MoneyNum,
                totalMoney - amount);

            // 提现记录显示扣除手续费的金额
            _customerActivityService.InsertWithdraw(customer,
                actualAmount,
                "提现申请{0}, 实际金额{1}",
                amount,
                actualAmount);

            _customerActivityService.InsertActivity(customer,
                SystemZhiXiaoLogTypes.Withdraw,
                "提现申请{0}, 实际金额{1}",
                amount,
                actualAmount);

            return actualAmount;
        }

        #endregion

        #endregion

        /// <summary>
        /// 发货信息
        /// </summary>
        public class SendProductInfo
        {
            /// <summary>
            /// 状态
            /// </summary>
            public SendProductStatus Status { get; set; }
            /// <summary>
            /// 发货对应的log
            /// </summary>
            public ActivityLog SendLog { get; set; }
            /// <summary>
            /// 收货对应的log
            /// </summary>
            public ActivityLog ReceiveLog { get; set; }

            /// <summary>
            /// Is prodcut sended?
            /// </summary>
            public bool HasSent
            {
                get
                {
                    return Status == SendProductStatus.Sended;
                }
            }
        }
    }
}
