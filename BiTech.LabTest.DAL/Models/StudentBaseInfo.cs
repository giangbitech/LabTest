using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace BiTech.LabTest.DAL.Models
{
    public class StudentBaseInfo : IModel
    {
        public ObjectId Id { get; set; }

        public DateTime RecordDateTime { get; set; }

        /// <summary>
        /// ID của bài thi
        /// </summary>
        public string TestDataID { get; set; }

        /// <summary>
        /// ID của bài thi
        /// </summary>
        public string TestResultID { get; set; }

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
        /// Điểm thi của học sinh
        /// Backup thì điểm = -1
        /// </summary>
        public double Score { get; set; }
    }
}
