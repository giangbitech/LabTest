using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiTech.LabTest.Models.ViewModels.Interface
{
    public interface IQuestionsOfType
    {
        List<ISingleQuestionViewModel> ListOfSingles { get; set; }
        List<IGroupQuestionViewModel> ListOfGroups { get; set; }
    }
}
