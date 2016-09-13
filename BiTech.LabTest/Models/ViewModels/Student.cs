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
    }
}