using System.ComponentModel.DataAnnotations;

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

        public class WaitingScreenViewModel
        {
            public string UserIpAddress { get; set; }

            public string FullName { get; set; }

            public string Class { get; set; }
        }
    }
}