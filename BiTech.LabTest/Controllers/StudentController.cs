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
        public StudentLogic TeacherLogic { get; set; }

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
                Session["FullName"]              = viewModel.FullName;
                Session["Class"]                 = viewModel.Class;
                Session["IsCurrentTestFinished"] = false;

                CurrentStudentTestStep = StudentTestStepEnum.WaitingTest;
                return RedirectToAction("WaitingScreen");
            }

            return View();
        }

        public ActionResult WaitingScreen()
        {
            // Chưa nhập thông tin thí sinh
            if (Session["FullName"] == null || Session["Class"] == null)
            {
                return RedirectToAction("JoinTest");
            }

            ViewBag.StudentName = "Nguyễn Trường Giang";
            return View();
        }

        public ActionResult DoTest()
        {
            CurrentStudentTestStep = StudentTestStepEnum.DoingTest;
            return View();
        }
    }
}