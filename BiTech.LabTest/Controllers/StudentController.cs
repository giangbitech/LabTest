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
            ViewBag.StudentIPAdd = Request.ServerVariables["LOCAL_ADDR"].ToString();
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
            var testGroupList = new List<TestGroupViewModel>();
            // lấy từng nhóm câu hỏi ra
            int stt = 1;
            for (int i = 0; i < testDataInJson["test"]["groups"].Count(); i++)
            {
                TestGroupViewModel testGroup = new TestGroupViewModel();
                testGroup.isForAll = (i == 0);

                testGroup.Title = testDataInJson["test"]["groups"][i]["groupInfo"]["title"]?.ToString();

                #region Câu Trắc Nghiệm - Quiz Question
                testGroup.Quiz = new Question_String_OneChoice_Package();
                testGroup.QuizN = new Question_Bool_MultiChoices_Package();

                // lấy câu hỏi trắc nghiệm đơn - get single quiz
                testGroup.Quiz.ListOfSingles = new List<SingleQuestion_String_OneChoice_ViewModel>();
                testGroup.QuizN.ListOfSingles = new List<SingleQuestion_Bool_MultiChoices_ViewModel>();

                for (int j = 0; j < testDataInJson["test"]["groups"][i]["quiz"]["qSingle"].Count(); j++)
                {

                    List<PossibleAnswerViewModel> PossibleAnswerList = new List<PossibleAnswerViewModel>();
                    int rightChoiceCount = 0;
                    // lấy đáp án
                    int awStt = 1;
                    for (int k = 0; k < testDataInJson["test"]["groups"][i]["quiz"]["qSingle"][j]["sAList"].Count(); k++)
                    {
                        PossibleAnswerViewModel avm = new PossibleAnswerViewModel();
                        avm.STT = awStt.ToString();
                        avm.ID = awStt.ToString();
                        awStt++;
                        avm.Content = testDataInJson["test"]["groups"][i]["quiz"]["qSingle"][j]["sAList"][k]["sAContain"]?.ToString();
                        avm.RightAnswer = testDataInJson["test"]["groups"][i]["quiz"]["qSingle"][j]["sAList"][k]["sAw"]?.ToString();
                        rightChoiceCount += bool.Parse(avm.RightAnswer) == true ? 1 : 0;
                    }
                    if (rightChoiceCount == 1)
                    {
                        SingleQuestion_String_OneChoice_ViewModel sqvm = new SingleQuestion_String_OneChoice_ViewModel();
                        sqvm.STT = stt.ToString();
                        stt++;
                        sqvm.ID = testDataInJson["test"]["groups"][i]["quiz"]["qSingle"][j]["sID"]?.ToString();
                        sqvm.Score = testDataInJson["test"]["groups"][i]["quiz"]["qSingle"][j]["sScore"]?.ToString();
                        sqvm.Content = testDataInJson["test"]["groups"][i]["quiz"]["qSingle"][j]["sContain"]?.ToString();
                        sqvm.QuestionType = QuestionTypeEnum.Quiz;
                        sqvm.PossibleAnswerList = PossibleAnswerList;
                        testGroup.Quiz.ListOfSingles.Add(sqvm);
                    }
                    else
                    {
                        SingleQuestion_Bool_MultiChoices_ViewModel sqvm = new SingleQuestion_Bool_MultiChoices_ViewModel();
                        sqvm.STT = stt.ToString();
                        stt++;
                        sqvm.ID = testDataInJson["test"]["groups"][i]["quiz"]["qSingle"][j]["sID"]?.ToString();
                        sqvm.Score = testDataInJson["test"]["groups"][i]["quiz"]["qSingle"][j]["sScore"]?.ToString();
                        sqvm.Content = testDataInJson["test"]["groups"][i]["quiz"]["qSingle"][j]["sContain"]?.ToString();
                        sqvm.QuestionType = QuestionTypeEnum.Quiz;
                        sqvm.PossibleAnswerList = PossibleAnswerList;
                        testGroup.QuizN.ListOfSingles.Add(sqvm);
                    }
                }

                // lấy câu hỏi trắc nghiệm chùm - get group quiz
                testGroup.Quiz.ListOfGroups = new List<GroupQuestion_String_OneChoice_ViewModel>();
                testGroup.QuizN.ListOfGroups = new List<GroupQuestion_Bool_MultiChoices_ViewModel>();
                for (int l = 0; l < testDataInJson["test"]["groups"][i]["quiz"]["qGroup"].Count(); l++)
                {
                    GroupQuestion_String_OneChoice_ViewModel gqvm = new GroupQuestion_String_OneChoice_ViewModel();
                    gqvm.Content = testDataInJson["test"]["groups"][i]["quiz"]["qGroup"][l]["qgTitle"]?.ToString();
                    gqvm.SingleQuestionList = new List<SingleQuestionViewModel>();
                    for (int j = 0; j < testDataInJson["test"]["groups"][i]["quiz"]["qGroup"][l]["quizSList"].Count(); j++)
                    {
                        SingleQuestionViewModel sqvm = new SingleQuestionViewModel();
                        sqvm.STT = stt.ToString();
                        stt++;
                        sqvm.ID = testDataInJson["test"]["groups"][i]["quiz"]["qGroup"][l]["quizSList"][j]["sID"]?.ToString();
                        sqvm.Score = testDataInJson["test"]["groups"][i]["quiz"]["qGroup"][l]["quizSList"][j]["sScore"]?.ToString();
                        sqvm.Content = testDataInJson["test"]["groups"][i]["quiz"]["qGroup"][l]["quizSList"][j]["sContain"]?.ToString();
                        sqvm.QuestionType = QuestionTypeEnum.Quiz;
                        sqvm.PossibleAnswerList = new List<PossibleAnswerViewModel>();
                        // lấy đáp án
                        int awStt = 1;
                        for (int k = 0; k < testDataInJson["test"]["groups"][i]["quiz"]["qGroup"][l]["quizSList"][j]["sAList"].Count(); k++)
                        {
                            PossibleAnswerViewModel avm = new PossibleAnswerViewModel();
                            avm.STT = awStt.ToString();
                            avm.ID = awStt.ToString();
                            awStt++;
                            avm.Content = testDataInJson["test"]["groups"][i]["quiz"]["qGroup"][l]["quizSList"][j]["sAList"][k]["sAContain"]?.ToString();
                            avm.RightAnswer = testDataInJson["test"]["groups"][i]["quiz"]["qGroup"][l]["quizSList"][j]["sAList"][k]["sAw"]?.ToString();
                            sqvm.PossibleAnswerList.Add(avm);
                        }
                        gqvm.SingleQuestionList.Add(sqvm);
                    }
                    testGroup.Quiz.ListOfGroups.Add(gqvm);
                }
                #endregion

                //#region Câu Gạch Chân - Underline question
                //testGroup.Underline = new QuestionPackage();

                //// lấy câu hỏi gạch chân đơn - get single underline
                //testGroup.Underline.ListOfSingles = new List<SingleQuestionViewModel>();
                //for (int j = 0; j < testDataInJson["test"]["groups"][i]["underline"]["qSingle"].Count(); j++)
                //{
                //    SingleQuestionViewModel sqvm = new SingleQuestionViewModel();
                //    sqvm.STT = stt.ToString();
                //    stt++;
                //    sqvm.ID = testDataInJson["test"]["groups"][i]["underline"]["qSingle"][j]["sID"]?.ToString();
                //    sqvm.Score = testDataInJson["test"]["groups"][i]["underline"]["qSingle"][j]["sScore"]?.ToString();
                //    sqvm.Content = testDataInJson["test"]["groups"][i]["underline"]["qSingle"][j]["sContain"]?.ToString();
                //    sqvm.QuestionType = QuestionTypeEnum.Underline;
                //    sqvm.PossibleAnswerList = new List<PossibleAnswerViewModel>();
                //    // lấy đáp án
                //    int awStt = 1;
                //    for (int k = 0; k < testDataInJson["test"]["groups"][i]["underline"]["qSingle"][j]["sAList"].Count(); k++)
                //    {
                //        PossibleAnswerViewModel avm = new PossibleAnswerViewModel();
                //        avm.STT = awStt.ToString();
                //        avm.ID = awStt.ToString();
                //        awStt++;
                //        avm.Content = testDataInJson["test"]["groups"][i]["underline"]["qSingle"][j]["sAList"][k]["sAContain"]?.ToString();
                //        avm.RightAnswer = testDataInJson["test"]["groups"][i]["underline"]["qSingle"][j]["sAList"][k]["sAw"]?.ToString();
                //        sqvm.PossibleAnswerList.Add(avm);
                //    }
                //    testGroup.Underline.ListOfSingles.Add(sqvm);
                //}

                //// lấy câu hỏi gạch chân chùm - get group underline
                //testGroup.Underline.ListOfGroups = new List<GroupQuestionViewModel>();
                //for (int l = 0; l < testDataInJson["test"]["groups"][i]["underline"]["qGroup"].Count(); l++)
                //{
                //    GroupQuestionViewModel gqvm = new GroupQuestionViewModel();
                //    gqvm.Content = testDataInJson["test"]["groups"][i]["underline"]["qGroup"][l]["qgTitle"]?.ToString();
                //    gqvm.SingleQuestionList = new List<SingleQuestionViewModel>();
                //    for (int j = 0; j < testDataInJson["test"]["groups"][i]["underline"]["qGroup"][l]["underlineSList"].Count(); j++)
                //    {
                //        SingleQuestionViewModel sqvm = new SingleQuestionViewModel();
                //        sqvm.STT = stt.ToString();
                //        stt++;
                //        sqvm.ID = testDataInJson["test"]["groups"][i]["underline"]["qGroup"][l]["underlineSList"][j]["sID"]?.ToString();
                //        sqvm.Score = testDataInJson["test"]["groups"][i]["underline"]["qGroup"][l]["underlineSList"][j]["sScore"]?.ToString();
                //        sqvm.Content = testDataInJson["test"]["groups"][i]["underline"]["qGroup"][l]["underlineSList"][j]["sContain"]?.ToString();
                //        sqvm.QuestionType = QuestionTypeEnum.Underline;
                //        sqvm.PossibleAnswerList = new List<PossibleAnswerViewModel>();
                //        // lấy đáp án
                //        int awStt = 1;
                //        for (int k = 0; k < testDataInJson["test"]["groups"][i]["underline"]["qGroup"][l]["underlineSList"][j]["sAList"].Count(); k++)
                //        {
                //            PossibleAnswerViewModel avm = new PossibleAnswerViewModel();
                //            avm.STT = awStt.ToString();
                //            avm.ID = awStt.ToString();
                //            awStt++;
                //            avm.Content = testDataInJson["test"]["groups"][i]["underline"]["qGroup"][l]["underlineSList"][j]["sAList"][k]["sAContain"]?.ToString();
                //            avm.RightAnswer = testDataInJson["test"]["groups"][i]["underline"]["qGroup"][l]["underlineSList"][j]["sAList"][k]["sAw"]?.ToString();
                //            sqvm.PossibleAnswerList.Add(avm);
                //        }
                //        gqvm.SingleQuestionList.Add(sqvm);
                //    }
                //    testGroup.Underline.ListOfGroups.Add(gqvm);
                //}
                //#endregion

                //#region Câu Điền Khuyết - Fill Question
                //testGroup.Fill = new QuestionPackage();

                //// lấy câu hỏi điền khuyết đơn - get single fill
                //testGroup.Fill.ListOfSingles = new List<SingleQuestionViewModel>();
                //for (int j = 0; j < testDataInJson["test"]["groups"][i]["fill"]["qSingle"].Count(); j++)
                //{
                //    SingleQuestionViewModel sqvm = new SingleQuestionViewModel();
                //    sqvm.STT = stt.ToString();
                //    stt++;
                //    sqvm.ID = testDataInJson["test"]["groups"][i]["fill"]["qSingle"][j]["sID"]?.ToString();
                //    sqvm.Score = testDataInJson["test"]["groups"][i]["fill"]["qSingle"][j]["sScore"]?.ToString();
                //    sqvm.Content = testDataInJson["test"]["groups"][i]["fill"]["qSingle"][j]["sContain"]?.ToString();
                //    sqvm.QuestionType = QuestionTypeEnum.Fill;
                //    sqvm.PossibleAnswerList = new List<PossibleAnswerViewModel>();
                //    // lấy đáp án
                //    int awStt = 1;
                //    for (int k = 0; k < testDataInJson["test"]["groups"][i]["fill"]["qSingle"][j]["sAList"].Count(); k++)
                //    {
                //        PossibleAnswerViewModel avm = new PossibleAnswerViewModel();
                //        avm.STT = awStt.ToString();
                //        avm.ID = awStt.ToString();
                //        awStt++;
                //        avm.Content = testDataInJson["test"]["groups"][i]["fill"]["qSingle"][j]["sAList"][k]["sAContain"]?.ToString();
                //        avm.RightAnswer = testDataInJson["test"]["groups"][i]["fill"]["qSingle"][j]["sAList"][k]["sAw"]?.ToString();
                //        sqvm.PossibleAnswerList.Add(avm);
                //    }
                //    testGroup.Fill.ListOfSingles.Add(sqvm);
                //}

                //// lấy câu hỏi điền khuyết chùm - get group fill
                //testGroup.Fill.ListOfGroups = new List<GroupQuestionViewModel>();
                //for (int l = 0; l < testDataInJson["test"]["groups"][i]["fill"]["qGroup"].Count(); l++)
                //{
                //    GroupQuestionViewModel gqvm = new GroupQuestionViewModel();
                //    gqvm.Content = testDataInJson["test"]["groups"][i]["fill"]["qGroup"][l]["qgTitle"]?.ToString();
                //    gqvm.SingleQuestionList = new List<SingleQuestionViewModel>();
                //    for (int j = 0; j < testDataInJson["test"]["groups"][i]["fill"]["qGroup"][l]["fillSList"].Count(); j++)
                //    {
                //        SingleQuestionViewModel sqvm = new SingleQuestionViewModel();
                //        sqvm.STT = stt.ToString();
                //        stt++;
                //        sqvm.ID = testDataInJson["test"]["groups"][i]["fill"]["qGroup"][l]["fillSList"][j]["sID"]?.ToString();
                //        sqvm.Score = testDataInJson["test"]["groups"][i]["fill"]["qGroup"][l]["fillSList"][j]["sScore"]?.ToString();
                //        sqvm.Content = testDataInJson["test"]["groups"][i]["fill"]["qGroup"][l]["fillSList"][j]["sContain"]?.ToString();
                //        sqvm.QuestionType = QuestionTypeEnum.Fill;
                //        sqvm.PossibleAnswerList = new List<PossibleAnswerViewModel>();
                //        // lấy đáp án
                //        int awStt = 1;
                //        for (int k = 0; k < testDataInJson["test"]["groups"][i]["fill"]["qGroup"][l]["fillSList"][j]["sAList"].Count(); k++)
                //        {
                //            PossibleAnswerViewModel avm = new PossibleAnswerViewModel();
                //            avm.STT = awStt.ToString();
                //            avm.ID = awStt.ToString();
                //            awStt++;
                //            avm.Content = testDataInJson["test"]["groups"][i]["fill"]["qGroup"][l]["fillSList"][j]["sAList"][k]["sAContain"]?.ToString();
                //            avm.RightAnswer = testDataInJson["test"]["groups"][i]["fill"]["qGroup"][l]["fillSList"][j]["sAList"][k]["sAw"]?.ToString();
                //            sqvm.PossibleAnswerList.Add(avm);
                //        }
                //        gqvm.SingleQuestionList.Add(sqvm);
                //    }
                //    testGroup.Fill.ListOfGroups.Add(gqvm);
                //}
                //#endregion

                //#region Câu Đúng Sai - True False Question
                //testGroup.TrueFalse = new QuestionPackage();

                //// lấy câu hỏi đúng sai đơn - get single trueFalse
                //testGroup.TrueFalse.ListOfSingles = new List<SingleQuestionViewModel>();
                //for (int j = 0; j < testDataInJson["test"]["groups"][i]["trueFalse"]["qSingle"].Count(); j++)
                //{
                //    SingleQuestionViewModel sqvm = new SingleQuestionViewModel();
                //    sqvm.STT = stt.ToString();
                //    stt++;
                //    sqvm.ID = testDataInJson["test"]["groups"][i]["trueFalse"]["qSingle"][j]["sID"]?.ToString();
                //    sqvm.Score = testDataInJson["test"]["groups"][i]["trueFalse"]["qSingle"][j]["sScore"]?.ToString();
                //    sqvm.Content = testDataInJson["test"]["groups"][i]["trueFalse"]["qSingle"][j]["sContain"]?.ToString();
                //    sqvm.QuestionType = QuestionTypeEnum.TrueFalse;
                //    sqvm.PossibleAnswerList = new List<PossibleAnswerViewModel>();
                //    // lấy đáp án
                //    int awStt = 1;
                //    for (int k = 0; k < testDataInJson["test"]["groups"][i]["trueFalse"]["qSingle"][j]["sAList"].Count(); k++)
                //    {
                //        PossibleAnswerViewModel avm = new PossibleAnswerViewModel();
                //        avm.STT = awStt.ToString();
                //        avm.ID = awStt.ToString();
                //        awStt++;
                //        avm.Content = testDataInJson["test"]["groups"][i]["trueFalse"]["qSingle"][j]["sAList"][k]["sAContain"]?.ToString();
                //        avm.RightAnswer = testDataInJson["test"]["groups"][i]["trueFalse"]["qSingle"][j]["sAList"][k]["sAw"]?.ToString();
                //        sqvm.PossibleAnswerList.Add(avm);
                //    }
                //    testGroup.TrueFalse.ListOfSingles.Add(sqvm);
                //}

                //// lấy câu hỏi đúng sai chùm - get group trueFalse
                //testGroup.TrueFalse.ListOfGroups = new List<GroupQuestionViewModel>();
                //for (int l = 0; l < testDataInJson["test"]["groups"][i]["trueFalse"]["qGroup"].Count(); l++)
                //{
                //    GroupQuestionViewModel gqvm = new GroupQuestionViewModel();
                //    gqvm.Content = testDataInJson["test"]["groups"][i]["trueFalse"]["qGroup"][l]["qgTitle"]?.ToString();
                //    gqvm.SingleQuestionList = new List<SingleQuestionViewModel>();
                //    for (int j = 0; j < testDataInJson["test"]["groups"][i]["trueFalse"]["qGroup"][l]["truefalseSList"].Count(); j++)
                //    {
                //        SingleQuestionViewModel sqvm = new SingleQuestionViewModel();
                //        sqvm.STT = stt.ToString();
                //        stt++;
                //        sqvm.ID = testDataInJson["test"]["groups"][i]["trueFalse"]["qGroup"][l]["truefalseSList"][j]["sID"]?.ToString();
                //        sqvm.Score = testDataInJson["test"]["groups"][i]["trueFalse"]["qGroup"][l]["truefalseSList"][j]["sScore"]?.ToString();
                //        sqvm.Content = testDataInJson["test"]["groups"][i]["trueFalse"]["qGroup"][l]["truefalseSList"][j]["sContain"]?.ToString();
                //        sqvm.QuestionType = QuestionTypeEnum.TrueFalse;
                //        sqvm.PossibleAnswerList = new List<PossibleAnswerViewModel>();
                //        // lấy đáp án
                //        int awStt = 1;
                //        for (int k = 0; k < testDataInJson["test"]["groups"][i]["trueFalse"]["qGroup"][l]["truefalseSList"][j]["sAList"].Count(); k++)
                //        {
                //            PossibleAnswerViewModel avm = new PossibleAnswerViewModel();
                //            avm.STT = awStt.ToString();
                //            avm.ID = awStt.ToString();
                //            awStt++;
                //            avm.Content = testDataInJson["test"]["groups"][i]["trueFalse"]["qGroup"][l]["truefalseSList"][j]["sAList"][k]["sAContain"]?.ToString();
                //            avm.RightAnswer = testDataInJson["test"]["groups"][i]["trueFalse"]["qGroup"][l]["truefalseSList"][j]["sAList"][k]["sAw"]?.ToString();
                //            sqvm.PossibleAnswerList.Add(avm);
                //        }
                //        gqvm.SingleQuestionList.Add(sqvm);
                //    }
                //    testGroup.TrueFalse.ListOfGroups.Add(gqvm);
                //}
                //#endregion

                //#region Câu Nối Chéo - Matching Question
                //testGroup.Matching = new QuestionPackage();

                //// lấy câu hỏi nối chéo đơn - get single matching
                //testGroup.Matching.ListOfSingles = new List<SingleQuestionViewModel>();
                //for (int j = 0; j < testDataInJson["test"]["groups"][i]["matching"]["qSingle"].Count(); j++)
                //{
                //    SingleQuestionViewModel sqvm = new SingleQuestionViewModel();
                //    sqvm.STT = stt.ToString();
                //    stt++;
                //    sqvm.ID = testDataInJson["test"]["groups"][i]["matching"]["qSingle"][j]["sID"]?.ToString();
                //    sqvm.Score = testDataInJson["test"]["groups"][i]["matching"]["qSingle"][j]["sScore"]?.ToString();
                //    sqvm.Content = testDataInJson["test"]["groups"][i]["matching"]["qSingle"][j]["sContain"]?.ToString();
                //    sqvm.QuestionType = QuestionTypeEnum.Matching;
                //    sqvm.PossibleAnswerList = new List<PossibleAnswerViewModel>();
                //    // lấy đáp án
                //    int awStt = 1;
                //    for (int k = 0; k < testDataInJson["test"]["groups"][i]["matching"]["qSingle"][j]["sAList"].Count(); k++)
                //    {
                //        PossibleAnswerViewModel avm = new PossibleAnswerViewModel();
                //        avm.STT = awStt.ToString();
                //        avm.ID = awStt.ToString();
                //        awStt++;
                //        avm.Content = testDataInJson["test"]["groups"][i]["matching"]["qSingle"][j]["sAList"][k]["sAContain"]?.ToString();
                //        avm.RightAnswer = testDataInJson["test"]["groups"][i]["matching"]["qSingle"][j]["sAList"][k]["sAw"]?.ToString();
                //        sqvm.PossibleAnswerList.Add(avm);
                //    }
                //    testGroup.Matching.ListOfSingles.Add(sqvm);
                //}

                //// lấy câu hỏi nối chéo chùm - get group matching
                //testGroup.Matching.ListOfGroups = new List<GroupQuestionViewModel>();
                //for (int l = 0; l < testDataInJson["test"]["groups"][i]["matching"]["qGroup"].Count(); l++)
                //{
                //    GroupQuestionViewModel gqvm = new GroupQuestionViewModel();
                //    gqvm.Content = testDataInJson["test"]["groups"][i]["matching"]["qGroup"][l]["qgTitle"]?.ToString();
                //    gqvm.SingleQuestionList = new List<SingleQuestionViewModel>();
                //    for (int j = 0; j < testDataInJson["test"]["groups"][i]["matching"]["qGroup"][l]["matchingSList"].Count(); j++)
                //    {
                //        SingleQuestionViewModel sqvm = new SingleQuestionViewModel();
                //        sqvm.STT = stt.ToString();
                //        stt++;
                //        sqvm.ID = testDataInJson["test"]["groups"][i]["matching"]["qGroup"][l]["matchingSList"][j]["sID"]?.ToString();
                //        sqvm.Score = testDataInJson["test"]["groups"][i]["matching"]["qGroup"][l]["matchingSList"][j]["sScore"]?.ToString();
                //        sqvm.Content = testDataInJson["test"]["groups"][i]["matching"]["qGroup"][l]["matchingSList"][j]["sContain"]?.ToString();
                //        sqvm.QuestionType = QuestionTypeEnum.Matching;
                //        sqvm.PossibleAnswerList = new List<PossibleAnswerViewModel>();
                //        // lấy đáp án
                //        int awStt = 1;
                //        for (int k = 0; k < testDataInJson["test"]["groups"][i]["matching"]["qGroup"][l]["matchingSList"][j]["sAList"].Count(); k++)
                //        {
                //            PossibleAnswerViewModel avm = new PossibleAnswerViewModel();
                //            avm.STT = awStt.ToString();
                //            avm.ID = awStt.ToString();
                //            awStt++;
                //            avm.Content = testDataInJson["test"]["groups"][i]["matching"]["qGroup"][l]["matchingSList"][j]["sAList"][k]["sAContain"]?.ToString();
                //            avm.RightAnswer = testDataInJson["test"]["groups"][i]["matching"]["qGroup"][l]["matchingSList"][j]["sAList"][k]["sAw"]?.ToString();
                //            sqvm.PossibleAnswerList.Add(avm);
                //        }
                //        gqvm.SingleQuestionList.Add(sqvm);
                //    }
                //    testGroup.Matching.ListOfGroups.Add(gqvm);
                //}
                //#endregion


                testGroupList.Add(testGroup);
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

            #region Test result
            //TestResultViewModel testResult = new TestResultViewModel();
            //testResult.GroupList = new List<TestGroupResultViewModel>();
            //testResult.StudentName = Session["FullName"]?.ToString() ?? "";
            //testResult.StudentClass = Session["Class"]?.ToString() ?? "";
            //testResult.StudentIPAdd = Request.ServerVariables["LOCAL_ADDR"].ToString();
            //foreach (TestGroupViewModel tg in testData.TestGroupList)
            //{
            //    stt = 0;
            //    TestGroupResultViewModel gr = new TestGroupResultViewModel();
            //    gr.QuestionList = new List<QuestionResultViewModel>();

            //    //Quiz
            //    for(int xx = 0; xx < tg.Quiz.ListOfSingles.Count; xx++)
            //    {
            //        QuestionResultViewModel qr = new QuestionResultViewModel();
            //        qr.Answer = new List<AnswerResultViewModel>();
            //        qr.STT = (stt++).ToString();
            //        foreach(var zz in tg.Quiz.ListOfSingles[xx].PossibleAnswerList)
            //        {
            //            qr.Answer.Add(new AnswerResultViewModel());
            //        }
            //        gr.QuestionList.Add(qr);
            //    }
            //    for (int xx = 0; xx < tg.Quiz.ListOfGroups.Count; xx++)
            //    {
            //        for (int yy = 0; yy < tg.Quiz.ListOfGroups[xx].SingleQuestionList.Count; yy++)
            //        {
            //            QuestionResultViewModel qr = new QuestionResultViewModel();
            //            qr.Answer = new List<AnswerResultViewModel>();
            //            qr.STT = (stt++).ToString();
            //            foreach (var zz in tg.Quiz.ListOfGroups[xx].SingleQuestionList[yy].PossibleAnswerList)
            //            {
            //                qr.Answer.Add(new AnswerResultViewModel());
            //            }
            //            gr.QuestionList.Add(qr);
            //        }
            //    }

            //    //Underline
            //    for (int xx = 0; xx < tg.Underline.ListOfSingles.Count; xx++)
            //    {
            //        QuestionResultViewModel qr = new QuestionResultViewModel();
            //        qr.Answer = new List<AnswerResultViewModel>();
            //        qr.STT = (stt++).ToString();
            //        foreach (var zz in tg.Underline.ListOfSingles[xx].PossibleAnswerList)
            //        {
            //            qr.Answer.Add(new AnswerResultViewModel());
            //        }
            //        gr.QuestionList.Add(qr);
            //    }
            //    for (int xx = 0; xx < tg.Underline.ListOfGroups.Count; xx++)
            //    {
            //        for (int yy = 0; yy < tg.Underline.ListOfGroups[xx].SingleQuestionList.Count; yy++)
            //        {
            //            QuestionResultViewModel qr = new QuestionResultViewModel();
            //            qr.Answer = new List<AnswerResultViewModel>();
            //            qr.STT = (stt++).ToString();
            //            foreach (var zz in tg.Underline.ListOfGroups[xx].SingleQuestionList[yy].PossibleAnswerList)
            //            {
            //                qr.Answer.Add(new AnswerResultViewModel());
            //            }
            //            gr.QuestionList.Add(qr);
            //        }
            //    }

            //    //Fill
            //    for (int xx = 0; xx < tg.Fill.ListOfSingles.Count; xx++)
            //    {
            //        QuestionResultViewModel qr = new QuestionResultViewModel();
            //        qr.Answer = new List<AnswerResultViewModel>();
            //        qr.STT = (stt++).ToString();
            //        foreach (var zz in tg.Fill.ListOfSingles[xx].PossibleAnswerList)
            //        {
            //            qr.Answer.Add(new AnswerResultViewModel());
            //        }
            //        gr.QuestionList.Add(qr);
            //    }
            //    for (int xx = 0; xx < tg.Fill.ListOfGroups.Count; xx++)
            //    {
            //        for (int yy = 0; yy < tg.Fill.ListOfGroups[xx].SingleQuestionList.Count; yy++)
            //        {
            //            QuestionResultViewModel qr = new QuestionResultViewModel();
            //            qr.Answer = new List<AnswerResultViewModel>();
            //            qr.STT = (stt++).ToString();
            //            foreach (var zz in tg.Fill.ListOfGroups[xx].SingleQuestionList[yy].PossibleAnswerList)
            //            {
            //                qr.Answer.Add(new AnswerResultViewModel());
            //            }
            //            gr.QuestionList.Add(qr);
            //        }
            //    }

            //    //True False
            //    for (int xx = 0; xx < tg.TrueFalse.ListOfSingles.Count; xx++)
            //    {
            //        QuestionResultViewModel qr = new QuestionResultViewModel();
            //        qr.Answer = new List<AnswerResultViewModel>();
            //        qr.STT = (stt++).ToString();
            //        foreach (var zz in tg.TrueFalse.ListOfSingles[xx].PossibleAnswerList)
            //        {
            //            qr.Answer.Add(new AnswerResultViewModel());
            //        }
            //        gr.QuestionList.Add(qr);
            //    }
            //    for (int xx = 0; xx < tg.TrueFalse.ListOfGroups.Count; xx++)
            //    {
            //        for (int yy = 0; yy < tg.TrueFalse.ListOfGroups[xx].SingleQuestionList.Count; yy++)
            //        {
            //            QuestionResultViewModel qr = new QuestionResultViewModel();
            //            qr.Answer = new List<AnswerResultViewModel>();
            //            qr.STT = (stt++).ToString();
            //            foreach (var zz in tg.TrueFalse.ListOfGroups[xx].SingleQuestionList[yy].PossibleAnswerList)
            //            {
            //                qr.Answer.Add(new AnswerResultViewModel());
            //            }
            //            gr.QuestionList.Add(qr);
            //        }
            //    }

            //    //Matching
            //    for (int xx = 0; xx < tg.Matching.ListOfSingles.Count; xx++)
            //    {
            //        QuestionResultViewModel qr = new QuestionResultViewModel();
            //        qr.Answer = new List<AnswerResultViewModel>();
            //        qr.STT = (stt++).ToString();
            //        foreach (var zz in tg.Matching.ListOfSingles[xx].PossibleAnswerList)
            //        {
            //            qr.Answer.Add(new AnswerResultViewModel());
            //        }
            //        gr.QuestionList.Add(qr);
            //    }
            //    for (int xx = 0; xx < tg.Matching.ListOfGroups.Count; xx++)
            //    {
            //        for (int yy = 0; yy < tg.Matching.ListOfGroups[xx].SingleQuestionList.Count; yy++)
            //        {
            //            QuestionResultViewModel qr = new QuestionResultViewModel();
            //            qr.Answer = new List<AnswerResultViewModel>();
            //            qr.STT = (stt++).ToString();
            //            foreach (var zz in tg.Matching.ListOfGroups[xx].SingleQuestionList[yy].PossibleAnswerList)
            //            {
            //                qr.Answer.Add(new AnswerResultViewModel());
            //            }
            //            gr.QuestionList.Add(qr);
            //        }
            //    }
            //    testResult.GroupList.Add(gr);
            //}

            #endregion

            //TestViewModel viewModel = new TestViewModel();
            //viewModel.TestData = testData;
            //viewModel.TestResult = testResult;
            return View(viewModel);
        }

        public ActionResult DoneTest(TestViewModel test)
        {
            
            return null;
        }

        public ActionResult DoneTestx(TestDataViewModel test)
        {
            string stt = test.StudentName;
            string id = test.StudentClass;
            return null;
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