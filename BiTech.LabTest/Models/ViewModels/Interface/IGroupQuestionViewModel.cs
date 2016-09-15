using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiTech.LabTest.Models.ViewModels.Interface
{
    public interface IGroupQuestionViewModel
    {
        string Content { get; set; }
        List<ISingleQuestionViewModel> SingleQuestionList { get; set; }
    }
}
