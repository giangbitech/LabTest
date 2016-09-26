using BiTech.LabTest.BLL;
using BiTech.LabTest.DAL.Models;
using BiTech.LabTest.Models.Enums;
using System.Web.Mvc;
using static BiTech.LabTest.Models.ViewModels.Student;

namespace BiTech.LabTest.Controllers
{
    public class StudentController : Controller
    {
        public StudentLogic StudentLogic { get; set; }

        public StudentTestStepEnum CurrentStudentTestStep { get; set; }


        public StudentController()
        {
            // Xác định quyền của người dùng trong controller này
            ViewBag.ApplicationRole = Models.Enums.ApplicationRole.Student;
            StudentLogic = new StudentLogic();
        }


        public ActionResult Index()
        {
            return RedirectToAction("JoinTest");
        }

        /// <summary>
        /// Màn hình nhập thông tin cá nhân của thí sinh
        /// </summary>
        /// <returns></returns>
        public ActionResult JoinTest()
        {
            return View();
            //string studentIpAddress = Request.UserHostAddress;
           

            //TestData lastTest = null;

            //// Bài gần đây nhất là hoàn tất thì bắt nhập lại thông tin mới để thi bài khác.
            //if (StudentLogic.IsLastTestFinished(out lastTest))
            //{
            //    // Nhập lại thông tin mới để thi bài thi khác.
            //    Session["StudentInformation"] = null;
            //    return View();
            //}
            //else
            //{
            //    // LastTest đang thi thì chuyển vào màn hình thi để thi típ.
            //    if (lastTest != null && lastTest.TestStep == TestData.TestStepEnum.OnWorking)
            //    {
            //        TestResult testResult = null;
            //        // Thí sinh đang trong vòng thi hiện tại.
            //        if (StudentLogic.IsCurrentIpOnTestProgress(studentIpAddress, lastTest.Id, out testResult))
            //        {
            //            Session["StudentInformation"] = testResult;
            //            // Vào màn hình tiếp tục thi
            //            return RedirectToAction("DoTest");
            //        }
            //        else
            //        {
            //            // Chờ vòng thi hiện tại kết thúc
            //            // Xóa dữ liệu thống kê online của IP máy này.
            //            this.StudentLogic.DeleteOnlineByIpAddress(studentIpAddress);
            //            return RedirectToAction("WaitingAnotherTestFinished");
            //        }
            //    }
            //    else
            //    {
            //        // Chờ giáo viên mở phòng thi mới
            //        // Nhập thông tin dự thi
            //        return View();
            //    }
            //}
        }

        [HttpPost]
        [ActionName("JoinTest")]
        public ActionResult DoJoinTest(StudentJoinTestViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                string studentIpAddress = Request.UserHostAddress;
                string studentName      = viewModel.FullName;
                string studentClass     = viewModel.Class;
                var testStep            = TestData.TestStepEnum.Waiting;


                var testResult = Session["StudentInformation"] = new TestResult
                {
                    StudentIPAddress = studentIpAddress,
                    StudentName      = studentName,
                    StudentClass     = studentClass,
                    TestStep         = testStep
                };

                // Cập nhật thông tin online
                StudentLogic.UpdateLastOnline(studentIpAddress, studentName, studentClass, testStep);

                return RedirectToAction("WaitingScreen");
            }

            return View();
        }

        public ActionResult WaitingScreen()
        {
            var studentInformation = (TestResult) Session["StudentInformation"];


            var viewModel = new WaitingScreenViewModel
            {
                UserIpAddress = studentInformation.StudentIPAddress,
                FullName      = studentInformation.StudentName,
                Class         = studentInformation.StudentClass
            };

            return View(viewModel);
            //// Chưa nhập thông tin thí sinh
            //if (Session["FullName"] == null || Session["Class"] == null)
            //{
            //    return RedirectToAction("JoinTest");
            //}

            //string studentIpAddress = Request.UserHostAddress;
            //string studentName      = Session["FullName"].ToString();
            //string studentClass     = Session["Class"].ToString();

            //var viewModel = new WaitingScreenViewModel {
            //    UserIpAddress = studentIpAddress,
            //    FullName      = studentName,
            //    Class         = studentClass
            //};

            //return View(viewModel);
        }

        public ActionResult DoTest()
        {
            var studentInformation      = (TestResult)Session["StudentInformation"];
            studentInformation.TestStep = TestData.TestStepEnum.OnWorking;


            var viewModel = new WaitingScreenViewModel
            {
                UserIpAddress = studentInformation.StudentIPAddress,
                FullName      = studentInformation.StudentName,
                Class         = studentInformation.StudentClass
            };

            Session["StudentInformation"] = studentInformation;

            ViewBag.IpAddress = studentInformation.StudentIPAddress;
            return View();
        }

        public ActionResult FinishTest()
        {
            string studentIpAddress = Request.UserHostAddress;
            string studentName = "Nguyễn Trường Giang";
            string studentClass = "12c1";

            return View();
        }
    }
}