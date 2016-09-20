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

        public string TestDataID { get; set; }

        public string StudentClass { get; set; }
        public string StudentIPAdd { get; set; }
        public string StudentName { get; set; }
        public string TestGroupChoose { get; set; }

        public double Score { get; set; }
        public string StudentTestData { get; set; }
        public List<string> StudentTestResult { get; set; }
    }
}
