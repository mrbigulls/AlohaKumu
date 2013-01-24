using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AlohaKumu.Models;
using TestApp.Models;

namespace AlohaKumu.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index( FormCollection form )
        {
            String username = form["username"];
            String password = form["password"];
            if (!DataAccessor.isUser(username))
            {
                Session["Error"] = "No such user: " + username;
                return RedirectToAction("Error");
            }
            User current = DataAccessor.login(username, password);
            if (current == null)
            {
                Session["Error"] = "Invalid password";
                return RedirectToAction("Error");
            }
            Session["User"] = current;
            return View();
        }

        public ActionResult LogOff()
        {
            Session["User"] = null;
            return RedirectToAction("Index");
        }

        public ActionResult Error()
        {
            return View();
        }
    }
}
