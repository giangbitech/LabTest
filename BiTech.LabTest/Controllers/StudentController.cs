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
            string IP = Request.UserHostAddress;
            //string dethiodangjson = "";
            //ViewBag.Data = Json(new { group = "gg", ddd = "dd" });

            //var dbTestData = _StudentLogic.GetTest(testDataId);
            //var testDataInJson = JObject.Parse(dbTestData.Data);

            //todo: Chinh lai Lay tu database
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
                testGroup.QuestionsList = new List<object>();

                #region Câu Trắc Nghiệm - Quiz Question

                //testGroup.Quiz = new Question_String_OneChoice_Package();
                //testGroup.QuizN = new Question_Bool_MultiChoices_Package();
                //testGroup.Quiz.ListOfSingles = new List<SingleQuestion_String_OneChoice_ViewModel>();
                //testGroup.QuizN.ListOfSingles = new List<SingleQuestion_Bool_MultiChoices_ViewModel>();

                // lấy câu hỏi trắc nghiệm đơn - get single quiz
                for (int j = 0; j < testDataInJson["test"]["groups"][i]["quiz"]["qSingle"].Count(); j++)
                {
                    List<PossibleAnswerViewModel> possibleAnswerList = new List<PossibleAnswerViewModel>();
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
                        possibleAnswerList.Add(avm);
                    }
                    awStt = 1;
                    ShuffleList(possibleAnswerList);
                    foreach (var answer in possibleAnswerList)
                    {
                        answer.STT = awStt.ToString();
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
                        sqvm.SelectedAnswer = "";
                        sqvm.PossibleAnswerList = possibleAnswerList;
                        testGroup.QuestionsList.Add(sqvm);
                    }
                    else
                    {
                        SingleQuestion_Bool_MultiChoices_ViewModel sqvm = new SingleQuestion_Bool_MultiChoices_ViewModel();
                        sqvm.STT = stt.ToString();
                        stt++;
                        sqvm.ID = testDataInJson["test"]["groups"][i]["quiz"]["qSingle"][j]["sID"]?.ToString();
                        sqvm.Score = testDataInJson["test"]["groups"][i]["quiz"]["qSingle"][j]["sScore"]?.ToString();
                        sqvm.Content = testDataInJson["test"]["groups"][i]["quiz"]["qSingle"][j]["sContain"]?.ToString();
                        sqvm.QuestionType = QuestionTypeEnum.QuizN;
                        sqvm.SelectedAnswer = new List<SingleQuestion_Bool_MultiChoices_ViewModel.AnswerPacket>();
                        sqvm.PossibleAnswerList = possibleAnswerList;
                        testGroup.QuestionsList.Add(sqvm);
                    }
                }

                // lấy câu hỏi trắc nghiệm chùm - get group quiz
                //testGroup.Quiz.ListOfGroups = new List<GroupQuestion_String_OneChoice_ViewModel>();
                //testGroup.QuizN.ListOfGroups = new List<GroupQuestion_Bool_MultiChoices_ViewModel>();
                for (int l = 0; l < testDataInJson["test"]["groups"][i]["quiz"]["qGroup"].Count(); l++)
                {
                    GroupQuestionViewModel gqvm = new GroupQuestionViewModel();
                    gqvm.Content = testDataInJson["test"]["groups"][i]["quiz"]["qGroup"][l]["qgTitle"]?.ToString();
                    gqvm.SingleQuestionsList = new List<object>();
                    gqvm.QuestionType = QuestionTypeEnum.Group;

                    for (int j = 0; j < testDataInJson["test"]["groups"][i]["quiz"]["qGroup"][l]["quizSList"].Count(); j++)
                    {
                        //SingleQuestionViewModel sqvm = new SingleQuestionViewModel();
                        //sqvm.STT = stt.ToString();
                        //stt++;
                        //sqvm.QuestionType = QuestionTypeEnum.Quiz;
                        //sqvm.PossibleAnswerList = new List<PossibleAnswerViewModel>();

                        List<PossibleAnswerViewModel> possibleAnswerList = new List<PossibleAnswerViewModel>();
                        int rightChoiceCount = 0;
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
                            rightChoiceCount += bool.Parse(avm.RightAnswer) == true ? 1 : 0;
                            possibleAnswerList.Add(avm);
                        }
                        awStt = 1;
                        ShuffleList(possibleAnswerList);
                        foreach (var answer in possibleAnswerList)
                        {
                            answer.STT = awStt.ToString();
                        }
                        if (rightChoiceCount == 1)
                        {
                            SingleQuestion_String_OneChoice_ViewModel sqvm = new SingleQuestion_String_OneChoice_ViewModel();
                            sqvm.STT = stt.ToString();
                            stt++;
                            sqvm.ID = testDataInJson["test"]["groups"][i]["quiz"]["qGroup"][l]["quizSList"][j]["sID"]?.ToString();
                            sqvm.Score = testDataInJson["test"]["groups"][i]["quiz"]["qGroup"][l]["quizSList"][j]["sScore"]?.ToString();
                            sqvm.Content = testDataInJson["test"]["groups"][i]["quiz"]["qGroup"][l]["quizSList"][j]["sContain"]?.ToString();
                            sqvm.QuestionType = QuestionTypeEnum.Quiz;
                            sqvm.SelectedAnswer = "";
                            sqvm.PossibleAnswerList = possibleAnswerList;

                            gqvm.SingleQuestionsList.Add(sqvm);
                        }
                        else
                        {
                            SingleQuestion_Bool_MultiChoices_ViewModel sqvm = new SingleQuestion_Bool_MultiChoices_ViewModel();
                            sqvm.STT = stt.ToString();
                            stt++;
                            sqvm.ID = testDataInJson["test"]["groups"][i]["quiz"]["qGroup"][l]["quizSList"][j]["sID"]?.ToString();
                            sqvm.Score = testDataInJson["test"]["groups"][i]["quiz"]["qGroup"][l]["quizSList"][j]["sScore"]?.ToString();
                            sqvm.Content = testDataInJson["test"]["groups"][i]["quiz"]["qGroup"][l]["quizSList"][j]["sContain"]?.ToString();
                            sqvm.QuestionType = QuestionTypeEnum.QuizN;
                            sqvm.SelectedAnswer = new List<SingleQuestion_Bool_MultiChoices_ViewModel.AnswerPacket>();
                            sqvm.PossibleAnswerList = possibleAnswerList;

                            gqvm.SingleQuestionsList.Add(sqvm);
                        }
                    }
                    testGroup.QuestionsList.Add(gqvm);
                }
                #endregion

                #region Câu Gạch Chân - Underline question
                //testGroup.Underline = new QuestionPackage();

                //// lấy câu hỏi gạch chân đơn - get single underline
                //testGroup.Underline.ListOfSingles = new List<SingleQuestionViewModel>();
                for (int j = 0; j < testDataInJson["test"]["groups"][i]["underline"]["qSingle"].Count(); j++)
                {
                    SingleQuestion_String_OneChoice_ViewModel sqvm = new SingleQuestion_String_OneChoice_ViewModel();
                    sqvm.STT = stt.ToString();
                    stt++;
                    sqvm.ID = testDataInJson["test"]["groups"][i]["underline"]["qSingle"][j]["sID"]?.ToString();
                    sqvm.Score = testDataInJson["test"]["groups"][i]["underline"]["qSingle"][j]["sScore"]?.ToString();
                    sqvm.Content = testDataInJson["test"]["groups"][i]["underline"]["qSingle"][j]["sContain"]?.ToString();
                    sqvm.QuestionType = QuestionTypeEnum.Underline;
                    sqvm.PossibleAnswerList = new List<PossibleAnswerViewModel>();
                    sqvm.SelectedAnswer = "";
                    // lấy đáp án
                    int awStt = 1;
                    for (int k = 0; k < testDataInJson["test"]["groups"][i]["underline"]["qSingle"][j]["sAList"].Count(); k++)
                    {
                        PossibleAnswerViewModel avm = new PossibleAnswerViewModel();
                        avm.STT = awStt.ToString();
                        avm.ID = awStt.ToString();
                        awStt++;
                        avm.Content = testDataInJson["test"]["groups"][i]["underline"]["qSingle"][j]["sAList"][k]["sAContain"]?.ToString();
                        avm.RightAnswer = testDataInJson["test"]["groups"][i]["underline"]["qSingle"][j]["sAList"][k]["sAw"]?.ToString();
                        sqvm.PossibleAnswerList.Add(avm);
                    }
                    awStt = 1;
                    ShuffleList(sqvm.PossibleAnswerList);
                    foreach (var answer in sqvm.PossibleAnswerList)
                    {
                        answer.STT = awStt.ToString();
                    }
                    testGroup.QuestionsList.Add(sqvm);
                }

                // lấy câu hỏi gạch chân chùm - get group underline
                //testGroup.Underline.ListOfGroups = new List<GroupQuestionViewModel>();
                for (int l = 0; l < testDataInJson["test"]["groups"][i]["underline"]["qGroup"].Count(); l++)
                {
                    GroupQuestionViewModel gqvm = new GroupQuestionViewModel();
                    gqvm.Content = testDataInJson["test"]["groups"][i]["underline"]["qGroup"][l]["qgTitle"]?.ToString();
                    gqvm.SingleQuestionsList = new List<object>();
                    gqvm.QuestionType = QuestionTypeEnum.Group;

                    for (int j = 0; j < testDataInJson["test"]["groups"][i]["underline"]["qGroup"][l]["underlineSList"].Count(); j++)
                    {
                        SingleQuestion_String_OneChoice_ViewModel sqvm = new SingleQuestion_String_OneChoice_ViewModel();
                        sqvm.STT = stt.ToString();
                        stt++;
                        sqvm.ID = testDataInJson["test"]["groups"][i]["underline"]["qGroup"][l]["underlineSList"][j]["sID"]?.ToString();
                        sqvm.Score = testDataInJson["test"]["groups"][i]["underline"]["qGroup"][l]["underlineSList"][j]["sScore"]?.ToString();
                        sqvm.Content = testDataInJson["test"]["groups"][i]["underline"]["qGroup"][l]["underlineSList"][j]["sContain"]?.ToString();
                        sqvm.QuestionType = QuestionTypeEnum.Underline;
                        sqvm.PossibleAnswerList = new List<PossibleAnswerViewModel>();
                        sqvm.SelectedAnswer = "";
                        // lấy đáp án
                        int awStt = 1;
                        for (int k = 0; k < testDataInJson["test"]["groups"][i]["underline"]["qGroup"][l]["underlineSList"][j]["sAList"].Count(); k++)
                        {
                            PossibleAnswerViewModel avm = new PossibleAnswerViewModel();
                            avm.STT = awStt.ToString();
                            avm.ID = awStt.ToString();
                            awStt++;
                            avm.Content = testDataInJson["test"]["groups"][i]["underline"]["qGroup"][l]["underlineSList"][j]["sAList"][k]["sAContain"]?.ToString();
                            avm.RightAnswer = testDataInJson["test"]["groups"][i]["underline"]["qGroup"][l]["underlineSList"][j]["sAList"][k]["sAw"]?.ToString();
                            sqvm.PossibleAnswerList.Add(avm);
                        }
                        awStt = 1;
                        ShuffleList(sqvm.PossibleAnswerList);
                        foreach (var answer in sqvm.PossibleAnswerList)
                        {
                            answer.STT = awStt.ToString();
                        }
                        gqvm.SingleQuestionsList.Add(sqvm);
                    }
                    testGroup.QuestionsList.Add(gqvm);
                }
                #endregion

                #region Câu Điền Khuyết - Fill Question
                //testGroup.Fill = new QuestionPackage();

                // lấy câu hỏi điền khuyết đơn - get single fill
                //testGroup.Fill.ListOfSingles = new List<SingleQuestionViewModel>();
                for (int j = 0; j < testDataInJson["test"]["groups"][i]["fill"]["qSingle"].Count(); j++)
                {
                    SingleQuestion_String_MultiChoices_ViewModel sqvm = new SingleQuestion_String_MultiChoices_ViewModel();
                    sqvm.STT = stt.ToString();
                    stt++;
                    sqvm.ID = testDataInJson["test"]["groups"][i]["fill"]["qSingle"][j]["sID"]?.ToString();
                    sqvm.Score = testDataInJson["test"]["groups"][i]["fill"]["qSingle"][j]["sScore"]?.ToString();
                    sqvm.Content = testDataInJson["test"]["groups"][i]["fill"]["qSingle"][j]["sContain"]?.ToString();
                    sqvm.QuestionType = QuestionTypeEnum.Fill;
                    sqvm.PossibleAnswerList = new List<PossibleAnswerViewModel>();
                    sqvm.SelectedAnswer = new List<SingleQuestion_String_MultiChoices_ViewModel.AnswerPacket>();
                    // lấy đáp án
                    int awStt = 1;
                    for (int k = 0; k < testDataInJson["test"]["groups"][i]["fill"]["qSingle"][j]["sAList"].Count(); k++)
                    {
                        PossibleAnswerViewModel avm = new PossibleAnswerViewModel();
                        avm.STT = awStt.ToString();
                        avm.ID = awStt.ToString();
                        awStt++;
                        avm.Content = testDataInJson["test"]["groups"][i]["fill"]["qSingle"][j]["sAList"][k]["sAContain"]?.ToString();
                        avm.RightAnswer = testDataInJson["test"]["groups"][i]["fill"]["qSingle"][j]["sAList"][k]["sAw"]?.ToString();
                        sqvm.PossibleAnswerList.Add(avm);
                    }
                    awStt = 1;
                    ShuffleList(sqvm.PossibleAnswerList);
                    foreach (var answer in sqvm.PossibleAnswerList)
                    {
                        answer.STT = awStt.ToString();
                    }
                    testGroup.QuestionsList.Add(sqvm);
                }

                // lấy câu hỏi điền khuyết chùm - get group fill
                //testGroup.Fill.ListOfGroups = new List<GroupQuestionViewModel>();
                for (int l = 0; l < testDataInJson["test"]["groups"][i]["fill"]["qGroup"].Count(); l++)
                {
                    GroupQuestionViewModel gqvm = new GroupQuestionViewModel();
                    gqvm.Content = testDataInJson["test"]["groups"][i]["fill"]["qGroup"][l]["qgTitle"]?.ToString();
                    gqvm.SingleQuestionsList = new List<object>();
                    gqvm.QuestionType = QuestionTypeEnum.Group;

                    for (int j = 0; j < testDataInJson["test"]["groups"][i]["fill"]["qGroup"][l]["fillSList"].Count(); j++)
                    {
                        SingleQuestion_String_MultiChoices_ViewModel sqvm = new SingleQuestion_String_MultiChoices_ViewModel();
                        sqvm.STT = stt.ToString();
                        stt++;
                        sqvm.ID = testDataInJson["test"]["groups"][i]["fill"]["qGroup"][l]["fillSList"][j]["sID"]?.ToString();
                        sqvm.Score = testDataInJson["test"]["groups"][i]["fill"]["qGroup"][l]["fillSList"][j]["sScore"]?.ToString();
                        sqvm.Content = testDataInJson["test"]["groups"][i]["fill"]["qGroup"][l]["fillSList"][j]["sContain"]?.ToString();
                        sqvm.QuestionType = QuestionTypeEnum.Fill;
                        sqvm.PossibleAnswerList = new List<PossibleAnswerViewModel>();
                        sqvm.SelectedAnswer = new List<SingleQuestion_String_MultiChoices_ViewModel.AnswerPacket>();
                        // lấy đáp án
                        int awStt = 1;
                        for (int k = 0; k < testDataInJson["test"]["groups"][i]["fill"]["qGroup"][l]["fillSList"][j]["sAList"].Count(); k++)
                        {
                            PossibleAnswerViewModel avm = new PossibleAnswerViewModel();
                            avm.STT = awStt.ToString();
                            avm.ID = awStt.ToString();
                            awStt++;
                            avm.Content = testDataInJson["test"]["groups"][i]["fill"]["qGroup"][l]["fillSList"][j]["sAList"][k]["sAContain"]?.ToString();
                            avm.RightAnswer = testDataInJson["test"]["groups"][i]["fill"]["qGroup"][l]["fillSList"][j]["sAList"][k]["sAw"]?.ToString();
                            sqvm.PossibleAnswerList.Add(avm);
                        }
                        awStt = 1;
                        ShuffleList(sqvm.PossibleAnswerList);
                        foreach (var answer in sqvm.PossibleAnswerList)
                        {
                            answer.STT = awStt.ToString();
                        }
                        gqvm.SingleQuestionsList.Add(sqvm);
                    }
                    testGroup.QuestionsList.Add(gqvm);
                }
                #endregion

                #region Câu Đúng Sai - True False Question
                //testGroup.TrueFalse = new QuestionPackage();

                // lấy câu hỏi đúng sai đơn - get single trueFalse
                //testGroup.TrueFalse.ListOfSingles = new List<SingleQuestionViewModel>();
                for (int j = 0; j < testDataInJson["test"]["groups"][i]["trueFalse"]["qSingle"].Count(); j++)
                {
                    SingleQuestion_Bool_MultiChoices_ViewModel sqvm = new SingleQuestion_Bool_MultiChoices_ViewModel();
                    sqvm.STT = stt.ToString();
                    stt++;
                    sqvm.ID = testDataInJson["test"]["groups"][i]["trueFalse"]["qSingle"][j]["sID"]?.ToString();
                    sqvm.Score = testDataInJson["test"]["groups"][i]["trueFalse"]["qSingle"][j]["sScore"]?.ToString();
                    sqvm.Content = testDataInJson["test"]["groups"][i]["trueFalse"]["qSingle"][j]["sContain"]?.ToString();
                    sqvm.QuestionType = QuestionTypeEnum.TrueFalse;
                    sqvm.PossibleAnswerList = new List<PossibleAnswerViewModel>();
                    sqvm.SelectedAnswer = new List<SingleQuestion_Bool_MultiChoices_ViewModel.AnswerPacket>();

                    // lấy đáp án
                    int awStt = 1;
                    for (int k = 0; k < testDataInJson["test"]["groups"][i]["trueFalse"]["qSingle"][j]["sAList"].Count(); k++)
                    {
                        PossibleAnswerViewModel avm = new PossibleAnswerViewModel();
                        avm.STT = awStt.ToString();
                        avm.ID = awStt.ToString();
                        awStt++;
                        avm.Content = testDataInJson["test"]["groups"][i]["trueFalse"]["qSingle"][j]["sAList"][k]["sAContain"]?.ToString();
                        avm.RightAnswer = testDataInJson["test"]["groups"][i]["trueFalse"]["qSingle"][j]["sAList"][k]["sAw"]?.ToString();
                        sqvm.PossibleAnswerList.Add(avm);
                    }
                    awStt = 1;
                    ShuffleList(sqvm.PossibleAnswerList);
                    foreach (var answer in sqvm.PossibleAnswerList)
                    {
                        answer.STT = awStt.ToString();
                    }
                    testGroup.QuestionsList.Add(sqvm);
                }

                // lấy câu hỏi đúng sai chùm - get group trueFalse
                //testGroup.TrueFalse.ListOfGroups = new List<GroupQuestionViewModel>();
                for (int l = 0; l < testDataInJson["test"]["groups"][i]["trueFalse"]["qGroup"].Count(); l++)
                {
                    GroupQuestionViewModel gqvm = new GroupQuestionViewModel();
                    gqvm.Content = testDataInJson["test"]["groups"][i]["trueFalse"]["qGroup"][l]["qgTitle"]?.ToString();
                    gqvm.SingleQuestionsList = new List<object>();
                    gqvm.QuestionType = QuestionTypeEnum.Group;

                    for (int j = 0; j < testDataInJson["test"]["groups"][i]["trueFalse"]["qGroup"][l]["truefalseSList"].Count(); j++)
                    {
                        SingleQuestion_Bool_MultiChoices_ViewModel sqvm = new SingleQuestion_Bool_MultiChoices_ViewModel();
                        sqvm.STT = stt.ToString();
                        stt++;
                        sqvm.ID = testDataInJson["test"]["groups"][i]["trueFalse"]["qGroup"][l]["truefalseSList"][j]["sID"]?.ToString();
                        sqvm.Score = testDataInJson["test"]["groups"][i]["trueFalse"]["qGroup"][l]["truefalseSList"][j]["sScore"]?.ToString();
                        sqvm.Content = testDataInJson["test"]["groups"][i]["trueFalse"]["qGroup"][l]["truefalseSList"][j]["sContain"]?.ToString();
                        sqvm.QuestionType = QuestionTypeEnum.TrueFalse;
                        sqvm.PossibleAnswerList = new List<PossibleAnswerViewModel>();
                        sqvm.SelectedAnswer = new List<SingleQuestion_Bool_MultiChoices_ViewModel.AnswerPacket>();

                        // lấy đáp án
                        int awStt = 1;
                        for (int k = 0; k < testDataInJson["test"]["groups"][i]["trueFalse"]["qGroup"][l]["truefalseSList"][j]["sAList"].Count(); k++)
                        {
                            PossibleAnswerViewModel avm = new PossibleAnswerViewModel();
                            avm.STT = awStt.ToString();
                            avm.ID = awStt.ToString();
                            awStt++;
                            avm.Content = testDataInJson["test"]["groups"][i]["trueFalse"]["qGroup"][l]["truefalseSList"][j]["sAList"][k]["sAContain"]?.ToString();
                            avm.RightAnswer = testDataInJson["test"]["groups"][i]["trueFalse"]["qGroup"][l]["truefalseSList"][j]["sAList"][k]["sAw"]?.ToString();
                            sqvm.PossibleAnswerList.Add(avm);
                        }
                        awStt = 1;
                        ShuffleList(sqvm.PossibleAnswerList);
                        foreach (var answer in sqvm.PossibleAnswerList)
                        {
                            answer.STT = awStt.ToString();
                        }
                        gqvm.SingleQuestionsList.Add(sqvm);
                    }
                    testGroup.QuestionsList.Add(gqvm);
                }
                #endregion

                #region Câu Nối Chéo - Matching Question
                //testGroup.Matching = new QuestionPackage();

                // lấy câu hỏi nối chéo đơn - get single matching
                //testGroup.Matching.ListOfSingles = new List<SingleQuestionViewModel>();
                for (int j = 0; j < testDataInJson["test"]["groups"][i]["matching"]["qSingle"].Count(); j++)
                {
                    SingleQuestion_String_MultiChoices_ViewModel sqvm = new SingleQuestion_String_MultiChoices_ViewModel();
                    sqvm.STT = stt.ToString();
                    stt++;
                    sqvm.ID = testDataInJson["test"]["groups"][i]["matching"]["qSingle"][j]["sID"]?.ToString();
                    sqvm.Score = testDataInJson["test"]["groups"][i]["matching"]["qSingle"][j]["sScore"]?.ToString();
                    sqvm.Content = testDataInJson["test"]["groups"][i]["matching"]["qSingle"][j]["sContain"]?.ToString();
                    sqvm.QuestionType = QuestionTypeEnum.Matching;
                    sqvm.PossibleAnswerList = new List<PossibleAnswerViewModel>();
                    sqvm.SelectedAnswer = new List<SingleQuestion_String_MultiChoices_ViewModel.AnswerPacket>();

                    // lấy đáp án
                    int awStt = 1;
                    for (int k = 0; k < testDataInJson["test"]["groups"][i]["matching"]["qSingle"][j]["sAList"].Count(); k++)
                    {
                        PossibleAnswerViewModel avm = new PossibleAnswerViewModel();
                        avm.STT = awStt.ToString();
                        avm.ID = awStt.ToString();
                        awStt++;
                        avm.Content = testDataInJson["test"]["groups"][i]["matching"]["qSingle"][j]["sAList"][k]["sAContain"]?.ToString();
                        avm.RightAnswer = testDataInJson["test"]["groups"][i]["matching"]["qSingle"][j]["sAList"][k]["sAw"]?.ToString();
                        sqvm.PossibleAnswerList.Add(avm);
                    }
                    testGroup.QuestionsList.Add(sqvm);
                }

                // lấy câu hỏi nối chéo chùm - get group matching
                //testGroup.Matching.ListOfGroups = new List<GroupQuestionViewModel>();
                for (int l = 0; l < testDataInJson["test"]["groups"][i]["matching"]["qGroup"].Count(); l++)
                {
                    GroupQuestionViewModel gqvm = new GroupQuestionViewModel();
                    gqvm.Content = testDataInJson["test"]["groups"][i]["matching"]["qGroup"][l]["qgTitle"]?.ToString();
                    gqvm.SingleQuestionsList = new List<object>();
                    gqvm.QuestionType = QuestionTypeEnum.Group;

                    for (int j = 0; j < testDataInJson["test"]["groups"][i]["matching"]["qGroup"][l]["matchingSList"].Count(); j++)
                    {
                        SingleQuestion_String_MultiChoices_ViewModel sqvm = new SingleQuestion_String_MultiChoices_ViewModel();
                        sqvm.STT = stt.ToString();
                        stt++;
                        sqvm.ID = testDataInJson["test"]["groups"][i]["matching"]["qGroup"][l]["matchingSList"][j]["sID"]?.ToString();
                        sqvm.Score = testDataInJson["test"]["groups"][i]["matching"]["qGroup"][l]["matchingSList"][j]["sScore"]?.ToString();
                        sqvm.Content = testDataInJson["test"]["groups"][i]["matching"]["qGroup"][l]["matchingSList"][j]["sContain"]?.ToString();
                        sqvm.QuestionType = QuestionTypeEnum.Matching;
                        sqvm.PossibleAnswerList = new List<PossibleAnswerViewModel>();
                        sqvm.SelectedAnswer = new List<SingleQuestion_String_MultiChoices_ViewModel.AnswerPacket>();

                        // lấy đáp án
                        int awStt = 1;
                        for (int k = 0; k < testDataInJson["test"]["groups"][i]["matching"]["qGroup"][l]["matchingSList"][j]["sAList"].Count(); k++)
                        {
                            PossibleAnswerViewModel avm = new PossibleAnswerViewModel();
                            avm.STT = awStt.ToString();
                            avm.ID = awStt.ToString();
                            awStt++;
                            avm.Content = testDataInJson["test"]["groups"][i]["matching"]["qGroup"][l]["matchingSList"][j]["sAList"][k]["sAContain"]?.ToString();
                            avm.RightAnswer = testDataInJson["test"]["groups"][i]["matching"]["qGroup"][l]["matchingSList"][j]["sAList"][k]["sAw"]?.ToString();
                            sqvm.PossibleAnswerList.Add(avm);
                        }
                        gqvm.SingleQuestionsList.Add(sqvm);
                    }
                    testGroup.QuestionsList.Add(gqvm);
                }
                #endregion

                ShuffleList(testGroup.QuestionsList);
                testGroupList.Add(testGroup);
            }
            #endregion

            #region To String All Right Answers
            stt = 1;
            string result = "";
            foreach (var testgroup in testGroupList)
            {
                foreach (var question in testgroup.QuestionsList)
                {
                    switch ((question as QuestionObjectViewModel).QuestionType)
                    {
                        case QuestionTypeEnum.Group:
                            foreach (var questionChild in ((GroupQuestionViewModel)question).SingleQuestionsList)
                            {
                                switch ((((SingleQuestionViewModel)questionChild) as QuestionObjectViewModel).QuestionType)
                                {
                                    case QuestionTypeEnum.Quiz:
                                    case QuestionTypeEnum.Underline:
                                        ((SingleQuestionViewModel)questionChild).STT = stt.ToString();
                                        int positionOneChoiceG = 1;
                                        foreach (var answer in ((SingleQuestionViewModel)questionChild).PossibleAnswerList)
                                        {
                                            if (bool.Parse(answer.RightAnswer))
                                            {
                                                result += stt.ToString() + "-" + positionOneChoiceG.ToString();
                                                break;
                                            }
                                            positionOneChoiceG++;
                                        }
                                        result += "|";
                                        stt++;
                                        break;
                                    case QuestionTypeEnum.QuizN:
                                    case QuestionTypeEnum.TrueFalse:
                                        ((SingleQuestionViewModel)questionChild).STT = stt.ToString();
                                        int positionMultiChoiceG = 1;
                                        result += stt.ToString();
                                        foreach (var answer in ((SingleQuestionViewModel)questionChild).PossibleAnswerList)
                                        {
                                            if (bool.Parse(answer.RightAnswer))
                                            {
                                                result += "-" + positionMultiChoiceG.ToString();
                                            }
                                            positionMultiChoiceG++;
                                        }
                                        result += "|";
                                        stt++;
                                        break;
                                    case QuestionTypeEnum.Fill:
                                        ((SingleQuestionViewModel)questionChild).STT = stt.ToString();
                                        int positionFillChoiceG = 1;
                                        result += stt.ToString();
                                        foreach (var answer in ((SingleQuestionViewModel)questionChild).PossibleAnswerList)
                                        {
                                            if (!answer.RightAnswer.Equals("0"))
                                            {
                                                result += "-" + positionFillChoiceG.ToString() + "+" + answer.RightAnswer;
                                            }
                                            positionFillChoiceG++;
                                        }
                                        result += "|";
                                        stt++;
                                        break;
                                }
                            }
                            break;
                        case QuestionTypeEnum.Quiz:
                        case QuestionTypeEnum.Underline:
                            ((SingleQuestionViewModel)question).STT = stt.ToString();
                            int positionOneChoice = 1;
                            foreach (var answer in ((SingleQuestionViewModel)question).PossibleAnswerList)
                            {
                                if (bool.Parse(answer.RightAnswer))
                                {
                                    result += stt.ToString() + "-" + positionOneChoice.ToString();
                                    break;
                                }
                                positionOneChoice++;
                            }
                            result += "|";
                            stt++;
                            break;
                        case QuestionTypeEnum.QuizN:
                        case QuestionTypeEnum.TrueFalse:
                            ((SingleQuestionViewModel)question).STT = stt.ToString();
                            int positionMultiChoice = 1;
                            result += stt.ToString();
                            foreach (var answer in ((SingleQuestionViewModel)question).PossibleAnswerList)
                            {
                                if (bool.Parse(answer.RightAnswer))
                                {
                                    result += "-" + positionMultiChoice.ToString();
                                }
                                positionMultiChoice++;
                            }
                            result += "|";
                            stt++;
                            break;
                        case QuestionTypeEnum.Fill:
                            ((SingleQuestionViewModel)question).STT = stt.ToString();
                            int positionFillChoice = 1;
                            result += stt.ToString();
                            foreach (var answer in ((SingleQuestionViewModel)question).PossibleAnswerList)
                            {
                                if (int.Parse(answer.RightAnswer) != 0)
                                {
                                    result += "-" + positionFillChoice.ToString() + "+" + answer.RightAnswer;
                                }
                                positionFillChoice++;
                            }
                            result += "|";
                            stt++;
                            break;
                        case QuestionTypeEnum.Matching:
                            ((SingleQuestionViewModel)question).STT = stt.ToString();
                            result += stt.ToString();
                            for (int i = 0; i < ((SingleQuestionViewModel)question).PossibleAnswerList.Count; i += 2)
                            {
                                if (!((SingleQuestionViewModel)question).PossibleAnswerList[i].RightAnswer.Equals("0"))
                                {
                                    result += "-" + (i + 1).ToString() + "+" + ((SingleQuestionViewModel)question).PossibleAnswerList[i].RightAnswer;
                                }
                            }
                            result += "|";
                            stt++;
                            break;
                    }
                }
            }
            result = Base64Encode(result);
            #endregion

            #region Inits Test Info
            var viewModel = new TestDataViewModel
            {
                SchoolName = testDataInJson["test"]["header"]["name"]?.ToString(),
                SchoolInfo = testDataInJson["test"]["header"]["info"]?.ToString(),
                Year = testDataInJson["test"]["header"]["year"]?.ToString(),
                TestType = testDataInJson["test"]["header"]["type"]?.ToString(),
                TestTime = testDataInJson["test"]["header"]["time"]?.ToString(),
                Subject = testDataInJson["test"]["header"]["subject"]?.ToString(),
                Grade = testDataInJson["test"]["header"]["grade"]?.ToString(),
                StudentName = Session["FullName"].ToString(),
                StudentClass = Session["Class"].ToString(),
                StudentIPAdd = IP,
                TestGroupChoose = "",
                Base64Code = result,
                TestGroupList = testGroupList
            };
            #endregion

            return View(viewModel);
        }

        public ActionResult DoneTest(FormCollection formCollection)
        {
            StreamReader sr = new StreamReader(Server.MapPath("~/Content/data.json"));
            CurrentStudentTestStep = StudentTestStepEnum.DoingTest;
            var testDataInJson = JObject.Parse(sr.ReadToEnd());

            string result = Base64Decode(formCollection["Base64Code"]);


            foreach (var item in formCollection)
            {
                if (item.ToString().StartsWith("Answer-"))
                {
                    var studentAnswerValue = formCollection[item.ToString()];

                    var answerSergments = studentAnswerValue.Split('-');
                    // Vi tri cau hoi
                    var questionSTT = answerSergments[0];
                    // Vi tri cau tra loi cua hoc  sinh
                    var selectedAnswer = answerSergments[1];

                }
            }
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

        private class ThreadSafeRandom
        {
            [ThreadStatic]
            private static Random Local;

            public static Random ThisThreadsRandom
            {
                get { return Local ?? (Local = new Random(unchecked(Environment.TickCount * 31 + System.Threading.Thread.CurrentThread.ManagedThreadId))); }
            }
        }

        private void ShuffleList(List<object> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
                object value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        private void ShuffleList(List<PossibleAnswerViewModel> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
                PossibleAnswerViewModel value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }


        public string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}