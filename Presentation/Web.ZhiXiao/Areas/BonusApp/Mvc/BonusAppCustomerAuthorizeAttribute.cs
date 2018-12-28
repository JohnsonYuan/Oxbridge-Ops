using System;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Web.Framework.Controllers;

namespace Web.ZhiXiao.Areas.BonusApp.Mvc
{
    /// <summary>
    /// bonusapp用户验证
    /// </summary>
    public class BonusAppCustomerAuthorizeAttribute : CustomerAuthorizeAttribute
    {
        #region Ctor

        public BonusAppCustomerAuthorizeAttribute()
            : base(false)
        {
        }

        public BonusAppCustomerAuthorizeAttribute(bool dontValidate)
            : base(dontValidate)
        {
        }

        #endregion

        /// <summary>
        /// Unauthorized action
        /// </summary>
        /// <param name="filterContext"></param>
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectToRouteResult("BonusApp_Login", null);
        }

        /// <summary>
        /// Authentication method
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (_dontValidate)
                return;

            if (filterContext == null)
                throw new ArgumentNullException("filterContext");

            var workContext = EngineContext.Current.Resolve<IWorkContext>();
            var bonusApp_Customer = workContext.CurrentBonusAppCustomer;

            if (bonusApp_Customer == null)
            {
                HandleUnauthorizedRequest(filterContext);
            }
        }
    }
}