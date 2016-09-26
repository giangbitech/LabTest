﻿using BiTech.LabTest.BLL;
using BiTech.LabTest.DAL.Models;
using BiTech.LabTest.Models.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using static BiTech.LabTest.DAL.Models.TestData;
using static BiTech.LabTest.Models.ViewModels.Student;
using static BiTech.LabTest.BLL.Tool;
using static BiTech.LabTest.DataObject.ViewModels.StudentViewModel;

namespace BiTech.LabTest.Controllers
{
    public class StudentController : Controller
    {
        public StudentLogic _StudentLogic { get; set; }

        public StudentTestStepEnum CurrentStudentTestStep { get; set; }

        private const string STUDENT_SESSION = "studentbaseinfo";

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
            _StudentLogic = new StudentLogic();

            StudentBaseInfo studentBaseInfo = (StudentBaseInfo)Session[STUDENT_SESSION];

            string testResultID = "";
            studentBaseInfo = CheckReconnection(Request.UserHostAddress);
            if (studentBaseInfo != null)
            {
                Session[STUDENT_SESSION] = studentBaseInfo;
                testResultID = studentBaseInfo?.TestResultID == null ? "" : studentBaseInfo.TestResultID;
                if (studentBaseInfo.Score > -1)
                {
                    return RedirectToAction("FinishTest");
                }
            }

            if (studentBaseInfo == null)
            {
                return View();
            }

            //Kiểm tra trạng thái phòng thi có cho phép thí sinh thi hay không
            if (!string.IsNullOrEmpty(testResultID))
            {
                var teststep = _StudentLogic.GetTestStep(testResultID.ToString());
                switch (teststep)
                {
                    case TestStepEnum.Finish:
                        return RedirectToAction("JoinTest");
                    case TestStepEnum.OnWorking:
                        // Nếu đang thi mà thí sinh khong có tên thì không được thi.
                        if (string.IsNullOrEmpty(studentBaseInfo.StudentName) || string.IsNullOrEmpty(studentBaseInfo.StudentClass))
                        {
                            return RedirectToAction("HoldingScreen");
                        }
                        return RedirectToAction("DoTest");
                    case TestStepEnum.Waiting:
                        // Kiểm tra nhập thông tin thí sinh
                        if (!string.IsNullOrEmpty(changeName) || (string.IsNullOrEmpty(studentBaseInfo.StudentName) || string.IsNullOrEmpty(studentBaseInfo.StudentClass)))
                        {
                            studentBaseInfo.StudentName = studentBaseInfo.StudentClass = "";
                            Session[STUDENT_SESSION] = studentBaseInfo;
                            return View();
                        }
                        return RedirectToAction("WaitingScreen");
                }
            }
            if (string.IsNullOrEmpty(studentBaseInfo.StudentName) || string.IsNullOrEmpty(studentBaseInfo.StudentClass))
            {
                return View();
            }
            return RedirectToAction("WaitingScreen");
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
            StudentBaseInfo studentBaseInfox = CheckReconnection(Request.UserHostAddress);
            if (studentBaseInfox == null)
            {
                if (studentBaseInfox.Score > -1)
                {
                    return RedirectToAction("FinishTest");
                }
            }
            _StudentLogic = new StudentLogic();
            StudentBaseInfo studentBaseInfo = (StudentBaseInfo)Session[STUDENT_SESSION];
            var testDataId = studentBaseInfo?.TestDataID;

            TestStepEnum teststep = TestStepEnum.Waiting;
            if (!string.IsNullOrEmpty(testDataId))
                teststep = _StudentLogic.GetTestStep(testDataId.ToString());

            //Nếu kết thúc rồi thì không cho thi
            if (teststep == TestStepEnum.Finish)
                return RedirectToAction("FinishTest");

