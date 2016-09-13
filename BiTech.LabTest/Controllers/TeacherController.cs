using BiTech.LabTest.BLL;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO.Compression;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using BiTech.LabTest.BLL.Interfaces;
using BiTech.LabTest.DAL.Models;
using static BiTech.LabTest.Models.ViewModels.Teacher;

namespace BiTech.LabTest.Controllers
{
    public class TeacherController : Controller
    {
        public ITeacherLogic TeacherLogic { get; set; }

        public TestInformationViewModel TestDataViewModel { get; set; }


        public TeacherController()
        {dasdasdasd
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

        #region New test
        public ActionResult NewTest()
        {
            // Thông tin bài test
            if (Session["TestInfomration"] != null)
            {
                TestDataViewModel = (Models.ViewModels.Teacher.TestInformationViewModel)Session["TestInfomration"];
            }
            else
            {
                TestDataViewModel = new TestInformationViewModel();
            }

            // Kiểm tra quyền của người truy cập
            if (Request.UserHostAddress != Request.ServerVariables["LOCAL_ADDR"])
            {
                return RedirectToAction("Index", "Student");
            }

            return View(TestDataViewModel);
        }

        [HttpPost]
        public ActionResult StartTest()
        {
            // Thông tin bài test
            TestDataViewModel = (Models.ViewModels.Teacher.TestInformationViewModel)Session["TestInfomration"];


            if (TestDataViewModel == new TestInformationViewModel())
            {
                return Json(new { success = 0 });
            }

            // Kiểm tra quyền của người truy cập
            if (Request.UserHostAddress != Request.ServerVariables["LOCAL_ADDR"])
            {
                return RedirectToAction("Index", "Student");
            }

            TestDataViewModel.TestId = TeacherLogic.StartTest(TestDataViewModel.TestData);

            if (string.IsNullOrEmpty(TestDataViewModel.TestId) == false)
            {
                TestDataViewModel.TestStep = TestData.TestStepEnum.OnWorking;
            }

            Session["TestInfomration"] = TestDataViewModel;

            return Json(new { success = 1, testId = TestDataViewModel.TestId });
        }

        [HttpPost]
        public async Task<ActionResult> UploadTestData()
        {
            if (Request.Files.Count > 0)
            {
                var file = Request.Files[0];
                if (file != null && file.ContentLength > 0)
                {
                    #region Kiểm tra thư mục lưu đề thi
                    var testDataFolder = Tool.GetConfiguration("TestDataFolder");
                    var testDataServerPath = Server.MapPath(testDataFolder);

                    if (Directory.Exists(testDataServerPath) == false)
                    {
                        Directory.CreateDirectory(testDataServerPath);
                    }
                    #endregion

                    // Cấu trúc tên file [original_file_name]-[Guid].[file_extension]
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
                                        testDataInString = await streamReader.ReadToEndAsync();
                                    }
                                }
                            }
                        }
                    }


                    Response.StatusCode = (int)System.Net.HttpStatusCode.OK;

                    var testDataInJson  = JObject.Parse(testDataInString);
                    var testInformation = new Models.ViewModels.Teacher.TestInformationViewModel
                    {
                        TestData = testDataInString,
                        TestStep = TestData.TestStepEnum.Waiting,
                        Subject  = testDataInJson["Test"]["Header"]["Subject"].ToString(),
                        Grade    = testDataInJson["Test"]["Header"]["Grade"].ToString()
                    };

                    // Lấy danh sách các nhóm câu hỏi
                    var groups = (JObject) testDataInJson["Test"]["Groups"];

                    foreach (var item in groups)
                    {
                        var currentItem = JObject.Parse(item.Value.ToString());

                        var questionGroup = new QuestionGroupInformation
                        {
                            Title          = currentItem["GroupInfo"]["Title"].ToString(),
                            TotalQuiz      = (currentItem["Quiz"]?["Quiz_Q_Single"]?["Quiz_S_Question"]?.Count() ?? 0) + (currentItem["Quiz"]?["Quiz_Q_Group"]?["Quiz_QG_Question"]?.Count() ?? 0),
                            TotalUnderline = (currentItem["Underline"]?["Underline_Q_Single"]?["Underline_S_Question"]?.Count() ?? 0) + (currentItem["Underline"]?["Underline_Q_Group"]?["Underline_QG_Question"]?.Count() ?? 0),
                            TotalFill      = (currentItem["Fill"]?["Fill_Q_Single"]?["Fill_S_Question"]?.Count() ?? 0) + (currentItem["Fill"]?["Fill_Q_Group"]?["Fill_QG_Question"]?.Count() ?? 0),
                            TotalTrueFalse = (currentItem["TrueFalse"]?["TrueFalse_Q_Single"]?["TrueFalse_S_Question"]?.Count() ?? 0) + (currentItem["TrueFalse"]?["TrueFalse_Q_Group"]?["TrueFalse_QG_Question"]?.Count() ?? 0),
                            TotalMatching  = (currentItem["Matching"]?["Matching_Q_Single"]?["Matching_S_Question"]?.Count() ?? 0) + (currentItem["Matching"]?["Matching_Q_Group"]?["Matching_QG_Question"]?.Count() ?? 0)
                        };

                        testInformation.QuestionGroups.Add(questionGroup);
                    }

                    Session["TestInfomration"] = testInformation;

                    return Json(new
                    {
                        status  = "OK",
                        subject = testDataInJson["Test"]["Header"]["Subject"].ToString(),
                        grade   = testDataInJson["Test"]["Header"]["Grade"].ToString(),
                    }, "text/plain");
                }
            }

            Response.StatusCode = (int)System.Net.HttpStatusCode.InternalServerError;
            return Json(new { status = "NO_OK" }, "text/plain");
        }
        #endregion

        public ActionResult TestHistories()
        {
            if (Request.UserHostAddress != Request.ServerVariables["LOCAL_ADDR"])
            {
                return RedirectToAction("Index", "Student");
            }

            return View();
        }
    }
}