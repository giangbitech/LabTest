using BiTech.LabTest.BLL;
using BiTech.LabTest.DAL.Models;
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
using System.Web.Security;
using static BiTech.LabTest.DAL.Models.TestData;
using static BiTech.LabTest.Models.ViewModels.Student;

namespace BiTech.LabTest.Controllers
{
    public class StudentController : Controller
    {
        public StudentLogic _StudentLogic { get; set; }

        public StudentTestStepEnum CurrentStudentTestStep { get; set; }

        private string STUDENT_INFO_COOKIE_NAME = "StudentInfoCookie";
        private string STUDENT_TEST_RESULT_ID_COOKIE_NAME = "StudentTestResultID";

        public StudentController()
        {
            // Xác định quyền của người dùng trong controller này
            ViewBag.ApplicationRole = Models.Enums.ApplicationRole.Student;
        }

        /// <summary>
        /// hàm index chính, khi vào dây sẽ chuyển sang action joinTest
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return RedirectToAction("JoinTest");
        }

        /// <summary>
        /// Cho ra view joinTest
        /// Nếu đang chờ thi thì không chuyển sang trang chờ thi
        /// Nếu có điểm rồi thì chuyến sang trang kết quả
        /// </summary>
        /// <returns></returns>
        public ActionResult JoinTest(string changeName)
        {
            if (this.ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains(STUDENT_TEST_RESULT_ID_COOKIE_NAME))
            {
                StudentInfoCookieModel studentIC = new StudentInfoCookieModel();
                HttpCookie cookie = this.ControllerContext.HttpContext.Request.Cookies[STUDENT_TEST_RESULT_ID_COOKIE_NAME];
                string testResultID = cookie.Value;
                _StudentLogic = new StudentLogic();
                TestResult testResult = _StudentLogic.GetTestResult(testResultID);
                string aa = testResult.StudentName;
            }

            var testDataId = Session["TestDataId"];

            //Nếu có điểm rồi không cho thi lại
            if (Session["Score"] != null)
                return RedirectToAction("FinishTest");

            // Nếu đang thi mà thí sinh khong có tên thì không được thi.
            if (CurrentStudentTestStep == StudentTestStepEnum.DoingTest && (Session["FullName"] == null || Session["Class"] == null))
            {
                return RedirectToAction("WaitingScreen");
            }

            //Nếu đang thi và có tên lẫn lớp thì vào thi
            if (CurrentStudentTestStep == StudentTestStepEnum.DoingTest && (Session["FullName"] != null && Session["Class"] != null) && testDataId != null)
            {
                return RedirectToAction("DoTest");
            }

            //Nếu đang thi và có tên lẫn lớp thì vào thi
            if (CurrentStudentTestStep == StudentTestStepEnum.DoingTest && (Session["FullName"] != null && Session["Class"] != null) && testDataId == null)
            {
                return RedirectToAction("WaitingScreen");
            }

            // Kiểm tra nhập thông tin thí sinh
            if (string.IsNullOrEmpty(changeName)
                ||
                (Session["FullName"] == null || Session["Class"] == null))
            {
                Session["FullName"] = Session["Class"] = null;
                return View();
            }

            // Kiểm tra trạng thái thi
            _StudentLogic = new StudentLogic();
            if (_StudentLogic.GetTestStep(testDataId.ToString()) == TestStepEnum.Waiting)
            {
                Session["FullName"] = null;
                Session["Class"] = null;
                removeStudentInfoCookie();
                return RedirectToAction("JoinTest");

            }

            // Thí sinh đã nhập thông tin
            // Chưa thi thì chuyển vào Waiting
            if (CurrentStudentTestStep == StudentTestStepEnum.WaitingTest) //|| Session["fullname"] != null
            {
                return RedirectToAction("WaitingScreen");
            }

            return View();
        }

