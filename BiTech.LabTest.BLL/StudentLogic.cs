using BiTech.LabTest.DAL;
using BiTech.LabTest.DAL.Models;
using BiTech.LabTest.DAL.Respository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiTech.LabTest.BLL
{
    public class StudentLogic
    {
        public IEntityRepository<TestData> TestDataRepository { get; set; }
        public IDatabase Database { get; set; }

        public StudentLogic()
        {
            var connectionString = Tool.GetConfiguration("ConnectionString");
            var databaseName = Tool.GetConfiguration("DatabaseName");
            Database = new Database(connectionString, databaseName);
            
        }

        public TestData GetTest(string testId)
        {
            this.TestDataRepository = new EntityRepository<TestData>(Database, "testdata");

            return TestDataRepository.GetById(testId);
        }
    }
}