            if (ModelState.IsValid)
            {
                if (studentBaseInfo == null)
                {
                    studentBaseInfo = new StudentBaseInfo();
                    studentBaseInfo.StudentName = viewModel.FullName;
                    studentBaseInfo.StudentClass = viewModel.Class;
                }

                Session[STUDENT_SESSION] = studentBaseInfo;
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
            _StudentLogic = new StudentLogic();

            StudentBaseInfo studentBaseInfo = (StudentBaseInfo)Session[STUDENT_SESSION];


            var testDataId = studentBaseInfo?.TestDataID;
            TestStepEnum teststep = TestStepEnum.Waiting;
            if (!string.IsNullOrEmpty(testDataId))
                teststep = _StudentLogic.GetTestStep(testDataId.ToString());

            //Nếu kết thúc rồi thì không cho thi
            if (teststep == TestStepEnum.Finish)
                return RedirectToAction("FinishTest");

            // Chưa nhập thông tin thí sinh

            if ((studentBaseInfo?.StudentName == null || studentBaseInfo?.StudentClass == null)
                && teststep == TestStepEnum.Waiting)
            {
                return RedirectToAction("JoinTest");
            }

            //Đã bắt đầu thi thì vào thi luôn
            if (teststep == TestStepEnum.OnWorking)
            {
                return RedirectToAction("DoTest");
            }

            ViewBag.StudentName = studentBaseInfo?.StudentName;
            ViewBag.ClassName = studentBaseInfo?.StudentClass;
            ViewBag.StudentIPAdd = Request.UserHostAddress;


            StudentBaseInfo studentBaseInfox = CheckReconnection(Request.UserHostAddress);
            if (studentBaseInfox == null)
            {
                if (studentBaseInfox.Score > -1)
                {
                    return RedirectToAction("FinishTest");
                }
                List<string> studentAnswersList = new List<string>(); //đáp án của thí sinh            
                TestResult testResult = new TestResult();
                testResult.TestDataID = studentBaseInfo.TestDataID;
                testResult.StudentName = studentBaseInfo.StudentName;
                testResult.StudentClass = studentBaseInfo.StudentClass;
                testResult.StudentIPAdd = Request.UserHostAddress;
                testResult.TestGroupChoose = "";
                testResult.StudentTestResult = studentAnswersList;
                testResult.StudentTestData = "";
                testResult.TestHints = "";
                testResult.Score = -1;
                testResult.RecordDateTime = DateTime.Now;
                _StudentLogic = new StudentLogic();
                string testResultID = _StudentLogic.SaveTestResultTemp(testResult);
                studentBaseInfo.TestResultID = testResultID;
            }

            Session[STUDENT_SESSION] = studentBaseInfo;
            return View();
        }

        /// <summary>
        /// Màn hình chờ hết kì thi khác
        /// </summary>
        /// <returns></returns>
        public ActionResult HoldingScreen()
        {
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
            StudentBaseInfo studentBaseInfox = CheckReconnection(Request.UserHostAddress);
            if (studentBaseInfox == null)
            {
                if (studentBaseInfox.Score > -1)
                {
                    return RedirectToAction("FinishTest");
                }
            }
            _StudentLogic = new StudentLogic();
            StudentBaseInfo studentBaseInfo = (StudentBaseInfo)Session[STUDENT_SESSION];
            TestStepEnum teststep = TestStepEnum.Waiting;
            //Nếu có điểm rồi không cho thi lại
            if (!string.IsNullOrEmpty(testDataId))
            {
                teststep = _StudentLogic.GetTestStep(testDataId.ToString());
                //Nếu kết thúc rồi thì không cho thi
                if (teststep == TestStepEnum.Finish)
                    return RedirectToAction("FinishTest");

                studentBaseInfo.TestDataID = testDataId;
                studentBaseInfo.StudentIPAdd = Request.UserHostAddress;

                TestResult testResult = new TestResult();
                testResult.TestDataID = studentBaseInfo.TestDataID;
                testResult.StudentName = studentBaseInfo.StudentName;
                testResult.StudentClass = studentBaseInfo.StudentClass;
                testResult.StudentIPAdd = studentBaseInfo.StudentIPAdd;
                testResult.TestGroupChoose = "";
                testResult.StudentTestResult = null;
                testResult.StudentTestData = "";
                testResult.TestHints = "";
                testResult.Score = -1;
                testResult.RecordDateTime = DateTime.Now;
                _StudentLogic = new StudentLogic();
                _StudentLogic.UpdateTestResultTemp(testResult, studentBaseInfo.TestResultID);

                Session[STUDENT_SESSION] = studentBaseInfo;
                return Json(new { testDataId = testDataId });
            }
            return RedirectToAction("JoinTest");
        }

