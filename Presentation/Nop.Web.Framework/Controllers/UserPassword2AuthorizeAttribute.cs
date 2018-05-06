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
        private readonly ICacheManager _cacheManager;
        private readonly Customer _customer;

        public UserPassword2AuthorizeAttribute()
            : this(false)
        {
        }

        public UserPassword2AuthorizeAttribute(bool dontValidate)
        {
            this._dontValidate = dontValidate;
            
            //this._cacheManager = EngineContext.Current.ContainerManager.Resolve<ICacheManager>("nop_cache_per_request");  
            this._cacheManager = EngineContext.Current.ContainerManager.Resolve<ICacheManager>("nop_cache_static");  
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
        }
        
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            if (_dontValidate)
                return;

            if (filterContext == null)
                throw new ArgumentNullException("filterContext");

            if (OutputCacheAttribute.IsChildActionCacheActive(filterContext))
                throw new InvalidOperationException("You cannot use [AdminAuthorize] attribute when a child action cache is active");

            if (!this.HasPassword2Verified())
                this.HandleUnauthorizedRequest(filterContext);
        }

        public virtual bool HasPassword2Verified()
        {
            return _cacheManager.IsSet(string.Format(ZhiXiaoConstants.Password2Key, _customer.Id)) &&
                _cacheManager.Get<bool>(string.Format(ZhiXiaoConstants.Password2Key, _customer.Id));
        }
    }
}