using BiTech.LabTest.DAL.Models;
using BiTech.LabTest.DAL.Respository;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;

namespace BiTech.LabTest.DAL
{
    public class TestResultEngine : EntityRepository<TestResult>
    {
        private IMongoCollection<TestResult> DatabaseCollection { get; set; }


        public TestResultEngine(IDatabase database, string tableName) : base(database, tableName)
        {
            Database = (IMongoDatabase)database.GetConnection();
            DatabaseCollection = Database.GetCollection<TestResult>(tableName);
        }

        /// <summary>
        /// Lấy danh sách kể quả thi theo TestDataId
        /// </summary>
        /// <param name="testId">TestDataId cần lấy kết quả thi</param>
        /// <returns></returns>
        public List<TestResult> GetAllByTestId(string testId)
        {
            return DatabaseCollection.Find(m => m.TestDataID == testId).ToList();
        }

        /// <summary>
        /// Lấy kết quả thi của địa chỉ Ip nào đó
        /// </summary>
        /// <param name="ipAddress">Địa chỉ Ip của máy cần tìm</param>
        /// <param name="testDataId">Id của bài thi cần tìm the Ip máy</param>
        /// <returns></returns>
        public TestResult GetTestResult(string ipAddress, string testDataId)
        {
            return DatabaseCollection.Find(m => m.StudentIPAddress == ipAddress && m.TestDataID == testDataId).FirstOrDefault();
        }
    }
}
