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
        public class StudentJoinTestViewModel
        {
            [Required(ErrorMessage = "Vui lòng điền tên của bạn")]
            public string FullName { get; set; }

            [Required(ErrorMessage = "Vui lòng điền lớp học của bạn")]
            public string Class { get; set; }
        }

        public class TestGroupViewModel : ITestGroupViewModel
        {
            public IQuestionsOfType Fill { get; set; }

            public IQuestionsOfType Matching { get; set; }

            public IQuestionsOfType Quiz { get; set; }

            public string Title { get; set; }

            public IQuestionsOfType TrueFalse { get; set; }

            public IQuestionsOfType Underline { get; set; }
        }

        public class QuestionsOfType : IQuestionsOfType
        {
            public List<IGroupQuestionViewModel> ListOfGroups { get; set; }

            public List<ISingleQuestionViewModel> ListOfSingles { get; set; }
        }

        public class GroupQuestionViewModel : IGroupQuestionViewModel
        {
            public string Content { get; set; }

            public List<ISingleQuestionViewModel> SingleQuestionList { get; set; }
        }

        public class SingleQuestionViewModel : ISingleQuestionViewModel
        {
            public List<IAnswerViewModel> AnswerList { get; set; }

            public string Content { get; set; }

            public string ID { get; set; }

            public QuestionTypeEnum QuestionType { get; set; }

            public string Score { get; set; }
        }

        public class AnswerViewModel : IAnswerViewModel
        {
            public string Content { get; set; }

            public string RightAnswer { get; set; }
        }

        //public class QuestionViewModel
        //{
        //    public string QuestionContent { get; set; }


        //    public string RightResult { get; set; }

        //    public QuestionTypeEnum QuestionType { get; set; }


        //}

        //public class QusetionGroupViewModel
        //{
        //    public string ID { get; set; }

        //    public string Title { get; set; }

        //    public string isPrintOut { get; set; }
        //}

        //public class SingleQuestionViewModel
        //{
        //    public string QuestionContent { get; set; }

        //    public string FirstAnswer { get; set; }

        //    public string SecondsAnswer { get; set; }

        //    public QuestionTypeEnum QuestionType { get; set; }

        //}



        public class TestDataViewModel
        {
            public string SchoolName { get; set; }
            public string SchoolInfo { get; set; }
            public string TestType { get; set; }
            public string Year { get; set; }
            public string TestTime { get; set; }
            public string Subject { get; set; }
            public string Grade { get; set; }

            public List<ITestGroupViewModel> TestGroupList { get; set; }

        }

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
