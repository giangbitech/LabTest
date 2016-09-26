using System.Collections.Generic;
using static BiTech.LabTest.DAL.Models.TestData;

namespace BiTech.LabTest.Models.ViewModels
{
    public class Teacher
    {
        public class QuestionGroupInformation
        {
            /// <summary>
            /// Tên nhóm:
            /// </summary>
            public string Title { get; set; } = "";

            /// <summary>
            /// Số lượng câu hỏi trắc nghiệm
            /// </summary>
            public int TotalQuiz { get; set; } = 0;

            /// <summary>
            /// Số lượng câu hỏi gạch chân
            /// </summary>
            public int TotalUnderline { get; set; } = 0;

            /// <summary>
            /// Số lượng câu hỏi điền vào chỗ trống
            /// </summary>
            public int TotalFill { get; set; } = 0;

            /// <summary>
            /// Số lượn câu hỏi đúng sai
            /// </summary>
            public int TotalTrueFalse { get; set; } = 0;

            /// <summary>
            /// Số lượng câu hỏi ghép câu
            /// </summary>
            public int TotalMatching { get; set; } = 0;

            /// <summary>
            /// Tổng số câu hỏi
            /// </summary>
            public int TotalAllQuestion
            {
                get
                {
                    return TotalQuiz + TotalUnderline + TotalFill + TotalTrueFalse + TotalMatching;
                }
            }
        }

        public class TestInformationViewModel
        {
            public string TestId { get; set; }

            /// <summary>
            /// Tiêu đề bài thi
            /// </summary>
            public string Subject { get; set; }

            /// <summary>
            /// Khối lớp thi
            /// </summary>
            public string Grade { get; set; }

            /// <summary>
            /// Data bài test được import từ file vào
            /// Json-format
            /// </summary>
            public string TestData { get; set; }

            /// <summary>
            /// Thời gian làm bài
            /// </summary>
            public int TotalMinutes { get; set; }

            /// <summary>
            /// Thời gian còn lại để làm bài thi. Tính bằng giây
            /// </summary>
            public int RemainingTestSeconds { get; set; }

            /// <summary>
            /// Loại bài thi
            /// </summary>
            public string Type { get; set; }

            /// <summary>
            /// Năm học
            /// </summary>
            public string Year { get; set; }

            public System.DateTime StartDateTime { get; set; }

            public System.DateTime EndDateTime { get; set; }

            public DAL.Models.TestData.TestStepEnum TestStep { get; set; }

            public List<QuestionGroupInformation> QuestionGroups { get; set; }


            public TestInformationViewModel()
            {
                TestStep = DAL.Models.TestData.TestStepEnum.Waiting;
                Subject = Grade = TestData = "";
                QuestionGroups = new List<QuestionGroupInformation>();
                TotalMinutes = 0;
            }
        }

        public class OnlineStudentViewModel
        {
            public int OrderNumber { get; set; }

            public string IpAddress { get; set; }

            /// <summary>
            /// Điểm thi
            /// </summary>
            public string Mark { get; set; }

            public string FullName { get; set; }

            public string StudentClass { get; set; }

            /// <summary>
            /// TestStep dạng Enum
            /// </summary>
            public TestStepEnum TestStep { get; set; }

            /// <summary>
            /// Mô tả TestStep bằng ngôn ngữ human-readable
            /// </summary>
            public string TestStepDescription { get; set; }
        }
    }
}