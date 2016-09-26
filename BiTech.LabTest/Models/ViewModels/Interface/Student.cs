using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BiTech.LabTest.Models.ViewModels.Interface
{
    #region interface test data view models

    public interface ITestGroupViewModel
    {
        bool isForAll { get; set; }
        string Title { get; set; }
        IQuestionsOfType Quiz { get; set; }
        IQuestionsOfType Underline { get; set; }
        IQuestionsOfType Fill { get; set; }
        IQuestionsOfType TrueFalse { get; set; }
        IQuestionsOfType Matching { get; set; }
    }

    public interface IQuestionsOfType
    {
        List<ISingleQuestionViewModel> ListOfSingles { get; set; }
        List<IGroupQuestionViewModel> ListOfGroups { get; set; }
    }

    public interface IGroupQuestionViewModel
    {
        string Content { get; set; }
        List<ISingleQuestionViewModel> SingleQuestionList { get; set; }
    }

    public interface ISingleQuestionViewModel
    {
        string STT { get; set; }
        string ID { get; set; }
        string Score { get; set; }
        string Content { get; set; }
        DataObject.ViewModels.StudentViewModel.QuestionTypeEnum QuestionType { get; set; }
        List<IAnswerViewModel> AnswerList { get; set; }
    }

    public class IAnswerViewModel
    {
        public string Content { get; set; }

        public string RightAnswer { get; set; }

        public string STT { get; set; }

        public string ID { get; set; }

        public bool isMixable { get; set; }
    }
    #endregion

    #region interface test redult view models
    public interface IGroupResult
    {
        string GroupName { get; set; }
        List<IQuestionResult> QuestionList { get; set; }
    }

    public interface IQuestionResult
    {
        string STT { get; set; }
        string ID { get; set; }
        List<string> Answer { get; set; }
    }
    #endregion
}