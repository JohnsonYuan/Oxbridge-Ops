using System;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Services.BonusApp.Customers;

namespace Web.ZhiXiao.Areas.BonusApp.Mvc
{
    public class BonusAppCustomerLastActivityAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!DataSettingsHelper.DatabaseIsInstalled())
                return;

            if (EngineContext.Current.Resolve<IWorkContext>().CurrentBonusAppCustomer == null)
                return;

            if (filterContext == null || filterContext.HttpContext == null || filterContext.HttpContext.Request == null)
                return;

            //don't apply filter to child methods
            if (filterContext.IsChildAction)
                return;

            //only GET requests
            if (!String.Equals(filterContext.HttpContext.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
                return;

            var workContext = EngineContext.Current.Resolve<IWorkContext>();
            var customer = workContext.CurrentBonusAppCustomer;
            //update last activity date
            if (customer.LastActivityDateUtc.AddMinutes(1.0) < DateTime.UtcNow)
            {
                var customerService = EngineContext.Current.Resolve<IBonusApp_CustomerService>();
                customer.LastActivityDateUtc = DateTime.UtcNow;
                customerService.UpdateCustomer(customer);
            }
        }
    }
}