        /// <summary>
        /// Lấy đề thi từ csdl và xuất ra view
        /// </summary>
        /// <returns>View của DoTest</returns>
        public ActionResult DoTest()
        {
            StudentBaseInfo studentBaseInfo = CheckReconnection(Request.UserHostAddress);
            bool isReconnected = false;
            _StudentLogic = new StudentLogic();
            if (studentBaseInfo == null)
            {
                isReconnected = false;
                studentBaseInfo = (StudentBaseInfo)Session[STUDENT_SESSION];
            }
            else
            {
                if (studentBaseInfo.Score > -1)
                {
                    return RedirectToAction("FinishTest");
                }
                isReconnected = true;
            }

            var testDataId = studentBaseInfo?.TestDataID;
            TestStepEnum teststep = TestStepEnum.Waiting;
            if (!string.IsNullOrEmpty(testDataId))
                teststep = _StudentLogic.GetTestStep(testDataId.ToString());

            //Nếu kết thúc rồi thì không cho thi
            if (teststep == TestStepEnum.Finish)
                return RedirectToAction("FinishTest");

            if (studentBaseInfo?.StudentName == null || studentBaseInfo?.StudentClass == null)
                return RedirectToAction("JoinTest");

            _StudentLogic = new StudentLogic();

            //Nếu chưa có Test Data Id thì vào đăng ký lại
            if (studentBaseInfo?.TestDataID == null)
            {
                return RedirectToAction("JoinTest");
            }

            ViewBag.TestDataID = testDataId;
            string IP = Request.UserHostAddress;

            //string dethiodangjson = "";
            //ViewBag.Data = Json(new { group = "gg", ddd = "dd" });

            //todo: Chinh lai Lay tu database
            //StreamReader steamReader = new StreamReader(Server.MapPath("~/Content/data.json"));
            //CurrentStudentTestStep = StudentTestStepEnum.DoingTest;
            //var testDataInJson = JObject.Parse(steamReader.ReadToEnd());

            #region Parse cac cau hoi de xuat ra man hinh
            var testGroupList = new List<TestGroupViewModel>();
            string hints = "";
            StudentTestDataLogic studentTestDataLogic = new StudentTestDataLogic();

            TestResult testResultBackup = _StudentLogic.GetTestResultTempByID(studentBaseInfo?.TestResultID);
            if (isReconnected && !string.IsNullOrEmpty(testResultBackup.StudentTestData))
            {

                testGroupList = studentTestDataLogic.LoadTestBackup(testResultBackup);
                if (testGroupList == null)
                {
                    return RedirectToAction("JoinTest");
                }
                else
                {
                    hints = testResultBackup.TestHints;
                }
            }
            else
            {
                testGroupList = studentTestDataLogic.LoadTestById(testDataId);

            }
            #endregion

            #region To String All Right Answers
            if (!(isReconnected && !string.IsNullOrEmpty(testResultBackup.StudentTestData)))
            {
                hints = Base64Encode(studentTestDataLogic.LoadHints(testGroupList));
            }
            #endregion

            #region Inits Test Info
            var testDataMixed_json = JsonConvert.SerializeObject(testGroupList);
            var testDataMixed_json_base64 = Base64Encode(testDataMixed_json);
            TestBaseInfo testbaseInfo = studentTestDataLogic.LoadTestBaseInfo(testDataId);

            var viewModel = new TestDataViewModel
            {
                TestInfo = testbaseInfo,
                StudentName = studentBaseInfo.StudentName,
                StudentClass = studentBaseInfo.StudentClass,
                StudentIPAdd = IP,
                TestGroupChoose = "",
                Base64Code = hints,
                Base64Data = testDataMixed_json_base64,
                TestGroupList = testGroupList
            };
            #endregion

            #region Backup lần đầu thông tin bài làm        
            if (string.IsNullOrEmpty(testResultBackup.StudentTestData))
            {
                List<string> studentAnswersList = new List<string>(); //đáp án của thí sinh    
                TestResult testResult = new TestResult();
                testResult.TestDataID = studentBaseInfo.TestDataID;
                testResult.StudentName = studentBaseInfo.StudentName;
                testResult.StudentClass = studentBaseInfo.StudentClass;
                testResult.StudentIPAdd = IP;
                testResult.TestGroupChoose = "";
                testResult.StudentTestResult = studentAnswersList;
                testResult.StudentTestData = testDataMixed_json;
                testResult.TestHints = hints;
                testResult.Score = -1;
                testResult.RecordDateTime = DateTime.Now;
                _StudentLogic = new StudentLogic();
                _StudentLogic.UpdateTestResultTemp(testResult, studentBaseInfo.TestResultID);

                Session[STUDENT_SESSION] = studentBaseInfo;
            }
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
            StudentBaseInfo studentBaseInfox = CheckReconnection(Request.UserHostAddress);
            if (studentBaseInfox == null)
            {
                if (studentBaseInfox.Score > -1)
                {
                    return RedirectToAction("FinishTest");
                }
            }
            System.Collections.Specialized.NameValueCollection nameValues = formCollection;
            StudentBaseInfo studentBaseInfo = (StudentBaseInfo)Session[STUDENT_SESSION];
            studentBaseInfo.Score = new StudentTestDataLogic().ReviewStudentTestResult(nameValues, studentBaseInfo);
            Session[STUDENT_SESSION] = studentBaseInfo;
            return RedirectToAction("FinishTest");
        }

