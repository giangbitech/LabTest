using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiTech.LabTest.Models.ViewModels.Interface
{
    public interface ISingleQuestionViewModel
    {
        string ID { get; set; }
        string Score { get; set; }
        string Content { get; set; }
        Student.QuestionTypeEnum QuestionType { get; set; }
        List<IAnswerViewModel> AnswerList { get; set; }
    }
}
