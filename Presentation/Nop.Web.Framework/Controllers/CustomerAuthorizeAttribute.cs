using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Infrastructure;

namespace Nop.Web.Framework.Controllers
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited=true, AllowMultiple=true)]
    public class CustomerAuthorizeAttribute : FilterAttribute, IAuthorizationFilter
    {
        protected readonly bool _dontValidate;


        public CustomerAuthorizeAttribute()
            : this(false)
        {
        }

        public CustomerAuthorizeAttribute(bool dontValidate)
        {
            this._dontValidate = dontValidate;
        }

        protected virtual void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new HttpUnauthorizedResult();
        }

        private IEnumerable<CustomerAuthorizeAttribute> GetCustomerAuthorizeAttributes(ActionDescriptor descriptor)
        {
            return descriptor.GetCustomAttributes(typeof(CustomerAuthorizeAttribute), true)
                .Concat(descriptor.ControllerDescriptor.GetCustomAttributes(typeof(CustomerAuthorizeAttribute), true))
                .OfType<CustomerAuthorizeAttribute>();
        }

        private bool IsCustomerPageRequested(AuthorizationContext filterContext)
        {
            var customerAttributes = GetCustomerAuthorizeAttributes(filterContext.ActionDescriptor);
            if (customerAttributes != null && customerAttributes.Any())
                return true;
            return false;
        }

        public virtual void OnAuthorization(AuthorizationContext filterContext)
        {
            if (_dontValidate)
                return;

            if (filterContext == null)
                throw new ArgumentNullException("filterContext");

            if (OutputCacheAttribute.IsChildActionCacheActive(filterContext))
                throw new InvalidOperationException("You cannot use [AdminAuthorize] attribute when a child action cache is active");

            if (IsCustomerPageRequested(filterContext))
            {
                if (!this.IsInRegisterRole())
                    this.HandleUnauthorizedRequest(filterContext);
            }
        }

        public virtual bool IsInRegisterRole()
        {
            var workContext = EngineContext.Current.Resolve<IWorkContext>();
            var customer = workContext.CurrentCustomer;

            bool result = (customer != null) &&
                (customer.IsRegistered() || customer.IsRegistered_Advanced());
            return result;
        }
    }
}