        /// <summary>
        /// Mỗi lần thí sinh chọn đáp án là update backup lại
        /// </summary>
        /// <param name="formCollection"></param>
        public void BackupTest(FormCollection formCollection)
        {
            StudentBaseInfo studentBaseInfo = (StudentBaseInfo)Session[STUDENT_SESSION];
            List<string> studentAnswersList = new List<string>(); //đáp án của thí sinh

            var Base64Code = formCollection["Base64Code"];// đáp án
            var Base64Data = Base64Decode(formCollection["Base64Data"]);// đề thi

            #region Lấy câu trả lời ra List
            var testGroupHints = Base64Decode(Base64Code).Split(new char[] { '#' }, StringSplitOptions.RemoveEmptyEntries);
            for (int groupID = 0; groupID < testGroupHints.Length; groupID++)
            {
                //lấy thông tin phần thi và dữ liệu: [0] là thong tin, [1] là dữ liệu
                var groupParts = testGroupHints[groupID].Split('@');

                //chia thông tin phần thi thành tên và kiểu thi (1 là chung, 0 là riêng)
                var groupInfos = groupParts[0].Split('!');

                //kiểm tra chọn phần thi của HS để tính điểm ("1" là phần chung hoặc "TestGroupChoose" là phần được chọn)

                var hints = groupParts[1].Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < hints.Length; i++)
                {
                    var questionSTT = hints[i].Split(new char[] { '-' }, 2, StringSplitOptions.RemoveEmptyEntries)[0].Split(new char[] { ')' })[1];
                    var studentAnswerValue = formCollection["Answer-" + questionSTT];
                    if (studentAnswerValue == null)
                    {
                        studentAnswersList.Add("Answer-" + questionSTT + "+NULL");
                    }
                    else
                    {
                        studentAnswersList.Add("Answer-" + questionSTT + "+" + studentAnswerValue);
                    }
                }
            }
            #endregion

