using BiTech.LabTest.BLL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.IO.Compression;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using BiTech.LabTest.DAL.Models;
using static BiTech.LabTest.Models.ViewModels.Teacher;

namespace BiTech.LabTest.Controllers
{
    public class TeacherController : Controller
    {
        public TeacherLogic TeacherLogic { get; set; }

        public TestInformationViewModel TestDataViewModel { get; set; }


        public TeacherController()
        {
            // Xác định quyền của người dùng trong controller này
            ViewBag.ApplicationRole = Models.Enums.ApplicationRole.Teacher;
            TeacherLogic = new TeacherLogic();
        }


        public ActionResult Index()
        {
            if (Request.UserHostAddress != Request.ServerVariables["LOCAL_ADDR"])
            {
                return RedirectToAction("Index", "Student");
            }

            return RedirectToAction("NewTest");
        }

        /// <summary>
        /// Màn hình tổ chức thi. 
        /// Upload bài thi
        /// Giám sát thi
        /// </summary>
        /// <returns></returns>
        public ActionResult NewTest()
        {
            // Kiểm tra quyền của người truy cập
            if (Request.UserHostAddress != Request.ServerVariables["LOCAL_ADDR"])
            {
                return RedirectToAction("Index", "Student");
            }

            // Thông tin bài test
            if (Session["TestInfomration"] != null)
            {
                TestDataViewModel = (Models.ViewModels.Teacher.TestInformationViewModel)Session["TestInfomration"];
            }
            else
            {
                TestDataViewModel = new TestInformationViewModel();
            }

            // Tính thời gian làm bài còn lại nếu đang trong quá trình thi
            if (TestDataViewModel.TestStep == TestData.TestStepEnum.OnWorking)
            {
                // Số giây còn lại để làm bài thi
                var remainingSeconds                   = TestDataViewModel.EndDateTime.Subtract(DateTime.Now).TotalSeconds;
                TestDataViewModel.RemainingTestSeconds = int.Parse(Math.Ceiling(remainingSeconds).ToString());

                // Kiểm tra hết giờ làm bài chưa ?
                if (remainingSeconds <= 0)
                {
                    TestDataViewModel.TestStep = TestData.TestStepEnum.Finish;
                }
            }


            Session["TestInfomration"] = TestDataViewModel;
            return View(TestDataViewModel);
        }

        /// <summary>
        /// Kết thúc bài thi hiện tại.
        /// </summary>
        /// <returns></returns>
        public ActionResult EndTest()
        {
            Session["TestInfomration"] = null;
            return RedirectToAction("NewTest");
        }

        /// <summary>
        /// Bắt đầu tính giờ làm bài
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult StartTest()
        {
            // Thông tin bài test
            TestDataViewModel = (TestInformationViewModel)Session["TestInfomration"];


            if (TestDataViewModel == new TestInformationViewModel())
            {
                return Json(new { success = 0 });
            }

            // Kiểm tra quyền của người truy cập
            if (Request.UserHostAddress != Request.ServerVariables["LOCAL_ADDR"])
            {
                return RedirectToAction("Index", "Student");
            }

            // Tiến hành thi
            TestDataViewModel.TestId = TeacherLogic.StartTest(TestDataViewModel.TestData);

            if (string.IsNullOrEmpty(TestDataViewModel.TestId) == false)
            {
                TestDataViewModel.TestStep      = TestData.TestStepEnum.OnWorking;
                TestDataViewModel.StartDateTime = DateTime.Now;
                TestDataViewModel.EndDateTime   = DateTime.Now.AddMinutes(TestDataViewModel.TotalMinutes);
            }

            Session["TestInfomration"] = TestDataViewModel;

            return Json(new
            {
                success              = 1,
                testId               = TestDataViewModel.TestId,
                remainingTestSeconds = TestDataViewModel.RemainingTestSeconds
            });
        }

