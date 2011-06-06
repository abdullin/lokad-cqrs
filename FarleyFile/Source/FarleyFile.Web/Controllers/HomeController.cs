using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FarleyFile.Web.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            var model = FarleyClient.GetUserDashboard();
            return View(model);
        }


        public ActionResult AddContact(string name)
        {
            FarleyClient.Send(new AddContact(name));
            return Content("Cool");
        }
    }
}
