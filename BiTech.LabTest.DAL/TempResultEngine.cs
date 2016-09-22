using BiTech.LabTest.DAL.Models;
using BiTech.LabTest.DAL.Respository;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiTech.LabTest.DAL
{
    public class TempResultEngine : EntityRepository<TestResult>
    {
        private IMongoCollection<TestResult> DatabaseCollection { get; set; }


        public TempResultEngine(IDatabase database, string tableName) : base(database, tableName)
        {
            Database = (IMongoDatabase)database.GetConnection();
            DatabaseCollection = Database.GetCollection<TestResult>(tableName);
        }

        /// <summary>
        /// Lấy TestResult theo IP máy học sinh
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public TestResult GetByIpAddress(string ip)
        {
            return DatabaseCollection.Find(m => m.StudentIPAdd == ip).SortByDescending(m=> m.Id).FirstOrDefault();
        }
    }
}