        /// <summary>
        /// Nhận & xữ lý file để thi do giáo viên đưa lên.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> UploadTestData()
        {
            if (Request.Files.Count > 0)
            {
                var file = Request.Files[0];
                if (file != null && file.ContentLength > 0)
                {
                    #region Kiểm tra thư mục lưu đề thi
                    var testDataFolder     = Tool.GetConfiguration("TestDataFolder");
                    var testDataServerPath = Server.MapPath(testDataFolder);

                    if (Directory.Exists(testDataServerPath) == false)
                    {
                        Directory.CreateDirectory(testDataServerPath);
                    }
                    #endregion

                    // Cấu trúc tên file [original_file_name]-[Guid].[file_extension]
                    //todo: kiểm tra file upload lên quá thì nhiều thì phải làm sao
                    string extension = Path.GetExtension(file.FileName);
                    string filename  = Path.GetFileNameWithoutExtension(file.FileName) + "-" + Guid.NewGuid() + extension;
                    var path         = Path.Combine(testDataServerPath, filename);

                    // Lưu file vào Server
                    file.SaveAs(path);

                    // Nội dung đề thi data-format in string
                    string testDataInString = null;
                    using (ZipArchive archive = ZipFile.OpenRead(Server.MapPath(testDataFolder + filename)))
                    {
                        foreach (var archiveFile in archive.Entries)
                        {
                            if (archiveFile.Name.EndsWith("data.json"))
                            {
                                var sample = archive.GetEntry(archiveFile.FullName);
                                if (sample != null)
                                {
                                    using (var zipEntryStream = sample.Open())
                                    {
                                        StreamReader streamReader = new StreamReader(zipEntryStream);
                                        testDataInString          = await streamReader.ReadToEndAsync();
                                    }
                                }
                            }
                        }
                    }


                    Response.StatusCode = (int)System.Net.HttpStatusCode.OK;

                    var testDataInJson = JObject.Parse(testDataInString);
                    TestDataViewModel = new Models.ViewModels.Teacher.TestInformationViewModel
                    {
                        TestData             = testDataInString,
                        TestStep             = TestData.TestStepEnum.Waiting,
                        Subject              = testDataInJson["test"]["header"]["subject"].ToString(),
                        Grade                = testDataInJson["test"]["header"]["grade"].ToString(),
                        TotalMinutes         = int.Parse(testDataInJson["test"]["header"]["time"].ToString()),
                        RemainingTestSeconds = int.Parse(testDataInJson["test"]["header"]["time"].ToString()) * 60,
                        Type                 = testDataInJson["test"]["header"]["type"].ToString(),
                        Year                 = testDataInJson["test"]["header"]["year"].ToString(),
                        QuestionGroups       = new List<QuestionGroupInformation>()
                    };

                    // Lấy danh sách các nhóm câu hỏi
                    var groups = (JArray)testDataInJson["test"]["groups"];

                    foreach (var item in groups)
                    {

                        //todo: Kiểm tra số lượng câu hỏi parse ra đúng chưa 
                        var currentItem = JObject.Parse(item.ToString());

                        var totalQuizQuestionInGroup      = System.Text.RegularExpressions.Regex.Matches(currentItem["quiz"]?["qGroup"].ToString(), "sID").Count;
                        var totalUnderlineQuestionInGroup = System.Text.RegularExpressions.Regex.Matches(currentItem["underline"]?["qGroup"].ToString(), "sID").Count;
                        var totalFillQuestionInGroup      = System.Text.RegularExpressions.Regex.Matches(currentItem["fill"]?["qGroup"].ToString(), "sID").Count;
                        var totalTrueFalseQuestionInGroup = System.Text.RegularExpressions.Regex.Matches(currentItem["trueFalse"]?["qGroup"].ToString(), "sID").Count;
                        var totalMatchingQuestionInGroup  = System.Text.RegularExpressions.Regex.Matches(currentItem["matching"]?["qGroup"].ToString(), "sID").Count;

                        var questionGroup = new QuestionGroupInformation
                        {
                            Title          = currentItem["groupInfo"]["title"].ToString(),
                            TotalQuiz      = (currentItem["quiz"]?["qSingle"]?.Count() ?? 0) + totalQuizQuestionInGroup,
                            TotalUnderline = (currentItem["underline"]?["qSingle"]?.Count() ?? 0) + totalUnderlineQuestionInGroup,
                            TotalFill      = (currentItem["fill"]?["qSingle"]?.Count() ?? 0) + totalFillQuestionInGroup,
                            TotalTrueFalse = (currentItem["trueFalse"]?["qSingle"]?.Count() ?? 0) + totalTrueFalseQuestionInGroup,
                            TotalMatching  = (currentItem["matching"]?["qSingle"]?.Count() ?? 0) + totalMatchingQuestionInGroup
                        };

                        TestDataViewModel.QuestionGroups.Add(questionGroup);
                    }

                    Session["TestInfomration"] = TestDataViewModel;

                    return Json(new
                    {
                        status               = "OK",
                        subject              = TestDataViewModel.Subject,
                        grade                = TestDataViewModel.Grade,
                        totalMinutes         = TestDataViewModel.TotalMinutes,
                        remainingTestSeconds = TestDataViewModel.RemainingTestSeconds,
                        type                 = TestDataViewModel.Type,
                        year                 = TestDataViewModel.Year
                    }, "text/plain");
                }
            }

            Response.StatusCode = (int)System.Net.HttpStatusCode.InternalServerError;
            return Json(new { status = "NO_OK" }, "text/plain");
        }

        /// <summary>
        /// Lấy thông tin bài thi sau khi đã upload lên
        /// </summary>
        /// <returns></returns>
        public ActionResult GetUploadedTestInformation()
        {
            var testInformation = (TestInformationViewModel)Session["TestInfomration"];
            return View("_TestDataGroupStatistic", testInformation.QuestionGroups);
        }

        /// <summary>
        /// Danh sách các Bài test đã được tổ chức thi.
        /// </summary>
        /// <returns></returns>
        public ActionResult TestHistories()
        {
            if (Request.UserHostAddress != Request.ServerVariables["LOCAL_ADDR"])
            {
                return RedirectToAction("Index", "Student");
            }

            return View();
        }

        /// <summary>
        /// Cho client request lên để giữ Session, ko cần xữ lý gì thêm
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult KeepSession()
        {
            return Json(new { success = true });
        }
    }
}