using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiTech.LabTest.Models.ViewModels.Interface
{
    public interface ITestGroupViewModel
    {
        string Title { get; set; }
        IQuestionsOfType Quiz { get; set; }
        IQuestionsOfType Underline { get; set; }
        IQuestionsOfType Fill { get; set; }
        IQuestionsOfType TrueFalse { get; set; }
        IQuestionsOfType Matching { get; set; }
    }
}
