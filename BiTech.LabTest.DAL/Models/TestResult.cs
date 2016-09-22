using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace BiTech.LabTest.DAL.Models
{
    public class TestResult : IModel
    {
        public ObjectId Id { get; set; }

        public DateTime RecordDateTime { get; set; }

        /// <summary>
        /// ID của bài thi
        /// </summary>
        public string TestDataID { get; set; }

        /// <summary>
        /// Lớp của học sinh
        /// </summary>
        public string StudentClass { get; set; }

        /// <summary>
        /// IP của học sinh
        /// </summary>
        public string StudentIPAdd { get; set; }

        /// <summary>
        /// Tên của học sinh
        /// </summary>
        public string StudentName { get; set; }

        /// <summary>
        /// Tên phần thi riêng mà học sinh chọn
        /// </summary>
        public string TestGroupChoose { get; set; }

        /// <summary>
        /// Điểm thi của học sinh
        /// Backup thì điểm = -1
        /// </summary>
        public double Score { get; set; }

        /// <summary>
        /// Đề thi của học sinh ở dạng json-format
        /// </summary>
        public string StudentTestData { get; set; }

        /// <summary>
        /// Kết quả thi của học sinh
        /// Answer-[stt câu]-[tên loại câu hỏi]+[đáp án chọn]
        /// 
        /// Kết quả thi backup của học sinh
        /// Answer-[stt câu]+[đáp án chọn]
        /// 
        /// Định dạng đáp án chọn:
        /// câu hỏi trắc nghiệm và gạch chân có 1 đáp án chọn
        /// VD: Answer-1-Quiz_Underline+2
        /// 
        /// câu hỏi trắc nghiệm nhiều lựa chọn có nhiều đáp án đúng
        /// VD: Answer-1-QuizN_TrueFalse+2,3
        /// 
        /// câu hỏi đúng sai có 0 hoặc nhiều đáp án chọn "True"
        /// VD: Answer-1-QuizN_TrueFalse+2,3
        /// 
        /// câu hỏi điền khuyết có nhiều đáp án chọn là vị trí điền câu trả lời X vào đoạn văn (với X là thứ tự câu gợi ý: A. ; B. ;)
        /// VD: Answer-1-Fill_Matching+,1,2,3,4,
        /// 
        /// câu hỏi nối chéo có nhiều đáp án với vị trí bên trái (thứ tự lẽ) chứa giá trị lựa chọn bên phải (thứ tự chẵn)
        /// VD: Answer-1-Fill_Matching+A,,B,,,,
        /// 
        /// </summary>
        public List<string> StudentTestResult { get; set; }
    }
}
