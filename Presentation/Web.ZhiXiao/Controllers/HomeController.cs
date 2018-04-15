using Nop.Services.Customers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Web.ZhiXiao.Controllers
{
    public class HomeController : BaseUserController
    {
        private ICustomerService _customerService;

        public HomeController(ICustomerService customerService)
        {
            this._customerService = customerService;
        }

        public ActionResult Index()
        {
            return View();
        }

        //page not found
        public ActionResult Users()
        {
            var customers = _customerService.GetAllCustomers();
            return View(customers);
        }
    }
}