using BiTech.LabTest.Models.ViewModels.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BiTech.LabTest.Models.ViewModels
{
    public class Student
    {
        // ViewModel trang nhập thông tin thí sinh
        public class StudentJoinTestViewModel
        {
            [Required(ErrorMessage = "Vui lòng điền tên của bạn")]
            public string FullName { get; set; }

            [Required(ErrorMessage = "Vui lòng điền lớp học của bạn")]
            public string Class { get; set; }
        }
        
        /// <summary>
        /// 
        /// </summary>
        public class TestViewModel
        {
            public TestResultViewModel TestResult { get; set; }
            public TestDataViewModel TestData { get; set; }
        }


        #region Test Data ViewModels
        /// <summary>
        /// 
        /// </summary>
        public class TestDataViewModel
        {
            public string SchoolName { get; set; }
            public string SchoolInfo { get; set; }
            public string TestType { get; set; }
            public string Year { get; set; }
            public string TestTime { get; set; }
            public string Subject { get; set; }
            public string Grade { get; set; }

            public List<TestGroupViewModel> TestGroupList { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public class TestGroupViewModel
        {
            public QuestionPackage Fill { get; set; }

            public QuestionPackage Matching { get; set; }

            public QuestionPackage Quiz { get; set; }

            public string Title { get; set; }

            public QuestionPackage TrueFalse { get; set; }

            public QuestionPackage Underline { get; set; }

            public bool isForAll { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public class QuestionPackage
        {
            public List<GroupQuestionViewModel> ListOfGroups { get; set; }

            public List<SingleQuestionViewModel> ListOfSingles { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public class GroupQuestionViewModel
        {
            public string Content { get; set; }

            public List<SingleQuestionViewModel> SingleQuestionList { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public class SingleQuestionViewModel
        {
            public List<AnswerViewModel> AnswerList { get; set; }

            public string Content { get; set; }

            public string ID { get; set; }

            public QuestionTypeEnum QuestionType { get; set; }

            public string Score { get; set; }

            public string STT { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public class AnswerViewModel
        {
            public string Content { get; set; }

            public string RightAnswer { get; set; }

            public string STT { get; set; }

            public string ID { get; set; }

            public bool isMixable { get; set; }
        }
        #endregion
        
        #region Test Result ViewModels
        /// <summary>
        /// 
        /// </summary>
        public class TestResultViewModel
        {
            public string StudentClass { get; set; }

            public string StudentIPAdd { get; set; }

            public string StudentName { get; set; }

            public string TestGroupChoose { get; set; }

            public List<TestGroupResultViewModel> GroupList { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public class TestGroupResultViewModel
        {
            public string GroupName { get; set; }

            public List<QuestionResultViewModel> QuestionList { get; set; }

            public TestGroupResultViewModel()
            {
                QuestionList = new List<QuestionResultViewModel>();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class QuestionResultViewModel
        {
            public List<AnswerResultViewModel> Answer { get; set; }

            public string ID { get; set; }

            public string STT { get; set; }

            public QuestionResultViewModel()
            {
                Answer = new List<AnswerResultViewModel>();
            }
        }

        public class AnswerResultViewModel
        {

            public string Content { get; set; }
        }
        #endregion

        public enum QuestionTypeEnum
        {
            Quiz,
            Fill,
            Underline,
            TrueFalse,
            Matching
        }
    }
}
