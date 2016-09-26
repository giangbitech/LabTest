using BiTech.LabTest.Models.ViewModels.Interface;
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
        /// Thông tin lưu trong cookie
        /// </summary>
        public class StudentInfoCookieModel
        {
            public string Name { get; set; }

            public string Class { get; set; }

            public string Score { get; set; }

            public string TestDataID { get; set; }
        }
    }
}
