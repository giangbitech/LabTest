using BiTech.LabTest.DAL.Models;
using BiTech.LabTest.DAL.Respository;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq;

namespace BiTech.LabTest.DAL
{
    public class TestDataEngine : EntityRepository<TestData>
    {
        private IMongoCollection<TestData> DatabaseCollection { get; set; }


        public TestDataEngine(IDatabase database, string tableName) : base(database, tableName)
        {
            Database = (IMongoDatabase)database.GetConnection();
            DatabaseCollection = Database.GetCollection<TestData>(tableName);
        }

        
        /// <summary>
        /// Lấy bài test cuối cùng đang có trong database
        /// </summary>
        /// <returns></returns>
        public TestData GetLastTest()
        {
            return DatabaseCollection.Find(new BsonDocument()).SortByDescending(m => m.RecordDateTime).FirstOrDefault();
        }
    }
}
