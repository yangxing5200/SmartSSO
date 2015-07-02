using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Passport.Areas.Admin.Controllers
{
    public class ProjectController : Controller
    {
        //
        // GET: /Admin/Project/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Edit(string id)
        {
            return Json("");
        }


    }
}
