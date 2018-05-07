using System;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.ZhiXiao;
using Nop.Core.Infrastructure;

namespace Nop.Web.Framework.Controllers
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited=true, AllowMultiple=true)]
    public class UserPassword2AuthorizeAttribute : FilterAttribute, IAuthorizationFilter
    {
        private readonly bool _dontValidate;
        private readonly string _activeMenuItemSystemName;
        private Customer _customer;

        public UserPassword2AuthorizeAttribute()
            : this(false, null)
        {
        }
        public UserPassword2AuthorizeAttribute(string activeMenuItemSystemName)
            : this(false, activeMenuItemSystemName)
        {
        }

        public UserPassword2AuthorizeAttribute(bool dontValidate, string activeMenuItemSystemName)
        {
            this._dontValidate = dontValidate;
            this._activeMenuItemSystemName = activeMenuItemSystemName;

            this._customer = EngineContext.Current.Resolve<IWorkContext>().CurrentCustomer;
        }

        private void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            // ref: Controller.cs -> RedirectToAction -> RouteValueHelpers.MergeRouteValues
            filterContext.Result = new RedirectToRouteResult(new System.Web.Routing.RouteValueDictionary
            {
                { "controller", "Customer" },
                { "action", "ValidatePassword2" },
                { "returnUrl",  filterContext.RequestContext.HttpContext.Request.RawUrl }
            });
            if (!String.IsNullOrEmpty(_activeMenuItemSystemName))
                filterContext.Controller.TempData["activeMenuItemSystemName"] = _activeMenuItemSystemName;
        }
        
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            if (_dontValidate)
                return;

            if (filterContext == null)
                throw new ArgumentNullException("filterContext");

            if (OutputCacheAttribute.IsChildActionCacheActive(filterContext))
                throw new InvalidOperationException("You cannot use [AdminAuthorize] attribute when a child action cache is active");

            if (!this.HasPassword2Verified(filterContext))
                this.HandleUnauthorizedRequest(filterContext);
        }

        public virtual bool HasPassword2Verified(AuthorizationContext filterContext)
        {
            var customer = EngineContext.Current.Resolve<IWorkContext>().CurrentCustomer;
            var key = string.Format(ZhiXiaoConstants.Password2Key, customer.Id);

            // use TempData validate
            //bool? isValid;
            //if (String.Equals(filterContext.HttpContext.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
            //{
            //    // GET请求只查看, 不清空状态
            //    isValid = filterContext.Controller.TempData.Peek(string.Format(ZhiXiaoConstants.Password2Key, _customer.Id)) as bool?;
            //}
            //else
            //{
            //    //当为post请求时, 清空当前值
            //    isValid = filterContext.Controller.TempData[string.Format(ZhiXiaoConstants.Password2Key, _customer.Id)] as bool?;
            //}
            //return isValid.HasValue && isValid.Value;

            var cacheManager = EngineContext.Current.ContainerManager.Resolve<ICacheManager>("nop_cache_static");

            return cacheManager.IsSet(key) && cacheManager.Get<bool>(key);
        }
    }
}