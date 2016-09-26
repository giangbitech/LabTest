using BiTech.LabTest.DAL.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BiTech.LabTest.DAL.Models.TestData;
using static BiTech.LabTest.DataObject.ViewModels.StudentViewModel;

namespace BiTech.LabTest.BLL
{
    public class StudentTestDataLogic
    {
        /// <summary>
        /// Lấy thông tin cơ bản của bài thi
        /// </summary>
        /// <param name="testDataId">id của bài thi</param>
        /// <returns></returns>
        public TestBaseInfo LoadTestBaseInfo(string testDataId)
        {
            StudentLogic studentLogic = new StudentLogic();
            var dbTestData = studentLogic.GetTest(testDataId);
            var testDataInJson = JObject.Parse(dbTestData.Data);
            TestBaseInfo testinfo = new TestBaseInfo();
            testinfo.SchoolName = testDataInJson["test"]["header"]["name"]?.ToString();
            testinfo.SchoolInfo = testDataInJson["test"]["header"]["info"]?.ToString();
            testinfo.Year = testDataInJson["test"]["header"]["year"]?.ToString();
            testinfo.TestType = testDataInJson["test"]["header"]["type"]?.ToString();
            testinfo.TestTime = testDataInJson["test"]["header"]["time"]?.ToString();
            testinfo.Subject = testDataInJson["test"]["header"]["subject"]?.ToString();
            testinfo.Grade = testDataInJson["test"]["header"]["grade"]?.ToString();
            return testinfo;
        }

        /// <summary>
        /// Lấy nội dung bài thi theo Id
        /// </summary>
        /// <param name="testDataId">id của bài thi</param>
        /// <returns></returns>
        public List<TestGroupViewModel> LoadTestById(string testDataId)
        {
            StudentLogic studentLogic = new StudentLogic();
            var dbTestData = studentLogic.GetTest(testDataId);
            return LoadTestByTestData(dbTestData);

        }

        /// <summary>
        /// Lấy nội dung bài thi theo TestData
        /// </summary>
        /// <param name="dbTestData">TestData</param>
        /// <returns></returns>
        public List<TestGroupViewModel> LoadTestByTestData(TestData dbTestData)
        {
            var testDataInJson = JObject.Parse(dbTestData.Data);
            var testGroupList = new List<TestGroupViewModel>();
            int stt = 1;
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

                Tool.ShuffleList(testGroup.QuestionsList);
                testGroupList.Add(testGroup);
            }
            return testGroupList;
        }

