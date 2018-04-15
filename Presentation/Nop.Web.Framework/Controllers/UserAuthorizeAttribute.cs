using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Infrastructure;
using Nop.Services.Security;

namespace Nop.Web.Framework.Controllers
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited=true, AllowMultiple=true)]
    public class UserAuthorizeAttribute : FilterAttribute, IAuthorizationFilter
    {
        private readonly bool _dontValidate;


        public UserAuthorizeAttribute()
            : this(false)
        {
        }

        public UserAuthorizeAttribute(bool dontValidate)
        {
            this._dontValidate = dontValidate;
        }

        private void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new HttpUnauthorizedResult();
        }

        private IEnumerable<UserAuthorizeAttribute> GetUserAuthorizeAttributes(ActionDescriptor descriptor)
        {
            return descriptor.GetCustomAttributes(typeof(UserAuthorizeAttribute), true)
                .Concat(descriptor.ControllerDescriptor.GetCustomAttributes(typeof(UserAuthorizeAttribute), true))
                .OfType<UserAuthorizeAttribute>();
        }

        private bool IsUserPageRequested(AuthorizationContext filterContext)
        {
            var adminAttributes = GetUserAuthorizeAttributes(filterContext.ActionDescriptor);
            if (adminAttributes != null && adminAttributes.Any())
                return true;
            return false;
        }

        public void OnAuthorization(AuthorizationContext filterContext)
        {
            if (_dontValidate)
                return;

            if (filterContext == null)
                throw new ArgumentNullException("filterContext");

            if (OutputCacheAttribute.IsChildActionCacheActive(filterContext))
                throw new InvalidOperationException("You cannot use [AdminAuthorize] attribute when a child action cache is active");



            if (IsUserPageRequested(filterContext))
            {
                if (!this.HasRegisteredAccess())
                    this.HandleUnauthorizedRequest(filterContext);
            }
        }

        public virtual bool HasRegisteredAccess()
        {
            var workContext = EngineContext.Current.Resolve<IWorkContext>();
            var customer = workContext.CurrentCustomer;
            if (customer == null)
                return false;

            return customer.IsRegistered();
            //var permissionService = EngineContext.Current.Resolve<IPermissionService>();
            //bool result = permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel);
            //return result;
        }
    }
}
