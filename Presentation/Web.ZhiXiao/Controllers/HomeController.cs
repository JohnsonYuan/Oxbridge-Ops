using Nop.Services.Customers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Web.ZhiXiao.Controllers
{
    public class HomeController : Controller
    {
        private ICustomerService _customerService;

        public HomeController(ICustomerService customerService)
        {
            this._customerService = customerService;
        }

        //page not found
        public ActionResult Index()
        {
            var customers = _customerService.GetAllCustomers();
            return View(customers);
        }
    }
}