            #region Lưu kết quả tạm vào csdl
            if (studentBaseInfo?.TestResultID != null)
            {
                TestResult testResult = new TestResult();
                testResult.TestDataID = studentBaseInfo.TestDataID;
                testResult.StudentName = studentBaseInfo.StudentName;
                testResult.StudentClass = studentBaseInfo.StudentClass;
                testResult.StudentIPAdd = formCollection["StudentIPAdd"];
                testResult.TestGroupChoose = formCollection["TestGroupChoose"] != null ? formCollection["TestGroupChoose"] : "";
                testResult.StudentTestResult = studentAnswersList;
                testResult.StudentTestData = Base64Data;
                testResult.TestHints = Base64Code;
                testResult.Score = -1;
                testResult.RecordDateTime = DateTime.Now;
                _StudentLogic = new StudentLogic();
                _StudentLogic.UpdateTestResultTemp(testResult, studentBaseInfo.TestResultID);
            }
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="testResultID"></param>
        /// <returns></returns>
        public ActionResult GetBackupTestResult()
        {
            _StudentLogic = new StudentLogic();
            var testResult = _StudentLogic.GetTestResultTempByIp(Request.UserHostAddress);
            if (testResult == null)
            {
                return Json(new string[] { "NO" });
            }
            if (testResult.StudentTestResult == null)
            {
                return Json(new string[] { "NO" });
            }
            testResult.StudentTestResult.Add(testResult.TestGroupChoose);
            var ret = Json(testResult.StudentTestResult);
            return ret;
        }

        /// <summary>
        /// Hoàn thành bài thi
        /// Chạy ra giao điện kết quả
        /// </summary>
        /// <returns></returns>
        public ActionResult FinishTest(FormCollection formCollection)
        {
            StudentBaseInfo studentBaseInfo = (StudentBaseInfo)Session[STUDENT_SESSION];
            if (studentBaseInfo == null)
            {
                return RedirectToAction("JoinTest");
            }
            var testDataId = studentBaseInfo.TestDataID;

            if (testDataId == null)
            {
                return RedirectToAction("WaitingScreen");
            }
            _StudentLogic = new StudentLogic();
            var state = _StudentLogic.GetTestStep(testDataId.ToString());

            if (state == TestStepEnum.Finish)
            {
                Session[STUDENT_SESSION] = null;
            }
            else
            {
                ViewBag.StudentName = studentBaseInfo.StudentName;
                ViewBag.ClassName = studentBaseInfo.StudentClass;
                ViewBag.Score = studentBaseInfo.Score.ToString();
            }
            return View();
        }

        /// <summary>
        /// Keep session alive
        /// </summary>
        public void KeepSession()
        {
            if (Session[STUDENT_SESSION] != null)
                Session[STUDENT_SESSION] = Session[STUDENT_SESSION];
        }

        /// <summary>
        /// Nếu trang làm bài bị lose focus thì hủy bài thi
        /// </summary>
        /// <returns></returns>
        public ActionResult OnLoseFocus()
        {
            return RedirectToAction("JoinTest");
        }


        public ActionResult TestSubmitDoTest()
        {
            Thread[] threads = new Thread[100];
            for (int i = 0; i < threads.Length; i++)
            {
                string x = i.ToString();
                ThreadWorker worker = new ThreadWorker(x);
                worker.ThreadDone += HandleThreadDone;

                threads[i] = new Thread(worker.Run);
                threads[i].Start();
            }
            return View();
        }


        void HandleThreadDone(object sender, EventArgs e)
        {
        }

