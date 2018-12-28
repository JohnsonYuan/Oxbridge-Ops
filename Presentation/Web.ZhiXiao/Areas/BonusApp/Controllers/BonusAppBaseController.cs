using System.Web.Mvc;
using Nop.Web.Framework.Controllers;
using Web.ZhiXiao.Areas.BonusApp.Models.UI;

namespace Web.ZhiXiao.Areas.BonusApp.Controllers
{
    public class BonusAppBaseController : BasePublicController
    {
        /// <summary>
        /// 返回自定义格式的json
        /// DataResponse {"data": [data object],"error":null,"success":true}
        /// </summary>
        /// <param name="data"></param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        public ActionResult ResponseJson(bool result, string message, object data)
        {
            return Json(new Response(result, message, data), JsonRequestBehavior.AllowGet);
        }

        public ActionResult ErrorJson(string errorMsg, object data = null)
        {
            return Json(new Response(false, errorMsg, data), JsonRequestBehavior.AllowGet);
        }

        public ActionResult SuccessJson(object data = null)
        {
            return Json(new Response(true, null, data), JsonRequestBehavior.AllowGet);
        }
    }
}