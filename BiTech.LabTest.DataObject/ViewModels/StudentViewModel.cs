using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiTech.LabTest.DataObject.ViewModels
{
    public class StudentViewModel
    {
        #region Test Data ViewModels
        
        /// <summary>
        /// Test Data view model
        /// </summary>
        public class TestDataViewModel
        {
            public TestBaseInfo TestInfo { get; set; }

            public string StudentClass { get; set; }
            public string StudentIPAdd { get; set; }
            public string StudentName { get; set; }
            public string TestGroupChoose { get; set; }

            public string Base64Code { get; set; }
            public string Base64Data { get; set; }

            public List<TestGroupViewModel> TestGroupList { get; set; }
        }

        public class TestBaseInfo
        {
            public string SchoolName { get; set; }
            public string SchoolInfo { get; set; }
            public string TestType { get; set; }
            public string Year { get; set; }
            public string TestTime { get; set; }
            public string Subject { get; set; }
            public string Grade { get; set; }
        }

        /// <summary>
        /// Phần câu hỏi thi (phần chung, phần riêng)
        /// </summary>
        public class TestGroupViewModel
        {
            public string Title { get; set; }

            public List<object> QuestionsList { get; set; }

            public bool isForAll { get; set; }
        }

        /// <summary>
        /// object chung cho các câu hỏi đơn và chùm
        /// </summary>
        public class QuestionObjectViewModel
        {
            public string Content { get; set; }

            public QuestionTypeEnum QuestionType { get; set; }
        }

        /// <summary>
        /// ViewModel cha cho các câu hỏi chùm
        /// </summary>
        public class GroupQuestionViewModel : QuestionObjectViewModel
        {
            //public string Content { get; set; }

            public List<object> SingleQuestionsList { get; set; }
        }

        
        /// <summary>
        /// ViewModel cha cho các câu hỏi
        /// </summary>
        public class SingleQuestionViewModel : QuestionObjectViewModel
        {
            public List<PossibleAnswerViewModel> PossibleAnswerList { get; set; }

            public string ID { get; set; }

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

        public enum QuestionTypeEnum
        {
            Quiz,
            QuizN,
            Fill,
            Underline,
            TrueFalse,
            Matching,
            Group
        }
        #endregion
    }
}
