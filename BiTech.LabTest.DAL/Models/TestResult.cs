using System;
using System.Collections.Generic;
using MongoDB.Bson;
using static BiTech.LabTest.DAL.Models.TestData;
using MongoDB.Bson.Serialization.Attributes;

namespace BiTech.LabTest.DAL.Models
{
    public class TestResult : IModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        /// <summary>
        /// Thời gian Online lần cuối.
        /// </summary>
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime RecordDateTime { get; set; }

        public string TestDataID { get; set; }

        public string StudentClass { get; set; }

        public string StudentIPAddress { get; set; }

        public string StudentName { get; set; }

        public string TestGroupChoose { get; set; }

        public double Score { get; set; }

        public string StudentTestData { get; set; }

        public TestStepEnum TestStep { get; set; }

        public List<string> StudentTestResult { get; set; }
    }
}
