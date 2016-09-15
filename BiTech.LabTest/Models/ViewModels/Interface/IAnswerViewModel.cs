using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiTech.LabTest.Models.ViewModels.Interface
{
    public interface IAnswerViewModel
    {
        string Content { get; set; }
        string RightAnswer { get; set; }
    }
}
