using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BiTech.LabTest.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var IsTeacher = true;
            if (IsTeacher && Request.UserHostAddress == Request.ServerVariables["LOCAL_ADDR"])
            {
                return RedirectToAction("Index", "Teacher");
            }
            else
            {
                return RedirectToAction("Index", "Student");
            }
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}