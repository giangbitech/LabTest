using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace BiTech.LabTest.DAL.Models
{
    public class TestData : IModel
    {
        public enum TestStepEnum
        {
            Waiting,
            OnWorking,
            Finish
        }


        public ObjectId Id { get; set; }

        public DateTime RecordDateTime { get; set; }

        /// <summary>
        /// Data được import từ giáo viên ở dạng json-format
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// Lộ trình Test đang ở bước nào
        /// </summary>
        public TestStepEnum TestStep { get; set; }

        public TestData()
        {
            RecordDateTime = DateTime.Now;
            TestStep = TestStepEnum.Waiting;
        }
    }
}
