using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SSO.Attributes;

namespace Passport.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult UserCenter()
        {
            ViewBag.User= Session["User"];
            return View();
        }
    }
}
