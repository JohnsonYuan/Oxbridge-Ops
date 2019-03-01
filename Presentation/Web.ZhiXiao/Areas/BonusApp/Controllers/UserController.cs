using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Domain.BonusApp;
using Nop.Core.Infrastructure;
using Nop.Extensions;
using Nop.Services.BonusApp.Customers;
using Nop.Services.BonusApp.Logging;
using Nop.Services.Helpers;
using Nop.Services.Media;
using Nop.Services.ZhiXiao.BonusApp;
using Web.ZhiXiao.Areas.BonusApp.Factories;
using Web.ZhiXiao.Areas.BonusApp.Models;
using Web.ZhiXiao.Areas.BonusApp.Models.Log;
using Web.ZhiXiao.Areas.BonusApp.Models.Users;

namespace Web.ZhiXiao.Areas.BonusApp.Controllers
{
    public class UserController : BaseUserController
    {
        #region Fields

        private const int PAGE_SIZE = 15;

        private IPictureService _pictureService;
        private IBonusAppService _appService;
        private IBonusApp_CustomerService _customerService;
        private IBonusApp_CustomerActivityService _customerActivityService;

        private IDateTimeHelper _dateTimeHelper;
        private IWebHelper _webHelper;
        private IWorkContext _workContext;
        private CustomerModelFactory _factory;

        #endregion

        #region Ctor

        public UserController(
            IPictureService pictureService,
            IBonusAppService appService,
            IBonusApp_CustomerService customerService,
            IBonusApp_CustomerActivityService customerActivityService,
            IDateTimeHelper dateTimeHelper,
            IWebHelper webHelper,
            IWorkContext workContext,
            CustomerModelFactory factory)
        {
            this._pictureService = pictureService;
            this._appService = appService;
            this._customerService = customerService;
            this._customerActivityService = customerActivityService;
            this._dateTimeHelper = dateTimeHelper;
            this._webHelper = webHelper;

            this._workContext = workContext;
            this._factory = factory;
        }

        #endregion

        #region Utilities

        private CustomerModel PrepareCustomerModel()
        {
            var customer = _workContext.CurrentBonusAppCustomer;
            return _factory.PrepareCustomerModel(customer);
        }

        #endregion

        public ActionResult Index()
        {
            var model = PrepareCustomerModel();
            return View(model);
        }

        #region user info

