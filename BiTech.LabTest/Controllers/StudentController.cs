using BiTech.LabTest.BLL;
using BiTech.LabTest.Models.Enums;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static BiTech.LabTest.Models.ViewModels.Student;

namespace BiTech.LabTest.Controllers
{
    public class StudentController : Controller
    {
        public StudentLogic _StudentLogic { get; set; }

        public StudentTestStepEnum CurrentStudentTestStep { get; set; }

        public StudentController()
        {
            // Xác định quyền của người dùng trong controller này
            ViewBag.ApplicationRole = Models.Enums.ApplicationRole.Student;
        }


        public ActionResult Index()
        {
            return RedirectToAction("JoinTest");
        }

        public ActionResult JoinTest()
        {

            // Thí sinh đã nhập thông tin
            // Chưa thi thì chuyển vào Waiting
            if (CurrentStudentTestStep == StudentTestStepEnum.WaitingTest)
            {
                return RedirectToAction("WaitingScreen");
            }

            return View();
        }

        [HttpPost]
        [ActionName("JoinTest")]
        public ActionResult DoJoinTest(StudentJoinTestViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                Session["FullName"] = viewModel.FullName;
                Session["Class"] = viewModel.Class;
                Session["IsCurrentTestFinished"] = false;
                Session["IsTestStarted"] = false;

                CurrentStudentTestStep = StudentTestStepEnum.WaitingTest;
                return RedirectToAction("WaitingScreen");
            }

            return View();
        }

        public ActionResult WaitingScreen()
        {
            // Chưa nhập thông tin thí sinh
            if (Session["FullName"] == null || Session["Class"] == null || Session["IsTestStarted"] == null)
            {
                return RedirectToAction("JoinTest");
            }
            if (Session["FullName"].ToString().Length == 0 || Session["Class"].ToString().Length == 0)
            {
                return RedirectToAction("JoinTest");
            }
            ViewBag.StudentName = Session["FullName"].ToString();
            return View();
        }


        [HttpPost]
        [ActionName("DoTest")]
        public ActionResult SubmitDoTest(string testDataId)
        {
            Session["IsTestStarted"] = true;


            return Json(new { testDataId = testDataId });
        }
     
        public ActionResult DoTest(string testDataId)
        {
            Session["IsTestStarted"] = true;
            string data = _StudentLogic.GetTest(testDataId);


            string dethiodangjson = "";

            return Json(dethiodangjson);
        }



        public ActionResult ChangeName()
        {
            if (!bool.Parse(Session["IsTestStarted"].ToString()))
            {
                Session["FullName"] = "";
                Session["Class"] = "";
                return RedirectToAction("JoinTest");

            }
            return RedirectToAction("WaitingScreen");
        }
    }
}