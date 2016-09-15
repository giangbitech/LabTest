using BiTech.LabTest.BLL;
using BiTech.LabTest.Models.Enums;
using BiTech.LabTest.Models.ViewModels.Interface;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Activities;
using System.Collections.Generic;
using System.IO;
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
            //Đã bắt đầu thi thì vào thi luôn
            if (CurrentStudentTestStep == StudentTestStepEnum.DoingTest)
            {
                return RedirectToAction("DoTest");
            }

            ViewBag.StudentName = Session["FullName"].ToString();
            ViewBag.ClassName = Session["Class"].ToString();
            return View();
        }


        [HttpPost]
        [ActionName("DoTest")]
        public ActionResult SubmitDoTest(string testDataId)
        {
            Session["IsTestStarted"] = true;
            Session["TestDataId"] = testDataId;

            return Json(new { testDataId = testDataId });
        }

        public ActionResult DoTest()
        {
            //Nếu chưa có Test Data Id thì vào đăng ký lại
            if (Session["TestDataId"] == null)
            {
                return RedirectToAction("JoinTest");
            }
            string testDataId = Session["TestDataId"].ToString();

            ViewBag.TestDataID = testDataId;
            //string dethiodangjson = "";
            //ViewBag.Data = Json(new { group = "gg", ddd = "dd" });

            //var dbTestData = _StudentLogic.GetTest(testDataId);
            //var testDataInJson = JObject.Parse(dbTestData.Data);

            StreamReader sr = new StreamReader(Server.MapPath("~/Content/data.json"));
            CurrentStudentTestStep = StudentTestStepEnum.DoingTest;
            var testDataInJson = JObject.Parse(sr.ReadToEnd());

            #region Parse cac cau hoi de xuat ra man hinh
            var testGroupList = new List<ITestGroupViewModel>();
            // lấy từng nhóm câu hỏi ra
            for (int i = 0; i < testDataInJson["test"]["groups"].Count(); i++)
            {
                TestGroupViewModel testGruop = new TestGroupViewModel();

                testGruop.Title = testDataInJson["test"]["groups"][i]["groupInfo"]["title"]?.ToString();

                #region Câu Trắc Nghiệm - Quiz Question
                testGruop.Quiz = new QuestionsOfType();

                // lấy câu hỏi trắc nghiệm đơn - get single quiz
                testGruop.Quiz.ListOfSingles = new List<Models.ViewModels.Interface.ISingleQuestionViewModel>();
                for (int j = 0; j < testDataInJson["test"]["groups"][i]["quiz"]["qSingle"].Count(); j++)
                {
                    SingleQuestionViewModel sqvm = new SingleQuestionViewModel();
                    sqvm.ID = testDataInJson["test"]["groups"][i]["quiz"]["qSingle"][j]["sID"]?.ToString();
                    sqvm.Score = testDataInJson["test"]["groups"][i]["quiz"]["qSingle"][j]["sScore"]?.ToString();
                    sqvm.Content = testDataInJson["test"]["groups"][i]["quiz"]["qSingle"][j]["sContain"]?.ToString();
                    sqvm.QuestionType = QuestionTypeEnum.Quiz;
                    sqvm.AnswerList = new List<Models.ViewModels.Interface.IAnswerViewModel>();
                    // lấy đáp án
                    for (int k = 0; k < testDataInJson["test"]["groups"][i]["quiz"]["qSingle"][j]["sAList"].Count(); k++)
                    {
                        AnswerViewModel avm = new AnswerViewModel();
                        avm.Content = testDataInJson["test"]["groups"][i]["quiz"]["qSingle"][j]["sAList"][k]["sAContain"]?.ToString();
                        avm.RightAnswer = testDataInJson["test"]["groups"][i]["quiz"]["qSingle"][j]["sAList"][k]["sAw"]?.ToString();
                        sqvm.AnswerList.Add(avm);
                    }
                    testGruop.Quiz.ListOfSingles.Add(sqvm);
                }

                // lấy câu hỏi trắc nghiệm chùm - get group quiz
                testGruop.Quiz.ListOfGroups = new List<Models.ViewModels.Interface.IGroupQuestionViewModel>();
                for (int l = 0; l < testDataInJson["test"]["groups"][i]["quiz"]["qGroup"].Count(); l++)
                {
                    GroupQuestionViewModel gqvm = new GroupQuestionViewModel();
                    gqvm.Content = testDataInJson["test"]["groups"][i]["quiz"]["qGroup"][l]["qgTitle"]?.ToString();
                    gqvm.SingleQuestionList = new List<Models.ViewModels.Interface.ISingleQuestionViewModel>();
                    for (int j = 0; j < testDataInJson["test"]["groups"][i]["quiz"]["qGroup"][l]["quizSList"].Count(); j++)
                    {
                        SingleQuestionViewModel sqvm = new SingleQuestionViewModel();
                        sqvm.ID = testDataInJson["test"]["groups"][i]["quiz"]["qGroup"][l]["quizSList"][j]["sID"]?.ToString();
                        sqvm.Score = testDataInJson["test"]["groups"][i]["quiz"]["qGroup"][l]["quizSList"][j]["sScore"]?.ToString();
                        sqvm.Content = testDataInJson["test"]["groups"][i]["quiz"]["qGroup"][l]["quizSList"][j]["sContain"]?.ToString();
                        sqvm.QuestionType = QuestionTypeEnum.Quiz;
                        sqvm.AnswerList = new List<Models.ViewModels.Interface.IAnswerViewModel>();
                        // lấy đáp án
                        for (int k = 0; k < testDataInJson["test"]["groups"][i]["quiz"]["qGroup"][l]["quizSList"][j]["sAList"].Count(); k++)
                        {
                            AnswerViewModel avm = new AnswerViewModel();
                            avm.Content = testDataInJson["test"]["groups"][i]["quiz"]["qGroup"][l]["quizSList"][j]["sAList"][k]["sAContain"]?.ToString();
                            avm.RightAnswer = testDataInJson["test"]["groups"][i]["quiz"]["qGroup"][l]["quizSList"][j]["sAList"][k]["sAw"]?.ToString();
                            sqvm.AnswerList.Add(avm);
                        }
                        gqvm.SingleQuestionList.Add(sqvm);
                    }
                    testGruop.Quiz.ListOfGroups.Add(gqvm);
                }
                #endregion

                #region Câu Gạch Chân - Underline question
                testGruop.Underline = new QuestionsOfType();

                // lấy câu hỏi gạch chân đơn - get single underline
                testGruop.Underline.ListOfSingles = new List<Models.ViewModels.Interface.ISingleQuestionViewModel>();
                for (int j = 0; j < testDataInJson["test"]["groups"][i]["underline"]["qSingle"].Count(); j++)
                {
                    SingleQuestionViewModel sqvm = new SingleQuestionViewModel();
                    sqvm.ID = testDataInJson["test"]["groups"][i]["underline"]["qSingle"][j]["sID"]?.ToString();
                    sqvm.Score = testDataInJson["test"]["groups"][i]["underline"]["qSingle"][j]["sScore"]?.ToString();
                    sqvm.Content = testDataInJson["test"]["groups"][i]["underline"]["qSingle"][j]["sContain"]?.ToString();
                    sqvm.QuestionType = QuestionTypeEnum.Underline;
                    sqvm.AnswerList = new List<Models.ViewModels.Interface.IAnswerViewModel>();
                    // lấy đáp án
                    for (int k = 0; k < testDataInJson["test"]["groups"][i]["underline"]["qSingle"][j]["sAList"].Count(); k++)
                    {
                        AnswerViewModel avm = new AnswerViewModel();
                        avm.Content = testDataInJson["test"]["groups"][i]["underline"]["qSingle"][j]["sAList"][k]["sAContain"]?.ToString();
                        avm.RightAnswer = testDataInJson["test"]["groups"][i]["underline"]["qSingle"][j]["sAList"][k]["sAw"]?.ToString();
                        sqvm.AnswerList.Add(avm);
                    }
                    testGruop.Underline.ListOfSingles.Add(sqvm);
                }

                // lấy câu hỏi gạch chân chùm - get group underline
                testGruop.Underline.ListOfGroups = new List<Models.ViewModels.Interface.IGroupQuestionViewModel>();
                for (int l = 0; l < testDataInJson["test"]["groups"][i]["underline"]["qGroup"].Count(); l++)
                {
                    GroupQuestionViewModel gqvm = new GroupQuestionViewModel();
                    gqvm.Content = testDataInJson["test"]["groups"][i]["underline"]["qGroup"][l]["qgTitle"]?.ToString();
                    gqvm.SingleQuestionList = new List<Models.ViewModels.Interface.ISingleQuestionViewModel>();
                    for (int j = 0; j < testDataInJson["test"]["groups"][i]["underline"]["qGroup"][l]["underlineSList"].Count(); j++)
                    {
                        SingleQuestionViewModel sqvm = new SingleQuestionViewModel();
                        sqvm.ID = testDataInJson["test"]["groups"][i]["underline"]["qGroup"][l]["underlineSList"][j]["sID"]?.ToString();
                        sqvm.Score = testDataInJson["test"]["groups"][i]["underline"]["qGroup"][l]["underlineSList"][j]["sScore"]?.ToString();
                        sqvm.Content = testDataInJson["test"]["groups"][i]["underline"]["qGroup"][l]["underlineSList"][j]["sContain"]?.ToString();
                        sqvm.QuestionType = QuestionTypeEnum.Underline;
                        sqvm.AnswerList = new List<Models.ViewModels.Interface.IAnswerViewModel>();
                        // lấy đáp án
                        for (int k = 0; k < testDataInJson["test"]["groups"][i]["underline"]["qGroup"][l]["underlineSList"][j]["sAList"].Count(); k++)
                        {
                            AnswerViewModel avm = new AnswerViewModel();
                            avm.Content = testDataInJson["test"]["groups"][i]["underline"]["qGroup"][l]["underlineSList"][j]["sAList"][k]["sAContain"]?.ToString();
                            avm.RightAnswer = testDataInJson["test"]["groups"][i]["underline"]["qGroup"][l]["underlineSList"][j]["sAList"][k]["sAw"]?.ToString();
                            sqvm.AnswerList.Add(avm);
                        }
                        gqvm.SingleQuestionList.Add(sqvm);
                    }
                    testGruop.Underline.ListOfGroups.Add(gqvm);
                }
                #endregion

                #region Câu Điền Khuyết - Fill Question
                testGruop.Fill = new QuestionsOfType();

                // lấy câu hỏi điền khuyết đơn - get single fill
                testGruop.Fill.ListOfSingles = new List<Models.ViewModels.Interface.ISingleQuestionViewModel>();
                for (int j = 0; j < testDataInJson["test"]["groups"][i]["fill"]["qSingle"].Count(); j++)
                {
                    SingleQuestionViewModel sqvm = new SingleQuestionViewModel();
                    sqvm.ID = testDataInJson["test"]["groups"][i]["fill"]["qSingle"][j]["sID"]?.ToString();
                    sqvm.Score = testDataInJson["test"]["groups"][i]["fill"]["qSingle"][j]["sScore"]?.ToString();
                    sqvm.Content = testDataInJson["test"]["groups"][i]["fill"]["qSingle"][j]["sContain"]?.ToString();
                    sqvm.QuestionType = QuestionTypeEnum.Fill;
                    sqvm.AnswerList = new List<Models.ViewModels.Interface.IAnswerViewModel>();
                    // lấy đáp án
                    for (int k = 0; k < testDataInJson["test"]["groups"][i]["fill"]["qSingle"][j]["sAList"].Count(); k++)
                    {
                        AnswerViewModel avm = new AnswerViewModel();
                        avm.Content = testDataInJson["test"]["groups"][i]["fill"]["qSingle"][j]["sAList"][k]["sAContain"]?.ToString();
                        avm.RightAnswer = testDataInJson["test"]["groups"][i]["fill"]["qSingle"][j]["sAList"][k]["sAw"]?.ToString();
                        sqvm.AnswerList.Add(avm);
                    }
                    testGruop.Fill.ListOfSingles.Add(sqvm);
                }

                // lấy câu hỏi điền khuyết chùm - get group fill
                testGruop.Fill.ListOfGroups = new List<Models.ViewModels.Interface.IGroupQuestionViewModel>();
                for (int l = 0; l < testDataInJson["test"]["groups"][i]["fill"]["qGroup"].Count(); l++)
                {
                    GroupQuestionViewModel gqvm = new GroupQuestionViewModel();
                    gqvm.Content = testDataInJson["test"]["groups"][i]["fill"]["qGroup"][l]["qgTitle"]?.ToString();
                    gqvm.SingleQuestionList = new List<Models.ViewModels.Interface.ISingleQuestionViewModel>();
                    for (int j = 0; j < testDataInJson["test"]["groups"][i]["fill"]["qGroup"][l]["fillSList"].Count(); j++)
                    {
                        SingleQuestionViewModel sqvm = new SingleQuestionViewModel();
                        sqvm.ID = testDataInJson["test"]["groups"][i]["fill"]["qGroup"][l]["fillSList"][j]["sID"]?.ToString();
                        sqvm.Score = testDataInJson["test"]["groups"][i]["fill"]["qGroup"][l]["fillSList"][j]["sScore"]?.ToString();
                        sqvm.Content = testDataInJson["test"]["groups"][i]["fill"]["qGroup"][l]["fillSList"][j]["sContain"]?.ToString();
                        sqvm.QuestionType = QuestionTypeEnum.Fill;
                        sqvm.AnswerList = new List<Models.ViewModels.Interface.IAnswerViewModel>();
                        // lấy đáp án
                        for (int k = 0; k < testDataInJson["test"]["groups"][i]["fill"]["qGroup"][l]["fillSList"][j]["sAList"].Count(); k++)
                        {
                            AnswerViewModel avm = new AnswerViewModel();
                            avm.Content = testDataInJson["test"]["groups"][i]["fill"]["qGroup"][l]["fillSList"][j]["sAList"][k]["sAContain"]?.ToString();
                            avm.RightAnswer = testDataInJson["test"]["groups"][i]["fill"]["qGroup"][l]["fillSList"][j]["sAList"][k]["sAw"]?.ToString();
                            sqvm.AnswerList.Add(avm);
                        }
                        gqvm.SingleQuestionList.Add(sqvm);
                    }
                    testGruop.Fill.ListOfGroups.Add(gqvm);
                }
                #endregion

                #region Câu Đúng Sai - True False Question
                testGruop.TrueFalse = new QuestionsOfType();

                // lấy câu hỏi đúng sai đơn - get single trueFalse
                testGruop.TrueFalse.ListOfSingles = new List<Models.ViewModels.Interface.ISingleQuestionViewModel>();
                for (int j = 0; j < testDataInJson["test"]["groups"][i]["trueFalse"]["qSingle"].Count(); j++)
                {
                    SingleQuestionViewModel sqvm = new SingleQuestionViewModel();
                    sqvm.ID = testDataInJson["test"]["groups"][i]["trueFalse"]["qSingle"][j]["sID"]?.ToString();
                    sqvm.Score = testDataInJson["test"]["groups"][i]["trueFalse"]["qSingle"][j]["sScore"]?.ToString();
                    sqvm.Content = testDataInJson["test"]["groups"][i]["trueFalse"]["qSingle"][j]["sContain"]?.ToString();
                    sqvm.QuestionType = QuestionTypeEnum.TrueFalse;
                    sqvm.AnswerList = new List<Models.ViewModels.Interface.IAnswerViewModel>();
                    // lấy đáp án
                    for (int k = 0; k < testDataInJson["test"]["groups"][i]["trueFalse"]["qSingle"][j]["sAList"].Count(); k++)
                    {
                        AnswerViewModel avm = new AnswerViewModel();
                        avm.Content = testDataInJson["test"]["groups"][i]["trueFalse"]["qSingle"][j]["sAList"][k]["sAContain"]?.ToString();
                        avm.RightAnswer = testDataInJson["test"]["groups"][i]["trueFalse"]["qSingle"][j]["sAList"][k]["sAw"]?.ToString();
                        sqvm.AnswerList.Add(avm);
                    }
                    testGruop.TrueFalse.ListOfSingles.Add(sqvm);
                }

                // lấy câu hỏi đúng sai chùm - get group trueFalse
                testGruop.TrueFalse.ListOfGroups = new List<Models.ViewModels.Interface.IGroupQuestionViewModel>();
                for (int l = 0; l < testDataInJson["test"]["groups"][i]["trueFalse"]["qGroup"].Count(); l++)
                {
                    GroupQuestionViewModel gqvm = new GroupQuestionViewModel();
                    gqvm.Content = testDataInJson["test"]["groups"][i]["trueFalse"]["qGroup"][l]["qgTitle"]?.ToString();
                    gqvm.SingleQuestionList = new List<Models.ViewModels.Interface.ISingleQuestionViewModel>();
                    for (int j = 0; j < testDataInJson["test"]["groups"][i]["trueFalse"]["qGroup"][l]["truefalseSList"].Count(); j++)
                    {
                        SingleQuestionViewModel sqvm = new SingleQuestionViewModel();
                        sqvm.ID = testDataInJson["test"]["groups"][i]["trueFalse"]["qGroup"][l]["truefalseSList"][j]["sID"]?.ToString();
                        sqvm.Score = testDataInJson["test"]["groups"][i]["trueFalse"]["qGroup"][l]["truefalseSList"][j]["sScore"]?.ToString();
                        sqvm.Content = testDataInJson["test"]["groups"][i]["trueFalse"]["qGroup"][l]["truefalseSList"][j]["sContain"]?.ToString();
                        sqvm.QuestionType = QuestionTypeEnum.TrueFalse;
                        sqvm.AnswerList = new List<Models.ViewModels.Interface.IAnswerViewModel>();
                        // lấy đáp án
                        for (int k = 0; k < testDataInJson["test"]["groups"][i]["trueFalse"]["qGroup"][l]["truefalseSList"][j]["sAList"].Count(); k++)
                        {
                            AnswerViewModel avm = new AnswerViewModel();
                            avm.Content = testDataInJson["test"]["groups"][i]["trueFalse"]["qGroup"][l]["truefalseSList"][j]["sAList"][k]["sAContain"]?.ToString();
                            avm.RightAnswer = testDataInJson["test"]["groups"][i]["trueFalse"]["qGroup"][l]["truefalseSList"][j]["sAList"][k]["sAw"]?.ToString();
                            sqvm.AnswerList.Add(avm);
                        }
                        gqvm.SingleQuestionList.Add(sqvm);
                    }
                    testGruop.TrueFalse.ListOfGroups.Add(gqvm);
                }
                #endregion

                #region Câu Nối Chéo - Matching Question
                testGruop.Matching = new QuestionsOfType();

                // lấy câu hỏi nối chéo đơn - get single matching
                testGruop.Matching.ListOfSingles = new List<Models.ViewModels.Interface.ISingleQuestionViewModel>();
                for (int j = 0; j < testDataInJson["test"]["groups"][i]["matching"]["qSingle"].Count(); j++)
                {
                    SingleQuestionViewModel sqvm = new SingleQuestionViewModel();
                    sqvm.ID = testDataInJson["test"]["groups"][i]["matching"]["qSingle"][j]["sID"]?.ToString();
                    sqvm.Score = testDataInJson["test"]["groups"][i]["matching"]["qSingle"][j]["sScore"]?.ToString();
                    sqvm.Content = testDataInJson["test"]["groups"][i]["matching"]["qSingle"][j]["sContain"]?.ToString();
                    sqvm.QuestionType = QuestionTypeEnum.Matching;
                    sqvm.AnswerList = new List<Models.ViewModels.Interface.IAnswerViewModel>();
                    // lấy đáp án
                    for (int k = 0; k < testDataInJson["test"]["groups"][i]["matching"]["qSingle"][j]["sAList"].Count(); k++)
                    {
                        AnswerViewModel avm = new AnswerViewModel();
                        avm.Content = testDataInJson["test"]["groups"][i]["matching"]["qSingle"][j]["sAList"][k]["sAContain"]?.ToString();
                        avm.RightAnswer = testDataInJson["test"]["groups"][i]["matching"]["qSingle"][j]["sAList"][k]["sAw"]?.ToString();
                        sqvm.AnswerList.Add(avm);
                    }
                    testGruop.Matching.ListOfSingles.Add(sqvm);
                }

                // lấy câu hỏi nối chéo chùm - get group matching
                testGruop.Matching.ListOfGroups = new List<Models.ViewModels.Interface.IGroupQuestionViewModel>();
                for (int l = 0; l < testDataInJson["test"]["groups"][i]["matching"]["qGroup"].Count(); l++)
                {
                    GroupQuestionViewModel gqvm = new GroupQuestionViewModel();
                    gqvm.Content = testDataInJson["test"]["groups"][i]["matching"]["qGroup"][l]["qgTitle"]?.ToString();
                    gqvm.SingleQuestionList = new List<Models.ViewModels.Interface.ISingleQuestionViewModel>();
                    for (int j = 0; j < testDataInJson["test"]["groups"][i]["matching"]["qGroup"][l]["matchingSList"].Count(); j++)
                    {
                        SingleQuestionViewModel sqvm = new SingleQuestionViewModel();
                        sqvm.ID = testDataInJson["test"]["groups"][i]["matching"]["qGroup"][l]["matchingSList"][j]["sID"]?.ToString();
                        sqvm.Score = testDataInJson["test"]["groups"][i]["matching"]["qGroup"][l]["matchingSList"][j]["sScore"]?.ToString();
                        sqvm.Content = testDataInJson["test"]["groups"][i]["matching"]["qGroup"][l]["matchingSList"][j]["sContain"]?.ToString();
                        sqvm.QuestionType = QuestionTypeEnum.Matching;
                        sqvm.AnswerList = new List<Models.ViewModels.Interface.IAnswerViewModel>();
                        // lấy đáp án
                        for (int k = 0; k < testDataInJson["test"]["groups"][i]["matching"]["qGroup"][l]["matchingSList"][j]["sAList"].Count(); k++)
                        {
                            AnswerViewModel avm = new AnswerViewModel();
                            avm.Content = testDataInJson["test"]["groups"][i]["matching"]["qGroup"][l]["matchingSList"][j]["sAList"][k]["sAContain"]?.ToString();
                            avm.RightAnswer = testDataInJson["test"]["groups"][i]["matching"]["qGroup"][l]["matchingSList"][j]["sAList"][k]["sAw"]?.ToString();
                            sqvm.AnswerList.Add(avm);
                        }
                        gqvm.SingleQuestionList.Add(sqvm);
                    }
                    testGruop.Matching.ListOfGroups.Add(gqvm);
                }
                #endregion
                testGroupList.Add(testGruop);
            }
            //testGroupList.Title


            #endregion



            var viewModel = new TestDataViewModel
            {
                SchoolName = testDataInJson["test"]["header"]["name"]?.ToString(),
                SchoolInfo = testDataInJson["test"]["header"]["info"]?.ToString(),
                Year = testDataInJson["test"]["header"]["year"]?.ToString(),
                TestType = testDataInJson["test"]["header"]["type"]?.ToString(),
                TestTime = testDataInJson["test"]["header"]["time"]?.ToString(),
                Subject = testDataInJson["test"]["header"]["subject"]?.ToString(),
                Grade = testDataInJson["test"]["header"]["grade"]?.ToString(),
                TestGroupList = testGroupList
            };

            return View(viewModel);
        }


        public ActionResult ChangeName()
        {
            //Nếu Chưa thi thì đổi tên được
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