        /// <summary>
        /// Kiểm tra học sinh đăng ký tên và cho vào trang chờ
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("JoinTest")]
        public ActionResult DoJoinTest(StudentJoinTestViewModel viewModel)
        {
            //Nếu có điểm rồi không cho thi lại
            if (Session["Score"] != null)
                return RedirectToAction("FinishTest");

            if (ModelState.IsValid)
            {
                Session["FullName"] = viewModel.FullName;
                Session["Class"] = viewModel.Class;

                CurrentStudentTestStep = StudentTestStepEnum.WaitingTest;
                return RedirectToAction("WaitingScreen");
            }

            return View();
        }

        /// <summary>
        /// Màn hình chờ thi
        /// </summary>
        /// <returns></returns>
        public ActionResult WaitingScreen()
        {
            //Nếu có điểm rồi không cho thi lại
            if (Session["Score"] != null)
                return RedirectToAction("FinishTest");

            // Chưa nhập thông tin thí sinh
            if (Session["FullName"] == null || Session["Class"] == null)
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

        /// <summary>
        /// submit test data ID
        /// </summary>
        /// <param name="testDataId"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("DoTest")]
        public ActionResult SubmitDoTest(string testDataId)
        {
            //Nếu có điểm rồi không cho thi lại
            if (Session["Score"] != null)
                return RedirectToAction("FinishTest");

            Session["TestDataId"] = testDataId;

            return Json(new { testDataId = testDataId });
        }

        /// <summary>
        /// Lấy đề thi từ csdl và xuất ra view
        /// </summary>
        /// <returns>View của DoTest</returns>
        public ActionResult DoTest()
        {
            //Nếu có điểm rồi không cho thi lại
            if (Session["Score"] != null)
                return RedirectToAction("FinishTest");
            if (Session["FullName"] == null || Session["Class"] == null)
                return RedirectToAction("JoinTest");

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
            StreamReader steamReader = new StreamReader(Server.MapPath("~/Content/data.json"));
            CurrentStudentTestStep = StudentTestStepEnum.DoingTest;
            var testDataInJson = JObject.Parse(steamReader.ReadToEnd());

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
                // lấy câu hỏi trắc nghiệm đơn - get single quiz
                for (int j = 0; j < testDataInJson["test"]["groups"][i]["quiz"]["qSingle"].Count(); j++)
                {
                    List<PossibleAnswerViewModel> possibleAnswerList = new List<PossibleAnswerViewModel>();
                    int rightChoiceCount = 0;
                    // lấy đáp án
                    int awStt = 1;
                    for (int k = 0; k < testDataInJson["test"]["groups"][i]["quiz"]["qSingle"][j]["sAList"].Count(); k++)
                    {
                        PossibleAnswerViewModel answerVM = new PossibleAnswerViewModel();
                        answerVM.STT = awStt.ToString();
                        answerVM.ID = awStt.ToString();
                        awStt++;
                        answerVM.Content = testDataInJson["test"]["groups"][i]["quiz"]["qSingle"][j]["sAList"][k]["sAContain"]?.ToString();
                        answerVM.RightAnswer = testDataInJson["test"]["groups"][i]["quiz"]["qSingle"][j]["sAList"][k]["sAw"]?.ToString();
                        rightChoiceCount += bool.Parse(answerVM.RightAnswer) == true ? 1 : 0;
                        possibleAnswerList.Add(answerVM);
                    }
                    awStt = 1;
                    ShuffleList(possibleAnswerList);
                    foreach (var answer in possibleAnswerList)
                    {
                        answer.STT = awStt.ToString();
                        awStt++;
                    }

                    if (rightChoiceCount == 1)
                    {
                        SingleQuestion_String_OneChoice_ViewModel singleQuestionVM = new SingleQuestion_String_OneChoice_ViewModel();
                        singleQuestionVM.STT = stt.ToString();
                        stt++;
                        singleQuestionVM.ID = testDataInJson["test"]["groups"][i]["quiz"]["qSingle"][j]["sID"]?.ToString();
                        singleQuestionVM.Score = testDataInJson["test"]["groups"][i]["quiz"]["qSingle"][j]["sScore"]?.ToString();
                        singleQuestionVM.Content = testDataInJson["test"]["groups"][i]["quiz"]["qSingle"][j]["sContain"]?.ToString();
                        singleQuestionVM.QuestionType = QuestionTypeEnum.Quiz;
                        singleQuestionVM.SelectedAnswer = "";
                        singleQuestionVM.PossibleAnswerList = possibleAnswerList;
                        testGroup.QuestionsList.Add(singleQuestionVM);
                    }
                    else
                    {
                        SingleQuestion_Bool_MultiChoices_ViewModel singleQuestionVM = new SingleQuestion_Bool_MultiChoices_ViewModel();
                        singleQuestionVM.STT = stt.ToString();
                        stt++;
                        singleQuestionVM.ID = testDataInJson["test"]["groups"][i]["quiz"]["qSingle"][j]["sID"]?.ToString();
                        singleQuestionVM.Score = testDataInJson["test"]["groups"][i]["quiz"]["qSingle"][j]["sScore"]?.ToString();
                        singleQuestionVM.Content = testDataInJson["test"]["groups"][i]["quiz"]["qSingle"][j]["sContain"]?.ToString();
                        singleQuestionVM.QuestionType = QuestionTypeEnum.QuizN;
                        singleQuestionVM.SelectedAnswer = new List<SingleQuestion_Bool_MultiChoices_ViewModel.AnswerPacket>();
                        singleQuestionVM.PossibleAnswerList = possibleAnswerList;
                        testGroup.QuestionsList.Add(singleQuestionVM);
                    }
                }

                // lấy câu hỏi trắc nghiệm chùm - get group quiz
                for (int l = 0; l < testDataInJson["test"]["groups"][i]["quiz"]["qGroup"].Count(); l++)
                {
                    GroupQuestionViewModel groupQuestionVM = new GroupQuestionViewModel();
                    groupQuestionVM.Content = testDataInJson["test"]["groups"][i]["quiz"]["qGroup"][l]["qgTitle"]?.ToString();
                    groupQuestionVM.SingleQuestionsList = new List<object>();
                    groupQuestionVM.QuestionType = QuestionTypeEnum.Group;

                    for (int j = 0; j < testDataInJson["test"]["groups"][i]["quiz"]["qGroup"][l]["quizSList"].Count(); j++)
                    {
                        List<PossibleAnswerViewModel> possibleAnswerList = new List<PossibleAnswerViewModel>();
                        int rightChoiceCount = 0;
                        // lấy đáp án
                        int awStt = 1;
                        for (int k = 0; k < testDataInJson["test"]["groups"][i]["quiz"]["qGroup"][l]["quizSList"][j]["sAList"].Count(); k++)
                        {
                            PossibleAnswerViewModel answerVM = new PossibleAnswerViewModel();
                            answerVM.STT = awStt.ToString();
                            answerVM.ID = awStt.ToString();
                            awStt++;
                            answerVM.Content = testDataInJson["test"]["groups"][i]["quiz"]["qGroup"][l]["quizSList"][j]["sAList"][k]["sAContain"]?.ToString();
                            answerVM.RightAnswer = testDataInJson["test"]["groups"][i]["quiz"]["qGroup"][l]["quizSList"][j]["sAList"][k]["sAw"]?.ToString();
                            rightChoiceCount += bool.Parse(answerVM.RightAnswer) == true ? 1 : 0;
                            possibleAnswerList.Add(answerVM);
                        }
                        awStt = 1;
                        ShuffleList(possibleAnswerList);
                        foreach (var answer in possibleAnswerList)
                        {
                            answer.STT = awStt.ToString();
                            awStt++;
                        }
                        if (rightChoiceCount == 1)
                        {
                            SingleQuestion_String_OneChoice_ViewModel singleQuestionVM = new SingleQuestion_String_OneChoice_ViewModel();
                            singleQuestionVM.STT = stt.ToString();
                            stt++;
                            singleQuestionVM.ID = testDataInJson["test"]["groups"][i]["quiz"]["qGroup"][l]["quizSList"][j]["sID"]?.ToString();
                            singleQuestionVM.Score = testDataInJson["test"]["groups"][i]["quiz"]["qGroup"][l]["quizSList"][j]["sScore"]?.ToString();
                            singleQuestionVM.Content = testDataInJson["test"]["groups"][i]["quiz"]["qGroup"][l]["quizSList"][j]["sContain"]?.ToString();
                            singleQuestionVM.QuestionType = QuestionTypeEnum.Quiz;
                            singleQuestionVM.SelectedAnswer = "";
                            singleQuestionVM.PossibleAnswerList = possibleAnswerList;

                            groupQuestionVM.SingleQuestionsList.Add(singleQuestionVM);
                        }
                        else
                        {
                            SingleQuestion_Bool_MultiChoices_ViewModel singleQuestionVM = new SingleQuestion_Bool_MultiChoices_ViewModel();
                            singleQuestionVM.STT = stt.ToString();
                            stt++;
                            singleQuestionVM.ID = testDataInJson["test"]["groups"][i]["quiz"]["qGroup"][l]["quizSList"][j]["sID"]?.ToString();
                            singleQuestionVM.Score = testDataInJson["test"]["groups"][i]["quiz"]["qGroup"][l]["quizSList"][j]["sScore"]?.ToString();
                            singleQuestionVM.Content = testDataInJson["test"]["groups"][i]["quiz"]["qGroup"][l]["quizSList"][j]["sContain"]?.ToString();
                            singleQuestionVM.QuestionType = QuestionTypeEnum.QuizN;
                            singleQuestionVM.SelectedAnswer = new List<SingleQuestion_Bool_MultiChoices_ViewModel.AnswerPacket>();
                            singleQuestionVM.PossibleAnswerList = possibleAnswerList;

                            groupQuestionVM.SingleQuestionsList.Add(singleQuestionVM);
                        }
                    }
                    testGroup.QuestionsList.Add(groupQuestionVM);
                }
                #endregion

                #region Câu Gạch Chân - Underline question
                //testGroup.Underline = new QuestionPackage();

                //// lấy câu hỏi gạch chân đơn - get single underline
                //testGroup.Underline.ListOfSingles = new List<SingleQuestionViewModel>();
                for (int j = 0; j < testDataInJson["test"]["groups"][i]["underline"]["qSingle"].Count(); j++)
                {
                    SingleQuestion_String_OneChoice_ViewModel singleQuestionVM = new SingleQuestion_String_OneChoice_ViewModel();
                    singleQuestionVM.STT = stt.ToString();
                    stt++;
                    singleQuestionVM.ID = testDataInJson["test"]["groups"][i]["underline"]["qSingle"][j]["sID"]?.ToString();
                    singleQuestionVM.Score = testDataInJson["test"]["groups"][i]["underline"]["qSingle"][j]["sScore"]?.ToString();
                    singleQuestionVM.Content = testDataInJson["test"]["groups"][i]["underline"]["qSingle"][j]["sContain"]?.ToString();
                    singleQuestionVM.QuestionType = QuestionTypeEnum.Underline;
                    singleQuestionVM.PossibleAnswerList = new List<PossibleAnswerViewModel>();
                    singleQuestionVM.SelectedAnswer = "";
                    // lấy đáp án
                    int awStt = 1;
                    for (int k = 0; k < testDataInJson["test"]["groups"][i]["underline"]["qSingle"][j]["sAList"].Count(); k++)
                    {
                        PossibleAnswerViewModel answerVM = new PossibleAnswerViewModel();
                        answerVM.STT = awStt.ToString();
                        answerVM.ID = awStt.ToString();
                        awStt++;
                        answerVM.Content = testDataInJson["test"]["groups"][i]["underline"]["qSingle"][j]["sAList"][k]["sAContain"]?.ToString();
                        answerVM.RightAnswer = testDataInJson["test"]["groups"][i]["underline"]["qSingle"][j]["sAList"][k]["sAw"]?.ToString();
                        singleQuestionVM.PossibleAnswerList.Add(answerVM);
                    }
                    awStt = 1;
                    ShuffleList(singleQuestionVM.PossibleAnswerList);
                    foreach (var answer in singleQuestionVM.PossibleAnswerList)
                    {
                        answer.STT = awStt.ToString();
                        awStt++;
                    }
                    testGroup.QuestionsList.Add(singleQuestionVM);
                }

                // lấy câu hỏi gạch chân chùm - get group underline
                //testGroup.Underline.ListOfGroups = new List<GroupQuestionViewModel>();
                for (int l = 0; l < testDataInJson["test"]["groups"][i]["underline"]["qGroup"].Count(); l++)
                {
                    GroupQuestionViewModel groupQuestionVM = new GroupQuestionViewModel();
                    groupQuestionVM.Content = testDataInJson["test"]["groups"][i]["underline"]["qGroup"][l]["qgTitle"]?.ToString();
                    groupQuestionVM.SingleQuestionsList = new List<object>();
                    groupQuestionVM.QuestionType = QuestionTypeEnum.Group;

                    for (int j = 0; j < testDataInJson["test"]["groups"][i]["underline"]["qGroup"][l]["underlineSList"].Count(); j++)
                    {
                        SingleQuestion_String_OneChoice_ViewModel singleQuestionVM = new SingleQuestion_String_OneChoice_ViewModel();
                        singleQuestionVM.STT = stt.ToString();
                        stt++;
                        singleQuestionVM.ID = testDataInJson["test"]["groups"][i]["underline"]["qGroup"][l]["underlineSList"][j]["sID"]?.ToString();
                        singleQuestionVM.Score = testDataInJson["test"]["groups"][i]["underline"]["qGroup"][l]["underlineSList"][j]["sScore"]?.ToString();
                        singleQuestionVM.Content = testDataInJson["test"]["groups"][i]["underline"]["qGroup"][l]["underlineSList"][j]["sContain"]?.ToString();
                        singleQuestionVM.QuestionType = QuestionTypeEnum.Underline;
                        singleQuestionVM.PossibleAnswerList = new List<PossibleAnswerViewModel>();
                        singleQuestionVM.SelectedAnswer = "";
                        // lấy đáp án
                        int awStt = 1;
                        for (int k = 0; k < testDataInJson["test"]["groups"][i]["underline"]["qGroup"][l]["underlineSList"][j]["sAList"].Count(); k++)
                        {
                            PossibleAnswerViewModel answerVM = new PossibleAnswerViewModel();
                            answerVM.STT = awStt.ToString();
                            answerVM.ID = awStt.ToString();
                            awStt++;
                            answerVM.Content = testDataInJson["test"]["groups"][i]["underline"]["qGroup"][l]["underlineSList"][j]["sAList"][k]["sAContain"]?.ToString();
                            answerVM.RightAnswer = testDataInJson["test"]["groups"][i]["underline"]["qGroup"][l]["underlineSList"][j]["sAList"][k]["sAw"]?.ToString();
                            singleQuestionVM.PossibleAnswerList.Add(answerVM);
                        }
                        awStt = 1;
                        ShuffleList(singleQuestionVM.PossibleAnswerList);
                        foreach (var answer in singleQuestionVM.PossibleAnswerList)
                        {
                            answer.STT = awStt.ToString();
                            awStt++;
                        }
                        groupQuestionVM.SingleQuestionsList.Add(singleQuestionVM);
                    }
                    testGroup.QuestionsList.Add(groupQuestionVM);
                }
                #endregion

                #region Câu Điền Khuyết - Fill Question
                //testGroup.Fill = new QuestionPackage();

                // lấy câu hỏi điền khuyết đơn - get single fill
                //testGroup.Fill.ListOfSingles = new List<SingleQuestionViewModel>();
                for (int j = 0; j < testDataInJson["test"]["groups"][i]["fill"]["qSingle"].Count(); j++)
                {
                    SingleQuestion_String_MultiChoices_ViewModel singleQuestionVM = new SingleQuestion_String_MultiChoices_ViewModel();
                    singleQuestionVM.STT = stt.ToString();
                    stt++;
                    singleQuestionVM.ID = testDataInJson["test"]["groups"][i]["fill"]["qSingle"][j]["sID"]?.ToString();
                    singleQuestionVM.Score = testDataInJson["test"]["groups"][i]["fill"]["qSingle"][j]["sScore"]?.ToString();
                    singleQuestionVM.Content = testDataInJson["test"]["groups"][i]["fill"]["qSingle"][j]["sContain"]?.ToString();
                    singleQuestionVM.QuestionType = QuestionTypeEnum.Fill;
                    singleQuestionVM.PossibleAnswerList = new List<PossibleAnswerViewModel>();
                    singleQuestionVM.SelectedAnswer = new List<SingleQuestion_String_MultiChoices_ViewModel.AnswerPacket>();
                    // lấy đáp án
                    int awStt = 1;
                    for (int k = 0; k < testDataInJson["test"]["groups"][i]["fill"]["qSingle"][j]["sAList"].Count(); k++)
                    {
                        PossibleAnswerViewModel answerVM = new PossibleAnswerViewModel();
                        answerVM.STT = awStt.ToString();
                        answerVM.ID = awStt.ToString();
                        awStt++;
                        answerVM.Content = testDataInJson["test"]["groups"][i]["fill"]["qSingle"][j]["sAList"][k]["sAContain"]?.ToString();
                        answerVM.RightAnswer = testDataInJson["test"]["groups"][i]["fill"]["qSingle"][j]["sAList"][k]["sAw"]?.ToString();
                        singleQuestionVM.PossibleAnswerList.Add(answerVM);
                    }
                    awStt = 1;
                    ShuffleList(singleQuestionVM.PossibleAnswerList);
                    foreach (var answer in singleQuestionVM.PossibleAnswerList)
                    {
                        answer.STT = awStt.ToString();
                        awStt++;
                    }
                    testGroup.QuestionsList.Add(singleQuestionVM);
                }

                // lấy câu hỏi điền khuyết chùm - get group fill
                //testGroup.Fill.ListOfGroups = new List<GroupQuestionViewModel>();
                for (int l = 0; l < testDataInJson["test"]["groups"][i]["fill"]["qGroup"].Count(); l++)
                {
                    GroupQuestionViewModel groupQuestionVM = new GroupQuestionViewModel();
                    groupQuestionVM.Content = testDataInJson["test"]["groups"][i]["fill"]["qGroup"][l]["qgTitle"]?.ToString();
                    groupQuestionVM.SingleQuestionsList = new List<object>();
                    groupQuestionVM.QuestionType = QuestionTypeEnum.Group;

                    for (int j = 0; j < testDataInJson["test"]["groups"][i]["fill"]["qGroup"][l]["fillSList"].Count(); j++)
                    {
                        SingleQuestion_String_MultiChoices_ViewModel singleQuestionVM = new SingleQuestion_String_MultiChoices_ViewModel();
                        singleQuestionVM.STT = stt.ToString();
                        stt++;
                        singleQuestionVM.ID = testDataInJson["test"]["groups"][i]["fill"]["qGroup"][l]["fillSList"][j]["sID"]?.ToString();
                        singleQuestionVM.Score = testDataInJson["test"]["groups"][i]["fill"]["qGroup"][l]["fillSList"][j]["sScore"]?.ToString();
                        singleQuestionVM.Content = testDataInJson["test"]["groups"][i]["fill"]["qGroup"][l]["fillSList"][j]["sContain"]?.ToString();
                        singleQuestionVM.QuestionType = QuestionTypeEnum.Fill;
                        singleQuestionVM.PossibleAnswerList = new List<PossibleAnswerViewModel>();
                        singleQuestionVM.SelectedAnswer = new List<SingleQuestion_String_MultiChoices_ViewModel.AnswerPacket>();
                        // lấy đáp án
                        int awStt = 1;
                        for (int k = 0; k < testDataInJson["test"]["groups"][i]["fill"]["qGroup"][l]["fillSList"][j]["sAList"].Count(); k++)
                        {
                            PossibleAnswerViewModel answerVM = new PossibleAnswerViewModel();
                            answerVM.STT = awStt.ToString();
                            answerVM.ID = awStt.ToString();
                            awStt++;
                            answerVM.Content = testDataInJson["test"]["groups"][i]["fill"]["qGroup"][l]["fillSList"][j]["sAList"][k]["sAContain"]?.ToString();
                            answerVM.RightAnswer = testDataInJson["test"]["groups"][i]["fill"]["qGroup"][l]["fillSList"][j]["sAList"][k]["sAw"]?.ToString();
                            singleQuestionVM.PossibleAnswerList.Add(answerVM);
                        }
                        awStt = 1;
                        ShuffleList(singleQuestionVM.PossibleAnswerList);
                        foreach (var answer in singleQuestionVM.PossibleAnswerList)
                        {
                            answer.STT = awStt.ToString();
                            awStt++;
                        }
                        groupQuestionVM.SingleQuestionsList.Add(singleQuestionVM);
                    }
                    testGroup.QuestionsList.Add(groupQuestionVM);
                }
                #endregion

                #region Câu Đúng Sai - True False Question
                //testGroup.TrueFalse = new QuestionPackage();

                // lấy câu hỏi đúng sai đơn - get single trueFalse
                //testGroup.TrueFalse.ListOfSingles = new List<SingleQuestionViewModel>();
                for (int j = 0; j < testDataInJson["test"]["groups"][i]["trueFalse"]["qSingle"].Count(); j++)
                {
                    SingleQuestion_Bool_MultiChoices_ViewModel singleQuestionVM = new SingleQuestion_Bool_MultiChoices_ViewModel();
                    singleQuestionVM.STT = stt.ToString();
                    stt++;
                    singleQuestionVM.ID = testDataInJson["test"]["groups"][i]["trueFalse"]["qSingle"][j]["sID"]?.ToString();
                    singleQuestionVM.Score = testDataInJson["test"]["groups"][i]["trueFalse"]["qSingle"][j]["sScore"]?.ToString();
                    singleQuestionVM.Content = testDataInJson["test"]["groups"][i]["trueFalse"]["qSingle"][j]["sContain"]?.ToString();
                    singleQuestionVM.QuestionType = QuestionTypeEnum.TrueFalse;
                    singleQuestionVM.PossibleAnswerList = new List<PossibleAnswerViewModel>();
                    singleQuestionVM.SelectedAnswer = new List<SingleQuestion_Bool_MultiChoices_ViewModel.AnswerPacket>();

                    // lấy đáp án
                    int awStt = 1;
                    for (int k = 0; k < testDataInJson["test"]["groups"][i]["trueFalse"]["qSingle"][j]["sAList"].Count(); k++)
                    {
                        PossibleAnswerViewModel answerVM = new PossibleAnswerViewModel();
                        answerVM.STT = awStt.ToString();
                        answerVM.ID = awStt.ToString();
                        awStt++;
                        answerVM.Content = testDataInJson["test"]["groups"][i]["trueFalse"]["qSingle"][j]["sAList"][k]["sAContain"]?.ToString();
                        answerVM.RightAnswer = testDataInJson["test"]["groups"][i]["trueFalse"]["qSingle"][j]["sAList"][k]["sAw"]?.ToString();
                        singleQuestionVM.PossibleAnswerList.Add(answerVM);
                    }
                    awStt = 1;
                    ShuffleList(singleQuestionVM.PossibleAnswerList);
                    foreach (var answer in singleQuestionVM.PossibleAnswerList)
                    {
                        answer.STT = awStt.ToString();
                        awStt++;
                    }
                    testGroup.QuestionsList.Add(singleQuestionVM);
                }

                // lấy câu hỏi đúng sai chùm - get group trueFalse
                //testGroup.TrueFalse.ListOfGroups = new List<GroupQuestionViewModel>();
                for (int l = 0; l < testDataInJson["test"]["groups"][i]["trueFalse"]["qGroup"].Count(); l++)
                {
                    GroupQuestionViewModel groupQuestionVM = new GroupQuestionViewModel();
                    groupQuestionVM.Content = testDataInJson["test"]["groups"][i]["trueFalse"]["qGroup"][l]["qgTitle"]?.ToString();
                    groupQuestionVM.SingleQuestionsList = new List<object>();
                    groupQuestionVM.QuestionType = QuestionTypeEnum.Group;

                    for (int j = 0; j < testDataInJson["test"]["groups"][i]["trueFalse"]["qGroup"][l]["truefalseSList"].Count(); j++)
                    {
                        SingleQuestion_Bool_MultiChoices_ViewModel singleQuestionVM = new SingleQuestion_Bool_MultiChoices_ViewModel();
                        singleQuestionVM.STT = stt.ToString();
                        stt++;
                        singleQuestionVM.ID = testDataInJson["test"]["groups"][i]["trueFalse"]["qGroup"][l]["truefalseSList"][j]["sID"]?.ToString();
                        singleQuestionVM.Score = testDataInJson["test"]["groups"][i]["trueFalse"]["qGroup"][l]["truefalseSList"][j]["sScore"]?.ToString();
                        singleQuestionVM.Content = testDataInJson["test"]["groups"][i]["trueFalse"]["qGroup"][l]["truefalseSList"][j]["sContain"]?.ToString();
                        singleQuestionVM.QuestionType = QuestionTypeEnum.TrueFalse;
                        singleQuestionVM.PossibleAnswerList = new List<PossibleAnswerViewModel>();
                        singleQuestionVM.SelectedAnswer = new List<SingleQuestion_Bool_MultiChoices_ViewModel.AnswerPacket>();

                        // lấy đáp án
                        int awStt = 1;
                        for (int k = 0; k < testDataInJson["test"]["groups"][i]["trueFalse"]["qGroup"][l]["truefalseSList"][j]["sAList"].Count(); k++)
                        {
                            PossibleAnswerViewModel answerVM = new PossibleAnswerViewModel();
                            answerVM.STT = awStt.ToString();
                            answerVM.ID = awStt.ToString();
                            awStt++;
                            answerVM.Content = testDataInJson["test"]["groups"][i]["trueFalse"]["qGroup"][l]["truefalseSList"][j]["sAList"][k]["sAContain"]?.ToString();
                            answerVM.RightAnswer = testDataInJson["test"]["groups"][i]["trueFalse"]["qGroup"][l]["truefalseSList"][j]["sAList"][k]["sAw"]?.ToString();
                            singleQuestionVM.PossibleAnswerList.Add(answerVM);
                        }
                        awStt = 1;
                        ShuffleList(singleQuestionVM.PossibleAnswerList);
                        foreach (var answer in singleQuestionVM.PossibleAnswerList)
                        {
                            answer.STT = awStt.ToString();
                            awStt++;
                        }
                        groupQuestionVM.SingleQuestionsList.Add(singleQuestionVM);
                    }
                    testGroup.QuestionsList.Add(groupQuestionVM);
                }
                #endregion

                #region Câu Nối Chéo - Matching Question
                //testGroup.Matching = new QuestionPackage();

                // lấy câu hỏi nối chéo đơn - get single matching
                //testGroup.Matching.ListOfSingles = new List<SingleQuestionViewModel>();
                for (int j = 0; j < testDataInJson["test"]["groups"][i]["matching"]["qSingle"].Count(); j++)
                {
                    SingleQuestion_String_MultiChoices_ViewModel singleQuestionVM = new SingleQuestion_String_MultiChoices_ViewModel();
                    singleQuestionVM.STT = stt.ToString();
                    stt++;
                    singleQuestionVM.ID = testDataInJson["test"]["groups"][i]["matching"]["qSingle"][j]["sID"]?.ToString();
                    singleQuestionVM.Score = testDataInJson["test"]["groups"][i]["matching"]["qSingle"][j]["sScore"]?.ToString();
                    singleQuestionVM.Content = testDataInJson["test"]["groups"][i]["matching"]["qSingle"][j]["sContain"]?.ToString();
                    singleQuestionVM.QuestionType = QuestionTypeEnum.Matching;
                    singleQuestionVM.PossibleAnswerList = new List<PossibleAnswerViewModel>();
                    singleQuestionVM.SelectedAnswer = new List<SingleQuestion_String_MultiChoices_ViewModel.AnswerPacket>();

                    // lấy đáp án
                    int awStt = 1;
                    for (int k = 0; k < testDataInJson["test"]["groups"][i]["matching"]["qSingle"][j]["sAList"].Count(); k++)
                    {
                        PossibleAnswerViewModel answerVM = new PossibleAnswerViewModel();
                        answerVM.STT = awStt.ToString();
                        answerVM.ID = awStt.ToString();
                        awStt++;
                        answerVM.Content = testDataInJson["test"]["groups"][i]["matching"]["qSingle"][j]["sAList"][k]["sAContain"]?.ToString();
                        answerVM.RightAnswer = testDataInJson["test"]["groups"][i]["matching"]["qSingle"][j]["sAList"][k]["sAw"]?.ToString();
                        singleQuestionVM.PossibleAnswerList.Add(answerVM);
                    }
                    testGroup.QuestionsList.Add(singleQuestionVM);
                }

                // lấy câu hỏi nối chéo chùm - get group matching
                //testGroup.Matching.ListOfGroups = new List<GroupQuestionViewModel>();
                for (int l = 0; l < testDataInJson["test"]["groups"][i]["matching"]["qGroup"].Count(); l++)
                {
                    GroupQuestionViewModel groupQuestionVM = new GroupQuestionViewModel();
                    groupQuestionVM.Content = testDataInJson["test"]["groups"][i]["matching"]["qGroup"][l]["qgTitle"]?.ToString();
                    groupQuestionVM.SingleQuestionsList = new List<object>();
                    groupQuestionVM.QuestionType = QuestionTypeEnum.Group;

                    for (int j = 0; j < testDataInJson["test"]["groups"][i]["matching"]["qGroup"][l]["matchingSList"].Count(); j++)
                    {
                        SingleQuestion_String_MultiChoices_ViewModel singleQuestionVM = new SingleQuestion_String_MultiChoices_ViewModel();
                        singleQuestionVM.STT = stt.ToString();
                        stt++;
                        singleQuestionVM.ID = testDataInJson["test"]["groups"][i]["matching"]["qGroup"][l]["matchingSList"][j]["sID"]?.ToString();
                        singleQuestionVM.Score = testDataInJson["test"]["groups"][i]["matching"]["qGroup"][l]["matchingSList"][j]["sScore"]?.ToString();
                        singleQuestionVM.Content = testDataInJson["test"]["groups"][i]["matching"]["qGroup"][l]["matchingSList"][j]["sContain"]?.ToString();
                        singleQuestionVM.QuestionType = QuestionTypeEnum.Matching;
                        singleQuestionVM.PossibleAnswerList = new List<PossibleAnswerViewModel>();
                        singleQuestionVM.SelectedAnswer = new List<SingleQuestion_String_MultiChoices_ViewModel.AnswerPacket>();

                        // lấy đáp án
                        int awStt = 1;
                        for (int k = 0; k < testDataInJson["test"]["groups"][i]["matching"]["qGroup"][l]["matchingSList"][j]["sAList"].Count(); k++)
                        {
                            PossibleAnswerViewModel answerVM = new PossibleAnswerViewModel();
                            answerVM.STT = awStt.ToString();
                            answerVM.ID = awStt.ToString();
                            awStt++;
                            answerVM.Content = testDataInJson["test"]["groups"][i]["matching"]["qGroup"][l]["matchingSList"][j]["sAList"][k]["sAContain"]?.ToString();
                            answerVM.RightAnswer = testDataInJson["test"]["groups"][i]["matching"]["qGroup"][l]["matchingSList"][j]["sAList"][k]["sAw"]?.ToString();
                            singleQuestionVM.PossibleAnswerList.Add(answerVM);
                        }
                        groupQuestionVM.SingleQuestionsList.Add(singleQuestionVM);
                    }
                    testGroup.QuestionsList.Add(groupQuestionVM);
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
                result += "#" + testgroup.Title + "!" + (testgroup.isForAll ? "1" : "0") + "@";
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
                                        result += ((SingleQuestionViewModel)questionChild).Score.ToString() + ")" + stt.ToString();
                                        foreach (var answer in ((SingleQuestionViewModel)questionChild).PossibleAnswerList)
                                        {
                                            if (bool.Parse(answer.RightAnswer))
                                            {
                                                result += "-" + positionOneChoiceG.ToString();
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
                                        result += ((SingleQuestionViewModel)questionChild).Score.ToString() + ")" + stt.ToString();
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
                                        result += ((SingleQuestionViewModel)questionChild).Score.ToString() + ")" + stt.ToString();
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
                            result += ((SingleQuestionViewModel)question).Score.ToString() + ")" + stt.ToString();
                            foreach (var answer in ((SingleQuestionViewModel)question).PossibleAnswerList)
                            {
                                if (bool.Parse(answer.RightAnswer))
                                {
                                    result += "-" + positionOneChoice.ToString();
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
                            result += ((SingleQuestionViewModel)question).Score.ToString() + ")" + stt.ToString();
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
                            result += ((SingleQuestionViewModel)question).Score.ToString() + ")" + stt.ToString();
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
                            result += ((SingleQuestionViewModel)question).Score.ToString() + ")" + stt.ToString();
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
            #endregion


            #region Inits Test Info
            var testDataMixed_json = JsonConvert.SerializeObject(testGroupList);
            var testDataMixed_json_base64 = Base64Encode(testDataMixed_json);
            result = Base64Encode(result);

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
                Base64Data = testDataMixed_json_base64,
                TestGroupList = testGroupList
            };
            #endregion

            #region Backup lần đầu thông tin bài làm            
            List<string> studentAnswersList = new List<string>(); //đáp án của thí sinh            
            TestResult testResult = new TestResult();
            testResult.TestDataID = Session["TestDataId"].ToString();
            testResult.StudentName = Session["FullName"].ToString();
            testResult.StudentClass = Session["Class"].ToString();
            testResult.StudentIPAdd = IP;
            testResult.TestGroupChoose = "";
            testResult.StudentTestResult = studentAnswersList;
            testResult.StudentTestData = testDataMixed_json;
            testResult.Score = -1;
            testResult.RecordDateTime = DateTime.Now;
            _StudentLogic = new StudentLogic();
            string testResultID = _StudentLogic.SaveTestResult(testResult);
            Session["TestResultID"] = testResultID;

            HttpCookie cookie = new HttpCookie(STUDENT_TEST_RESULT_ID_COOKIE_NAME);
            cookie.Value = testResultID;
            this.ControllerContext.HttpContext.Response.Cookies.Add(cookie);
            #endregion

            return View(viewModel);
        }

        /// <summary>
        /// Hoàn thành bài thi
        /// Chấm điểm và lưu xuống csdl
        /// </summary>
        /// <param name="formCollection"></param>
        /// <returns>Trả về trang FinishTest</returns>
        public ActionResult DoneTest(FormCollection formCollection)
        {
            double score = 0; // điểm thi
            List<string> studentAnswersList = new List<string>(); //đáp án của thí sinh

            var Base64Code = formCollection["Base64Code"];// đáp án
            var Base64Data = Base64Decode(formCollection["Base64Data"]);// đề thi

            var testGroupHints = Base64Decode(Base64Code).Split(new char[] { '#' }, StringSplitOptions.RemoveEmptyEntries);
            for (int groupID = 0; groupID < testGroupHints.Length; groupID++)
            {
                //lấy thông tin phần thi và dữ liệu: [0] là thong tin, [1] là dữ liệu
                var groupParts = testGroupHints[groupID].Split('@');

                //chia thông tin phần thi thành tên và kiểu thi (1 là chung, 0 là riêng)
                var groupInfos = groupParts[0].Split('!');

                //kiểm tra chọn phần thi của HS để tính điểm ("1" là phần chung hoặc "TestGroupChoose" là phần được chọn)
                if (groupInfos[1].Equals("1") || groupInfos[0].Equals(formCollection["TestGroupChoose"]))
                {
                    var hints = groupParts[1].Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < hints.Length; i++)
                    {
                        var questionSTT = hints[i].Split(new char[] { '-' }, 2, StringSplitOptions.RemoveEmptyEntries)[0].Split(new char[] { ')' })[1];
                        var studentAnswerValue = formCollection["Answer-" + questionSTT];
                        if (studentAnswerValue == null)
                        {

                            studentAnswersList.Add("Answer-" + questionSTT + "-NULL");
                        }
                        else
                        {
                            var hintSergments = hints[i].Split('-');

                            //kiểm tra nếu có "+" là câu fill hoặc matching, không có thì là còn lại
                            if (hints[i].Contains("+"))
                            {
                                var answerSergments = new string[] { "" };
                                if (studentAnswerValue.Contains(","))
                                    answerSergments = studentAnswerValue.Split(',');
                                else
                                    answerSergments = new string[] { studentAnswerValue };

                                studentAnswersList.Add("Answer-" + questionSTT + "-Fill_Matching+" + studentAnswerValue);

                                for (int answerID = 1; answerID < hintSergments.Length; answerID++)
                                {
                                    var studentAnswerPack = hintSergments[answerID].Split('+');

                                    bool isRight = true;
                                    if (!studentAnswerPack[1].Equals(answerSergments[int.Parse(studentAnswerPack[0]) - 1]))
                                    {
                                        isRight = false;
                                        break;
                                    }
                                    if (isRight)
                                    {
                                        //Nếu có điểm thi
                                        if (hintSergments[0].Contains(")"))
                                        {
                                            score += double.Parse(hintSergments[0].Split(')')[0]);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                int numberOfAnswers = hintSergments.Length;
                                //nếu hơn 2 là câu nhiều lựa chọn hoặc đúng sai
                                if (numberOfAnswers > 2)
                                {
                                    var hintsdetail = hints[i].Split(new char[] { '-' }, 2);

                                    var answerSergments = new string[] { "" };
                                    if (studentAnswerValue.Contains(","))
                                        answerSergments = studentAnswerValue.Split(',');
                                    else
                                        answerSergments = new string[] { studentAnswerValue };

                                    studentAnswersList.Add("Answer-" + questionSTT + "-QuizN_TrueFalse+" + studentAnswerValue);

                                    bool isRight = true;
                                    for (int answerID = 0; answerID < answerSergments.Length; answerID++)
                                    {
                                        if (!hintsdetail[1].Contains(answerSergments[answerID]))
                                        {
                                            isRight = false;
                                            break;
                                        }
                                        //answerSergments[answerID]
                                    }
                                    if (isRight)
                                    {
                                        //Nếu có điểm thi
                                        if (hintSergments[0].Contains(")"))
                                        {
                                            score += double.Parse(hintsdetail[0].Split(')')[0]);
                                        }
                                    }
                                }
                                else //1 lựa chọn hoặc gạch chân
                                {
                                    var answerSergments = new string[] { "" };
                                    if (studentAnswerValue.Contains(","))
                                        answerSergments = studentAnswerValue.Split(',');
                                    else
                                        answerSergments = new string[] { studentAnswerValue };

                                    if (answerSergments.Length <= numberOfAnswers)
                                    {
                                        studentAnswersList.Add("Answer-" + questionSTT + "-Quiz_UnderLine+" + studentAnswerValue);
                                        if (hintSergments[1].Contains(studentAnswerValue))
                                        {
                                            //Nếu có điểm thi
                                            if (hintSergments[0].Contains(")"))
                                            {
                                                score += double.Parse(hintSergments[0].Split(')')[0]);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        studentAnswersList.Add("Answer-" + questionSTT + "-TrueFalse+" + studentAnswerValue);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            var testResultID_Session = Session["TestResultID"];
            if (testResultID_Session != null)
            {
                var testResrultID = testResultID_Session.ToString();
                TestResult testResult = new TestResult();
                testResult.TestDataID = Session["TestDataId"].ToString();
                testResult.StudentName = Session["FullName"].ToString();
                testResult.StudentClass = Session["Class"].ToString();
                testResult.StudentIPAdd = formCollection["StudentIPAdd"];
                testResult.TestGroupChoose = formCollection["TestGroupChoose"] != null ? formCollection["TestGroupChoose"] : "";
                testResult.StudentTestResult = studentAnswersList;
                testResult.StudentTestData = Base64Data;
                testResult.Score = score;
                testResult.RecordDateTime = DateTime.Now;
                _StudentLogic = new StudentLogic();
                _StudentLogic.UpdateTestResult(testResult, testResrultID);
            }
            Session["Score"] = score.ToString();

            return RedirectToAction("FinishTest");
        }

        /// <summary>
        /// Mỗi lần thí sinh chọn đáp án là update backup lại
        /// </summary>
        /// <param name="formCollection"></param>
        public void BackupTest(FormCollection formCollection)
        {
            List<string> studentAnswersList = new List<string>(); //đáp án của thí sinh

            var Base64Code = formCollection["Base64Code"];// đáp án
            var Base64Data = Base64Decode(formCollection["Base64Data"]);// đề thi

            var testGroupHints = Base64Decode(Base64Code).Split(new char[] { '#' }, StringSplitOptions.RemoveEmptyEntries);
            for (int groupID = 0; groupID < testGroupHints.Length; groupID++)
            {
                //lấy thông tin phần thi và dữ liệu: [0] là thong tin, [1] là dữ liệu
                var groupParts = testGroupHints[groupID].Split('@');

                //chia thông tin phần thi thành tên và kiểu thi (1 là chung, 0 là riêng)
                var groupInfos = groupParts[0].Split('!');

                //kiểm tra chọn phần thi của HS để tính điểm ("1" là phần chung hoặc "TestGroupChoose" là phần được chọn)
                if (groupInfos[1].Equals("1") || groupInfos[0].Equals(formCollection["TestGroupChoose"]))
                {
                    var hints = groupParts[1].Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < hints.Length; i++)
                    {
                        var questionSTT = hints[i].Split(new char[] { '-' }, 2, StringSplitOptions.RemoveEmptyEntries)[0].Split(new char[] { ')' })[1];
                        var studentAnswerValue = formCollection["Answer-" + questionSTT];
                        if (studentAnswerValue == null)
                        {
                            studentAnswersList.Add("Answer-" + questionSTT + "-NULL");
                        }
                        else
                        {
                            studentAnswersList.Add("Answer-" + questionSTT + "+" + studentAnswerValue);
                        }
                    }
                }
            }

            var testResultID_Session = Session["TestResultID"];
            if (testResultID_Session != null)
            {
                var testResrultID = testResultID_Session.ToString();
                TestResult testResult = new TestResult();
                testResult.TestDataID = Session["TestDataId"].ToString();
                testResult.StudentName = Session["FullName"].ToString();
                testResult.StudentClass = Session["Class"].ToString();
                testResult.StudentIPAdd = formCollection["StudentIPAdd"];
                testResult.TestGroupChoose = formCollection["TestGroupChoose"] != null ? formCollection["TestGroupChoose"] : "";
                testResult.StudentTestResult = studentAnswersList;
                testResult.StudentTestData = Base64Data;
                testResult.Score = -1;
                testResult.RecordDateTime = DateTime.Now;
                _StudentLogic = new StudentLogic();
                _StudentLogic.UpdateTestResult(testResult, testResrultID);
            }
        }


        /// <summary>
        /// Hoàn thành bài thi
        /// Chạy ra giao điện kết quả
        /// </summary>
        /// <returns></returns>
        public ActionResult FinishTest(FormCollection formCollection)
        {
            var testDataId = Session["TestDataId"];

            if (testDataId == null)
            {
                return RedirectToAction("WaitingScreen");
            }
            _StudentLogic = new StudentLogic();
            var state = _StudentLogic.GetTestStep(testDataId.ToString());

            if (state == TestStepEnum.Finish)
            {
                removeStudentInfoCookie();
                Session["FullName"] = null;
                Session["Class"] = null;
                Session["Score"] = null;
                Session["TestDataId"] = null;
            }
            else
            {
                ViewBag.StudentName = Session["FullName"].ToString();
                ViewBag.ClassName = Session["Class"].ToString();
                ViewBag.Score = Session["Score"].ToString();
            }
            return View();
        }

        /// <summary>
        /// Keep session alive
        /// </summary>
        public void KeepSession()
        {
            if (Session["FullName"] != null)
                Session["FullName"] = Session["FullName"];

            if (Session["Class"] != null)
                Session["Class"] = Session["Class"];

            if (Session["Score"] != null)
                Session["Score"] = Session["Score"];

            if (Session["TestDataId"] != null)
                Session["TestDataId"] = Session["TestDataId"];
        }

        /// <summary>
        /// Nếu trang làm bài bị lose focus thì hủy bài thi
        /// </summary>
        /// <returns></returns>
        public ActionResult OnLoseFocus()
        {
            return RedirectToAction("JoinTest");
        }

        /// <summary>
        /// lấy cookie thông tin học sinh
        /// </summary>
        /// <returns>StudentInfoCookieModel</returns>
        private StudentInfoCookieModel getStudentInfoCookie()
        {
            if (this.ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains(STUDENT_INFO_COOKIE_NAME))
            {
                StudentInfoCookieModel studentIC = new StudentInfoCookieModel();
                HttpCookie cookie = this.ControllerContext.HttpContext.Request.Cookies[STUDENT_INFO_COOKIE_NAME];
                string[] values = cookie.Value.Split('|');
                if (values.Length == 4)
                {
                    studentIC.Name = values[0];
                    studentIC.Class = values[1];
                    studentIC.Score = values[2];
                    studentIC.TestDataID = values[3];
                }
                else
                    return null;

                return studentIC;
            }
            return null;
        }

        /// <summary>
        /// lưu cookie thông tin học sinh
        /// </summary>
        /// <param name="studentIC"></param>
        private void saveStudentInfoCookie(StudentInfoCookieModel studentIC)
        {
            HttpCookie cookie = new HttpCookie(STUDENT_INFO_COOKIE_NAME);
            cookie.Value = studentIC.Name + "|" + studentIC.Class + "|" + studentIC.Score + "|" + studentIC.TestDataID;
            this.ControllerContext.HttpContext.Response.Cookies.Add(cookie);
        }

        /// <summary>
        /// xóa cookie thông tin học sinh
        /// </summary>
        private void removeStudentInfoCookie()
        {
            if (this.ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains(STUDENT_INFO_COOKIE_NAME))
            {
                HttpCookie cookie = this.ControllerContext.HttpContext.Request.Cookies[STUDENT_INFO_COOKIE_NAME];
                cookie.Expires = DateTime.Now.AddDays(-1);
                this.ControllerContext.HttpContext.Response.Cookies.Add(cookie);
            }
        }

        /// <summary>
        /// Thead lấy random int an toàn
        /// </summary>
        private class ThreadSafeRandom
        {
            [ThreadStatic]
            private static Random Local;

            public static Random ThisThreadsRandom
            {
                get { return Local ?? (Local = new Random(unchecked(Environment.TickCount * 31 + System.Threading.Thread.CurrentThread.ManagedThreadId))); }
            }
        }

        /// <summary>
        /// Trộn random List<object>
        /// </summary>
        /// <param name="list"></param>
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

        /// <summary>
        /// Trộn random List<PossibleAnswerViewModel>
        /// </summary>
        /// <param name="list"></param>
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

        /// <summary>
        /// Mã hóa Base 64
        /// </summary>
        /// <param name="plainText"></param>
        /// <returns></returns>
        private string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        /// <summary>
        /// Giải mã Base 64
        /// </summary>
        /// <param name="base64EncodedData"></param>
        /// <returns></returns>
        private string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

    }
}