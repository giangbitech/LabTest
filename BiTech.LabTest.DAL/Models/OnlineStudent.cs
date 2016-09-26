using System;
using MongoDB.Bson;
using static BiTech.LabTest.DAL.Models.TestData;
using MongoDB.Bson.Serialization.Attributes;

namespace BiTech.LabTest.DAL.Models
{
    public class OnlineStudent : IModel
    {        
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        /// <summary>
        /// Thời gian cập nhật lần cuối.
        /// </summary>

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime RecordDateTime { get; set; }

        public string IpAddress { get; set; }

        public string FullName { get; set; }

        public string StudentClass { get; set; }

        public TestStepEnum TestStep { get; set; }


        public OnlineStudent()
        {
            RecordDateTime = DateTime.Now;
            TestStep = TestStepEnum.Waiting;
        }
    }
}
