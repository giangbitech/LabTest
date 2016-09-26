using BiTech.LabTest.DAL.Models;
using BiTech.LabTest.DAL.Respository;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiTech.LabTest.DAL
{
    public class OnlineStudentEngine : EntityRepository<TestResult>
    {
        private IMongoCollection<TestResult> DatabaseCollection { get; set; }


        public OnlineStudentEngine(IDatabase database, string tableName) : base(database, tableName)
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
        /// Nếu chưa có record nào đúng với ip cần tìm thì nó se tự động Insert record mới và trả về
        /// </summary>
        /// <param name="ipAddress">Địa chỉ Ip cần cập nhật trạng thái Online</param>
        /// <returns></returns>
        public TestResult GetByIpAddress(string ipAddress)
        {
            return DatabaseCollection.Find(m => m.StudentIPAddress == ipAddress).FirstOrDefault();
        }

        public List<TestResult> GetOnlineStudents()
        {
            //todo: lấy điểm thi sau khi đã Finished bài thi
            return this.DatabaseCollection.Find(new BsonDocument()).ToList();
        }

        /// <summary>
        /// Xóa dữ liệu Online của thí sinh đang ở bước chờ thi.
        /// Chừa lại các record đã có điểm thi của thí sinh.
        /// </summary>
        /// <param name="ipAddress">Địa chỉ Ip cần xóa</param>
        /// <returns></returns>
        public bool DeleteOnlineRecordsByIp(string ipAddress)
        {
            return (this.DatabaseCollection.DeleteMany<TestResult>(m => m.StudentIPAddress == ipAddress && m.TestStep == TestData.TestStepEnum.Waiting).DeletedCount > 0);
        }
    }
}
