using BiTech.LabTest.BLL.Interfaces;
using BiTech.LabTest.DAL;
using BiTech.LabTest.DAL.Interfaces;
using BiTech.LabTest.DAL.Models;
using BiTech.LabTest.DAL.Respository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiTech.LabTest.BLL
{
    public class TeacherLogic : ITeacherLogic
    {
        public IEntityRepository<TestData> TeacherRepository { get; set; }
        public IDatabase Database { get; set; }

        public TeacherLogic()
        {
            var connectionString = Tool.GetConfiguration("ConnectionString");
            var databaseName     = Tool.GetConfiguration("DatabaseName");
            Database             = new Database(connectionString, databaseName);


            this.TeacherRepository = new EntityRepository<TestData>(Database, "testdata");
        }

        /// <summary>
        /// Testdata cần để tổ chức thi
        /// </summary>
        /// <param name="testData"></param>
        /// <returns></returns>
        public string StartTest(string testData)
        {
            var entity            = new TestData();
            entity.Data           = testData;
            entity.RecordDateTime = DateTime.Now;
            entity.TestStep       = TestData.TestStepEnum.OnWorking;

            // Lưu vào database
            return TeacherRepository.Insert(entity);
        }
    }
}