        /// <summary>
        /// Lấy bài thi đã được lưu tạm của học sinh
        /// </summary>
        /// <param name="testResultBackup">thông tin lưu tạm của học sinh</param>
        /// <returns></returns>
        public List<TestGroupViewModel> LoadTestBackup(TestResult testResultBackup)
        {
            var testGroupList = new List<TestGroupViewModel>();

            #region Load câu hỏi đã lưu
            List<string> studentAnswersListBackup = new List<string>(); //đáp án của thí sinh      
            //ViewBag.TestGroupChoose = testResultBackup.TestGroupChoose;
            studentAnswersListBackup = testResultBackup.StudentTestResult;
            //testResultBackup.StudentTestData
            var tmptestGroupList = JsonConvert.DeserializeObject<List<TestGroupViewModel>>(testResultBackup.StudentTestData);
            foreach (var itemTestGroup in tmptestGroupList)
            {
                TestGroupViewModel testgroup = new TestGroupViewModel();
                testgroup.Title = itemTestGroup.Title;
                testgroup.isForAll = itemTestGroup.isForAll;
                testgroup.QuestionsList = new List<object>();

                foreach (var itemQuestionObject in itemTestGroup.QuestionsList)
                {
                    var jsonQuestion = JObject.Parse(itemQuestionObject.ToString());
                    var itemQuestionType = jsonQuestion["QuestionType"].ToString();
                    var type = (QuestionTypeEnum)Enum.Parse(typeof(QuestionTypeEnum), itemQuestionType);

                    switch (type)
                    {
                        case QuestionTypeEnum.Quiz:
                        case QuestionTypeEnum.Underline:
                            SingleQuestion_String_OneChoice_ViewModel tmp1 = JsonConvert.DeserializeObject<SingleQuestion_String_OneChoice_ViewModel>(jsonQuestion.ToString());
                            testgroup.QuestionsList.Add(tmp1);
                            break;
                        case QuestionTypeEnum.QuizN:
                        case QuestionTypeEnum.TrueFalse:
                            SingleQuestion_Bool_MultiChoices_ViewModel tmp2 = JsonConvert.DeserializeObject<SingleQuestion_Bool_MultiChoices_ViewModel>(jsonQuestion.ToString());
                            testgroup.QuestionsList.Add(tmp2);
                            break;
                        case QuestionTypeEnum.Fill:
                        case QuestionTypeEnum.Matching:
                            SingleQuestion_String_MultiChoices_ViewModel tmp3 = JsonConvert.DeserializeObject<SingleQuestion_String_MultiChoices_ViewModel>(jsonQuestion.ToString());
                            testgroup.QuestionsList.Add(tmp3);
                            break;
                        case QuestionTypeEnum.Group:
                            GroupQuestionViewModel tmp4 = JsonConvert.DeserializeObject<GroupQuestionViewModel>(jsonQuestion.ToString());
                            GroupQuestionViewModel tmpGroup = new GroupQuestionViewModel();
                            tmpGroup.Content = tmp4.Content;
                            tmpGroup.QuestionType = tmp4.QuestionType;
                            tmpGroup.SingleQuestionsList = new List<object>();
                            for (int i = 0; i < tmp4.SingleQuestionsList.Count; i++)
                            {
                                var jsonGQuestion = JObject.Parse(tmp4.SingleQuestionsList[i].ToString());
                                var itemGQuestionType = jsonGQuestion["QuestionType"].ToString();
                                var gtype = (QuestionTypeEnum)Enum.Parse(typeof(QuestionTypeEnum), itemGQuestionType);

                                switch (gtype)
                                {
                                    case QuestionTypeEnum.Quiz:
                                    case QuestionTypeEnum.Underline:
                                        SingleQuestion_String_OneChoice_ViewModel gtmp1 = JsonConvert.DeserializeObject<SingleQuestion_String_OneChoice_ViewModel>(jsonGQuestion.ToString());
                                        tmpGroup.SingleQuestionsList.Add(gtmp1);
                                        break;
                                    case QuestionTypeEnum.QuizN:
                                    case QuestionTypeEnum.TrueFalse:
                                        SingleQuestion_Bool_MultiChoices_ViewModel gtmp2 = JsonConvert.DeserializeObject<SingleQuestion_Bool_MultiChoices_ViewModel>(jsonGQuestion.ToString());
                                        tmpGroup.SingleQuestionsList.Add(gtmp2);
                                        break;
                                    case QuestionTypeEnum.Fill:
                                    case QuestionTypeEnum.Matching:
                                        SingleQuestion_String_MultiChoices_ViewModel gtmp3 = JsonConvert.DeserializeObject<SingleQuestion_String_MultiChoices_ViewModel>(jsonGQuestion.ToString());
                                        tmpGroup.SingleQuestionsList.Add(gtmp3);
                                        break;
                                }

                            }
                            testgroup.QuestionsList.Add(tmpGroup);
                            break;

                    }

                }
                testGroupList.Add(testgroup);
            }
            var jsonx = tmptestGroupList[0].QuestionsList[0];

            //var type = jsonx[""];
            if (testResultBackup.Score == -1)
            {
                return testGroupList;
            }
            else
            {
                return null;
            }
            #endregion
        }

        /// <summary>
        /// Tạo đáp án
        /// </summary>
        /// <param name="testGroupList">danh sách câu hỏi</param>
        /// <returns></returns>
        public string LoadHints(List<TestGroupViewModel> testGroupList)
        {
            int stt = 1;
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
            return hints;
        }

        public double ReviewStudentTestResult(NameValueCollection formCollection, StudentBaseInfo studentBaseInfo)
        {
            StudentLogic studentLogic = new StudentLogic();
            double score = 0;
            List<string> studentAnswersList = new List<string>(); //đáp án của thí sinh

            var Base64Code = formCollection["Base64Code"];// đáp án
            var Base64Data = Tool.Base64Decode(formCollection["Base64Data"]);// đề thi

            #region Chấm điểm
            var testGroupHints = Tool.Base64Decode(Base64Code).Split(new char[] { '#' }, StringSplitOptions.RemoveEmptyEntries);
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
            #endregion

            #region Lưu kết quả vào csdl

            TestResult testResult = new TestResult();
            testResult.TestDataID = studentBaseInfo.TestDataID;
            testResult.StudentName = studentBaseInfo.StudentName;
            testResult.StudentClass = studentBaseInfo.StudentClass;
            testResult.StudentIPAdd = formCollection["StudentIPAdd"];
            testResult.TestGroupChoose = formCollection["TestGroupChoose"] != null ? formCollection["TestGroupChoose"] : "";
            testResult.StudentTestResult = studentAnswersList;
            testResult.StudentTestData = Base64Data;
            testResult.TestHints = Base64Code;
            testResult.Score = score;
            testResult.RecordDateTime = DateTime.Now;
            studentLogic = new StudentLogic();
            string newID = studentLogic.SaveTestResult(testResult);
            studentLogic.UpdateTestResultTemp(testResult, studentBaseInfo.TestResultID);

            studentBaseInfo.TestResultID = newID;
            #endregion

            return score;
        }
       
    }
}
