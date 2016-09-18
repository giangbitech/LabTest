﻿using BiTech.LabTest.Models.ViewModels.Interface;
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

            public string StudentClass { get; set; }
            public string StudentIPAdd { get; set; }
            public string StudentName { get; set; }
            public string TestGroupChoose { get; set; }

            public List<TestGroupViewModel> TestGroupList { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public class TestGroupViewModel
        {
            public string Title { get; set; }

            public Question_String_MultiChoices_Package Fill { get; set; }

            public Question_String_MultiChoices_Package Matching { get; set; }

            public Question_String_OneChoice_Package Quiz { get; set; }

            public Question_Bool_MultiChoices_Package QuizN { get; set; }

            public Question_Bool_MultiChoices_Package TrueFalse { get; set; }

            public Question_String_OneChoice_Package Underline { get; set; }

            public bool isForAll { get; set; }
        }

        /// <summary>
        /// gói câu hỏi 1 lựa chọn
        /// </summary>
        public class Question_String_OneChoice_Package
        {
            public List<GroupQuestion_String_OneChoice_ViewModel> ListOfGroups { get; set; }

            public List<SingleQuestion_String_OneChoice_ViewModel> ListOfSingles { get; set; }
        }

        public class Question_Bool_MultiChoices_Package
        {
            public List<GroupQuestion_Bool_MultiChoices_ViewModel> ListOfGroups { get; set; }

            public List<SingleQuestion_Bool_MultiChoices_ViewModel> ListOfSingles { get; set; }
        }

        public class Question_String_MultiChoices_Package
        {
            public List<GroupQuestion_String_MultiChoices_ViewModel> ListOfGroups { get; set; }

            public List<SingleQuestion_String_MultiChoices_ViewModel> ListOfSingles { get; set; }
        }

        /// <summary>
        /// ViewModel cha cho các câu hỏi chùm
        /// </summary>
        public class GroupQuestionViewModel
        {
            public string Content { get; set; }
        }

        public class GroupQuestion_String_OneChoice_ViewModel : GroupQuestionViewModel
        {
            public List<SingleQuestion_String_OneChoice_ViewModel> SingleQuestionList { get; set; }
        }

        public class GroupQuestion_Bool_MultiChoices_ViewModel : GroupQuestionViewModel
        {
            public List<SingleQuestion_Bool_MultiChoices_ViewModel> SingleQuestionList { get; set; }
        }

        public class GroupQuestion_String_MultiChoices_ViewModel : GroupQuestionViewModel
        {
            public List<SingleQuestion_String_MultiChoices_ViewModel> SingleQuestionList { get; set; }
        }

        /// <summary>
        /// ViewModel cha cho các câu hỏi
        /// </summary>
        public class SingleQuestionViewModel
        {
            public List<PossibleAnswerViewModel> PossibleAnswerList { get; set; }

            public string Content { get; set; }

            public string ID { get; set; }

            public QuestionTypeEnum QuestionType { get; set; }

            public string Score { get; set; }

            public string STT { get; set; }
        }

        /// <summary>
        /// Câu hỏi 1 lựa chọn - trắc nghiệm hoặc gạch chân
        /// </summary>
        public class SingleQuestion_String_OneChoice_ViewModel : SingleQuestionViewModel
        {
            public string SelectedAnswer { get; set; }
        }

        /// <summary>
        /// Câu hỏi nhiều lựa chọn dạng check- trắc nghiệm n lựa chọn hoặc đúng sai
        /// </summary>
        public class SingleQuestion_Bool_MultiChoices_ViewModel : SingleQuestionViewModel
        {
            public List<AnswerPacket> SelectedAnswer { get; set; }

            public class AnswerPacket
            {
                public bool Choice { get; set; }
                public string STT { get; set; }
            }
        }

        /// <summary>
        /// Câu hỏi nhiều câu trả lời dạng text - điền khuyết, nối chéo
        /// </summary>
        public class SingleQuestion_String_MultiChoices_ViewModel : SingleQuestionViewModel
        {
            public List<AnswerPacket> SelectedAnswer { get; set; }

            public class AnswerPacket
            {
                public string Choice { get; set; }
                public string STT { get; set; }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class PossibleAnswerViewModel
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
            QuizN,
            Fill,
            Underline,
            TrueFalse,
            Matching
        }
    }
}