        public ActionResult Edit()
        {
            var model = PrepareCustomerModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult UpdatePwd(
            string oldPwd,
            string newPwd,
            string newPwd2)
        {
            if (string.IsNullOrEmpty(oldPwd))
                return ErrorJson("请输入原密码");
            if (string.IsNullOrEmpty(newPwd))
                return ErrorJson("请输入新密码");
            if (string.IsNullOrEmpty(newPwd2))
                return ErrorJson("请再次输入新密码");

            if (newPwd != newPwd2)
                return ErrorJson("两次密码不一致");

            var loginResult =
               _customerService.ValidateCustomer(_workContext.CurrentBonusAppCustomer.Username, oldPwd);

            if (loginResult != Nop.Core.Domain.Customers.CustomerLoginResults.Successful)
                return ErrorJson("原密码错误");

            _customerService.ChangeCustomerPassword(_workContext.CurrentBonusAppCustomer,newPwd);
            _customerActivityService.InsertActivity(BonusAppConstants.LogType_User_ChangePwd, "旧密码: {0}, 新密码: {1}", oldPwd, newPwd);

            return SuccessJson("修改密码成功");
        }

        #endregion

        #region Upload picture

        [HttpPost]
        //do not validate request token (XSRF)
        public virtual ActionResult ChangeAvatar()
        {
            var pictureService = EngineContext.Current.Resolve<IPictureService>();
            Stream stream = null;
            var fileName = "";
            var contentType = "";
            if (String.IsNullOrEmpty(Request["file"]))
            {
                // IE
                HttpPostedFileBase httpPostedFile = Request.Files[0];
                if (httpPostedFile == null)
                    throw new ArgumentException("No file uploaded");
                stream = httpPostedFile.InputStream;
                fileName = Path.GetFileName(httpPostedFile.FileName);
                contentType = httpPostedFile.ContentType;
            }
            else
            {
                //Webkit, Mozilla
                stream = Request.InputStream;
                fileName = Request["file"];
            }

            var fileBinary = new byte[stream.Length];
            stream.Read(fileBinary, 0, fileBinary.Length);
            
            var fileExtension = Path.GetExtension(fileName);
            if (!String.IsNullOrEmpty(fileExtension))
                fileExtension = fileExtension.ToLowerInvariant();

            if (String.IsNullOrEmpty(contentType))
            {
                switch (fileExtension)
                {
                    case ".bmp":
                        contentType = MimeTypes.ImageBmp;
                        break;
                    case ".gif":
                        contentType = MimeTypes.ImageGif;
                        break;
                    case ".jpeg":
                    case ".jpg":
                    case ".jpe":
                    case ".jfif":
                    case ".pjpeg":
                    case ".pjp":
                        contentType = MimeTypes.ImageJpeg;
                        break;
                    case ".png":
                        contentType = MimeTypes.ImagePng;
                        break;
                    case ".tiff":
                    case ".tif":
                        contentType = MimeTypes.ImageTiff;
                        break;
                    default:
                        break;
                }
            }

            // delete old picture
            pictureService.DeleteUploadPicture(_workContext.CurrentBonusAppCustomer.AvatarFileName);

            // avatar file name
            var avatarFileName = pictureService.SavePicture(fileBinary, contentType, 100);
            _workContext.CurrentBonusAppCustomer.AvatarFileName = avatarFileName;
            _customerService.UpdateCustomer(_workContext.CurrentBonusAppCustomer);
            _customerActivityService.ClearMoneyLogCache();  // 更换头像, log缓存需要更新
            //when returning JSON the mime-type must be set to text/plain
            //otherwise some browsers will pop-up a "Save As" dialog.
            return Json(new { success = true,
                imageUrl = pictureService.GetPictureUrl(avatarFileName) },
                MimeTypes.ApplicationJson);
        }

        #endregion

        #region tixian and log

        public ActionResult TiXian()
        {
            var model = PrepareCustomerModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult TiXian(TiXianModel model)
        {
            #region validate

            if (model.PayType != (int)TiXianPayType.Alipay
                && model.PayType != (int)TiXianPayType.Bank)    
                return ErrorJson("提现类型错误");

            // 支付宝
            if (model.PayType == (int)TiXianPayType.Alipay)
            {
                if (string.IsNullOrEmpty(model.AlipayName))
                    return ErrorJson("请输入支付宝账号");
            }

            if (model.PayType == (int)TiXianPayType.Bank)
            {
                if (string.IsNullOrEmpty(model.KaiHuMing))
                    return ErrorJson("请输入开户行");
                
                if (string.IsNullOrEmpty(model.KaiHuHang))
                    return ErrorJson("请输入开户名");
                
                if (string.IsNullOrEmpty(model.CardNum))
                    return ErrorJson("请输入银行卡号");
            }

            // 金额改为整数 注释验证小数个数
            //var moneyRegex = new Regex(@"^\d+(\.\d{1,2})?$");
            //if (!moneyRegex.IsMatch(money.ToString()))
            //    return ErrorJson("提现金额最多两位小数");

            // 实际提取金额会扣除手续费
            // var actualAmount = Convert.ToInt32(_zhiXiaoSettings.Withdraw_Rate * amount);
            var actualAmount = model.Money;

            var customer = _workContext.CurrentBonusAppCustomer;
            
            // 验证提现金额
            if (customer.Money <= 0)
                return ErrorJson("当前余额为0, 不能提现");

            if (actualAmount > customer.Money)
                return ErrorJson("提现金额超出当前余额");

            #endregion

            // 提现log
            string tixianMsg;
            if (model.PayType == (int)TiXianPayType.Alipay)
            {
                tixianMsg = string.Format("提现方式: 支付宝<br/>支付宝账号: {0}", model.AlipayName);
            }
            else
            {
                tixianMsg = string.Format("提现方式: 银行卡<br/>开户行: {0}<br/>开户名: {0}<br/>银行卡号: {0}", 
                    model.KaiHuHang,
                    model.KaiHuMing,
                    model.CardNum);
            }

            // 加入log
            _customerActivityService.InsertWithdraw(customer,
                actualAmount,
                "提现申请金额: {0}<br/>{1}",
                actualAmount,
                tixianMsg);

            var userMoneyAfterTiXian = customer.Money - actualAmount;

            // log 记录金额变化
            _customerActivityService.InsertActivity(BonusAppConstants.LogType_User_TiXian,
                "提现申请, 原金额: {0}, 提现金额: {1}, 提现后金额: {2}<br/>备注: {3}",
                customer.Money,
                actualAmount,
                userMoneyAfterTiXian,
                tixianMsg);

            // update user money
            customer.Money = userMoneyAfterTiXian;
            _customerService.UpdateCustomer(customer);

            return SuccessJson("提现申请提交, 请等待管理员处理");
        }

        /// <summary>
        /// 提现记录
        /// </summary>
        /// <returns></returns>
        public ActionResult TiXianLog()
        {
            var withdrawLogs = _customerActivityService.GetAllWithdraws(customerId: _workContext.CurrentBonusAppCustomer.Id);
            var model = withdrawLogs.Select(x => new WithdrawLogModel
            {
                Comment = x.Comment,
                Amount = x.Amount,
                IsDone = x.IsDone,
                CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc),
                CompleteOn = x.CompleteOnUtc.HasValue ? _dateTimeHelper.ConvertToUserTime(x.CompleteOnUtc.Value, DateTimeKind.Utc) : (DateTime?) null,
                IpAddress = x.IpAddress,
            });
            return View(model);
        }

        /// <summary>
        /// 资金记录
        /// </summary>
        /// <returns></returns>
        public ActionResult MoneyLog()
        {
            var withdrawLogs = _customerActivityService.GetAllMoneyLogs(customerId: _workContext.CurrentBonusAppCustomer.Id);
            var model = withdrawLogs.Select(x =>
            {
                var m = x.ToModel<MoneyLogModel>();
                m.OrderNum = _customerActivityService.GetMoneyLogOrderNumber(x);
                m.CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc);
                return m;
            });
            return View(model);
        }

        #endregion

        #region Comment
        
        public ActionResult Comment()
        {
            var customer = _workContext.CurrentBonusAppCustomer;
            var canComment = _customerService.CanCustomerComment(customer);

            return View(canComment);
        }

        [HttpPost]
        public ActionResult Comment(
            string content,
            int rate)
        {
            var customer = _workContext.CurrentBonusAppCustomer;
            var canComment = _customerService.CanCustomerComment(customer);
            if (!canComment)
            {
                return ErrorJson("未获得奖励用户不能评论");
            }

            if (string.IsNullOrEmpty(content))
                return ErrorJson("请输入评论内容");

            if (rate <= 0 || rate > 5)
                rate = 5;   // rating default 5

            _customerService.InsertComment(_workContext.CurrentBonusAppCustomer, content, rate);
            
            return SuccessJson("评论成功");
        }

        #endregion
    }
}