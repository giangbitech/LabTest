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
        public IEntityRepository<TestData> StudentRepository { get; set; }
        public IDatabase Database { get; set; }

        public StudentLogic()
        {
            var connectionString = Tool.GetConfiguration("ConnectionString");
            var databaseName = Tool.GetConfiguration("DatabaseName");
            Database = new Database(connectionString, databaseName);
            
            this.StudentRepository = new EntityRepository<TestData>(Database, "testdata");
        }

        public string GetTest(string testId)
        {
            /*;
            var connectionString = Tool.GetConfiguration("ConnectionString");
            var databaseName = Tool.GetConfiguration("DatabaseName");
            Database = new Database(connectionString, databaseName);


            this.StudentRepository = new EntityRepository<TestData>(Database, "testdata");
            TestData testdata  = StudentRepository.GetById(testId);*/
            return this.StudentRepository.GetById(testId).Data;
            //new EntityRepository(database, "testdata").GetById();
        }
    }
}
