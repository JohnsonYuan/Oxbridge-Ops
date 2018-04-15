using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Web.ZhiXiao.Controllers
{
    public partial class WidgetController : BasePublicController
    {
        #region Methods

        [ChildActionOnly]
        public virtual ActionResult WidgetsByZone(string widgetZone, object additionalData = null)
        {
            return Content("");
            //var model = _widgetModelFactory.GetRenderWidgetModels(widgetZone, additionalData);

            ////no data?
            //if (!model.Any())
            //    return Content("");

            //return PartialView(model);
        }

        #endregion
    }
}