        class ThreadWorker
        {
            public event EventHandler ThreadDone;
            public string i { get; set; }
            public ThreadWorker(string ix)
            {
                i = ix;
            }
            public void Run()
            {
                StudentController controller = new StudentController();
                //controller = SetFakeControllerContext(controller, i.ToString());
                // Act

                ViewResult result2 = controller.gxetxxx("57e343b71c5ccc1bf41f4565") as ViewResult;
                if (ThreadDone != null)
                    ThreadDone(this, EventArgs.Empty);
            }
        }

        public ActionResult gxetxxx(string testDataId)
        {
            _StudentLogic = new StudentLogic();

            string IP = "";
            //string dethiodangjson = "";
            //ViewBag.Data = Json(new { group = "gg", ddd = "dd" });

            var dbTestData = _StudentLogic.GetTest(testDataId);
            var testDataInJson = JObject.Parse(dbTestData.Data);

            var testGroupList = new List<TestGroupViewModel>();
            int stt = 1;
            #region MyRegion
            // lấy từng nhóm câu hỏi ra
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
                    //ShuffleList(possibleAnswerList);
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
                        //ShuffleList(possibleAnswerList);
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
                    //ShuffleList(singleQuestionVM.PossibleAnswerList);
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
                        //ShuffleList(singleQuestionVM.PossibleAnswerList);
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
                    //ShuffleList(singleQuestionVM.PossibleAnswerList);
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
                        //ShuffleList(singleQuestionVM.PossibleAnswerList);
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
                    //ShuffleList(singleQuestionVM.PossibleAnswerList);
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
                        //ShuffleList(singleQuestionVM.PossibleAnswerList);
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

            #region sss
            string hints = "";
            foreach (var testgroup in testGroupList)
            {
                hints += "#" + testgroup.Title + "!" + (testgroup.isForAll ? "1" : "0") + "@";
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
                                        hints += ((SingleQuestionViewModel)questionChild).Score.ToString() + ")" + stt.ToString();
                                        foreach (var answer in ((SingleQuestionViewModel)questionChild).PossibleAnswerList)
                                        {
                                            if (bool.Parse(answer.RightAnswer))
                                            {
                                                hints += "-" + positionOneChoiceG.ToString();
                                                break;
                                            }
                                            positionOneChoiceG++;
                                        }
                                        hints += "|";
                                        stt++;
                                        break;
                                    case QuestionTypeEnum.QuizN:
                                    case QuestionTypeEnum.TrueFalse:
                                        ((SingleQuestionViewModel)questionChild).STT = stt.ToString();
                                        int positionMultiChoiceG = 1;
                                        hints += ((SingleQuestionViewModel)questionChild).Score.ToString() + ")" + stt.ToString();
                                        foreach (var answer in ((SingleQuestionViewModel)questionChild).PossibleAnswerList)
                                        {
                                            if (bool.Parse(answer.RightAnswer))
                                            {
                                                hints += "-" + positionMultiChoiceG.ToString();
                                            }
                                            positionMultiChoiceG++;
                                        }
                                        hints += "|";
                                        stt++;
                                        break;
                                    case QuestionTypeEnum.Fill:
                                        ((SingleQuestionViewModel)questionChild).STT = stt.ToString();
                                        int positionFillChoiceG = 1;
                                        hints += ((SingleQuestionViewModel)questionChild).Score.ToString() + ")" + stt.ToString();
                                        foreach (var answer in ((SingleQuestionViewModel)questionChild).PossibleAnswerList)
                                        {
                                            if (!answer.RightAnswer.Equals("0"))
                                            {
                                                hints += "-" + positionFillChoiceG.ToString() + "+" + answer.RightAnswer;
                                            }
                                            positionFillChoiceG++;
                                        }
                                        hints += "|";
                                        stt++;
                                        break;
                                }
                            }
                            break;
                        case QuestionTypeEnum.Quiz:
                        case QuestionTypeEnum.Underline:
                            ((SingleQuestionViewModel)question).STT = stt.ToString();
                            int positionOneChoice = 1;
                            hints += ((SingleQuestionViewModel)question).Score.ToString() + ")" + stt.ToString();
                            foreach (var answer in ((SingleQuestionViewModel)question).PossibleAnswerList)
                            {
                                if (bool.Parse(answer.RightAnswer))
                                {
                                    hints += "-" + positionOneChoice.ToString();
                                    break;
                                }
                                positionOneChoice++;
                            }
                            hints += "|";
                            stt++;
                            break;
                        case QuestionTypeEnum.QuizN:
                        case QuestionTypeEnum.TrueFalse:
                            ((SingleQuestionViewModel)question).STT = stt.ToString();
                            int positionMultiChoice = 1;
                            hints += ((SingleQuestionViewModel)question).Score.ToString() + ")" + stt.ToString();
                            foreach (var answer in ((SingleQuestionViewModel)question).PossibleAnswerList)
                            {
                                if (bool.Parse(answer.RightAnswer))
                                {
                                    hints += "-" + positionMultiChoice.ToString();
                                }
                                positionMultiChoice++;
                            }
                            hints += "|";
                            stt++;
                            break;
                        case QuestionTypeEnum.Fill:
                            ((SingleQuestionViewModel)question).STT = stt.ToString();
                            int positionFillChoice = 1;
                            hints += ((SingleQuestionViewModel)question).Score.ToString() + ")" + stt.ToString();
                            foreach (var answer in ((SingleQuestionViewModel)question).PossibleAnswerList)
                            {
                                if (int.Parse(answer.RightAnswer) != 0)
                                {
                                    hints += "-" + positionFillChoice.ToString() + "+" + answer.RightAnswer;
                                }
                                positionFillChoice++;
                            }
                            hints += "|";
                            stt++;
                            break;
                        case QuestionTypeEnum.Matching:
                            ((SingleQuestionViewModel)question).STT = stt.ToString();
                            hints += ((SingleQuestionViewModel)question).Score.ToString() + ")" + stt.ToString();
                            for (int i = 0; i < ((SingleQuestionViewModel)question).PossibleAnswerList.Count; i += 2)
                            {
                                if (!((SingleQuestionViewModel)question).PossibleAnswerList[i].RightAnswer.Equals("0"))
                                {
                                    hints += "-" + (i + 1).ToString() + "+" + ((SingleQuestionViewModel)question).PossibleAnswerList[i].RightAnswer;
                                }
                            }
                            hints += "|";
                            stt++;
                            break;
                    }
                }
            }
            #endregion

            var testDataMixed_json = JsonConvert.SerializeObject(testGroupList);
            var testDataMixed_json_base64 = Base64Encode(testDataMixed_json);
            hints = Base64Encode(hints);

            TestBaseInfo testbaseInfo = new StudentTestDataLogic().LoadTestBaseInfo(testDataId);

            var viewModel = new TestDataViewModel
            {
                TestInfo = testbaseInfo,
                StudentName = "",
                StudentClass = "",
                StudentIPAdd = IP,
                TestGroupChoose = "",
                Base64Code = hints,
                Base64Data = testDataMixed_json_base64,
                TestGroupList = testGroupList
            };

            List<string> studentAnswersList = new List<string>(); //đáp án của thí sinh            
            TestResult testResult = new TestResult();
            testResult.TestDataID = testDataId;
            testResult.StudentName = "";
            testResult.StudentClass = "";
            testResult.StudentIPAdd = "";
            testResult.TestGroupChoose = "";
            testResult.StudentTestResult = studentAnswersList;
            testResult.StudentTestData = testDataMixed_json;
            testResult.TestHints = hints;
            testResult.Score = -1;
            testResult.RecordDateTime = DateTime.Now;
            _StudentLogic = new StudentLogic();
            string testResultID = _StudentLogic.SaveTestResultTemp(testResult);
            ViewBag.DoneLoading = "a";

            return View(viewModel);
        }
    }
}