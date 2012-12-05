using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TestApp.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        /*
        public ActionResult About()
        {
            ViewBag.Message = "I don't know why it hasn't been working so far.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Please, for the love of God, just deploy all the references with the project.";

            return View();
        }
         */
    }
}
