using System;
using System.Linq;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Domain.BonusApp.Logging;
using Nop.Core.Infrastructure;
using Nop.Extensions;
using Nop.Services.BonusApp.Customers;
using Nop.Services.BonusApp.Logging;
using Nop.Services.Helpers;
using Nop.Services.ZhiXiao.BonusApp;
using Nop.Web.Framework;
using Web.ZhiXiao.Areas.BonusApp.Factories;
using Web.ZhiXiao.Areas.BonusApp.Models;

namespace Web.ZhiXiao.Areas.BonusApp.Controllers
{
    public class HomeController : BaseUserController
    {
        #region Fields

        private const int PAGE_SIZE = 15;

        private IBonusAppService _appService;
        private IBonusApp_CustomerService _customerService;
        private IBonusApp_CustomerActivityService _customerActivityService;

        private IDateTimeHelper _dateTimeHelper;
        private IWebHelper _webHelper;
        private IWorkContext _workContext;

        #endregion

        #region Ctor

        public HomeController(
            IBonusAppService appService,
            IBonusApp_CustomerService customerService,
            IBonusApp_CustomerActivityService customerActivityService,
            IDateTimeHelper dateTimeHelper,
            IWebHelper webHelper,
            IWorkContext workContext)
        {
            this._appService = appService;
            this._customerService = customerService;
            this._customerActivityService = customerActivityService;
            this._dateTimeHelper = dateTimeHelper;
            this._webHelper = webHelper;
            this._workContext = workContext;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// 首页
        /// </summary>
        /// <param name="status"></param>
        /// <param name="pageNumber"></param>
        /// <returns></returns>
        private PoolItemListModel PreparePoolIListModel(BonusApp_MoneyReturnStatus status, int pageNumber)
        {
            var pageIndex = pageNumber - 1;
            var moneyLogs = _customerActivityService.GetMoneyLogByType(status, pageIndex, PAGE_SIZE);

            // 第一页第一条为1, 第二页第一条为1*PAGE_SIZE-1
            var firstItemNumber = pageIndex * PAGE_SIZE + 1;
            var model = new PoolItemListModel
            {
                Data = moneyLogs.Select((logItem, index) =>
                    new PoolItemModel
                    {
                        OrderNumber = index + firstItemNumber,
                        CustomerAvatar = logItem.Customer.AvatarFileName,
                        CustomerName = logItem.Customer.Nickname,
                        Money = (double)logItem.ReturnMoney,
                        DisplayDateTime = _dateTimeHelper.ConvertToUserTime(logItem.CreatedOnUtc, DateTimeKind.Utc).ToString("yyyy-MM-dd HH:mm")
                    }),
                TotalCount = moneyLogs.TotalCount,
                TotalPages = moneyLogs.TotalPages
            };

            return model;
        }
        private HomePageModel PrepareHomePageModel(BonusApp_MoneyReturnStatus status, int pageNumber)
        {
            HomePageModel model = new HomePageModel();
            model.BonusAppStatus = _appService.GetAppStatus();
            model.Status = status;
            model.PoolItems = PreparePoolIListModel(status, pageNumber);
            return model;
        }

        /// <summary>
        /// 评论
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        private CommentListModel PrepareCommentListModel(int pageNumber)
        {
            var pageIndex = pageNumber - 1;
            var comments = _customerService.GetAllCustomerComments(pageIndex: pageIndex, pageSize: PAGE_SIZE);

            var model = new CommentListModel
            {
                Data = comments.Select(x =>
                {
                    var m = x.ToModel<CommentModel>();
                    m.CreatedOn = x.CreatedOnUtc.RelativeFormat(true, "yyyy-MM-dd");
                    return m;
                }),
                TotalCount = comments.TotalCount,
                TotalPages = comments.TotalPages
            };

            return model;
        }

        #endregion

        #region Methods

        #region HomePage

        /// <summary>
        /// Home Page
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public ActionResult Index(BonusApp_MoneyReturnStatus status = BonusApp_MoneyReturnStatus.Waiting)
        {
            var model = PrepareHomePageModel(status, 1);
            return View(model);
        }
        /// <summary>
        /// Home Page - ajax load data
        /// </summary>
        /// <param name="status"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult LoadUser(BonusApp_MoneyReturnStatus status, int page = 1)
        {
            if (page < 1)
                page = 1;

            var model = PreparePoolIListModel(status, page);
            return PartialView("_PoolItems", model.Data);
        }

        #endregion

        #region Comment

        public ActionResult Comment()
        {
            var firstPageModel = PrepareCommentListModel(1);
            return View(firstPageModel);
        }

        public ActionResult LoadComment(int page = 1)
        {
            var model = PrepareCommentListModel(page);
            return PartialView("_CommentItems", model.Data);
        }

        #endregion

        #region UserSettings

        

        #endregion

        #endregion
    }
}