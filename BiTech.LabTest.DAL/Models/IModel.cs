using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace BiTech.LabTest.DAL.Models
{
    public interface IModel
    {
        [BsonId]
        ObjectId Id { get; set; }

        // Thời gian được khi vào database
        DateTime RecordDateTime { get; set; }
    }
}
