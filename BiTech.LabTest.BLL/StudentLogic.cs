using BiTech.LabTest.DAL;
using BiTech.LabTest.DAL.Models;
using BiTech.LabTest.DAL.Respository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BiTech.LabTest.DAL.Models.TestData;

namespace BiTech.LabTest.BLL
{
    public class StudentLogic
    {
        private IEntityRepository<TestData> TestDataRepository { get; set; }
        private IEntityRepository<TestResult> TestResultRepository { get; set; }
        private IDatabase Database { get; set; }
        private string TABLE_TESTDATA = "testdata";
        private string TABLE_TESTRESULT = "testresult";

        public StudentLogic()
        {
            var connectionString = Tool.GetConfiguration("ConnectionString");
            var databaseName = Tool.GetConfiguration("DatabaseName");
            Database = new Database(connectionString, databaseName);
            
        }

        /// <summary>
        /// Lấy bài thi
        /// </summary>
        /// <param name="testId">Mã bài thi trong csdl</param>
        /// <returns>Nội dung bài thi</returns>
        public TestData GetTest(string testId)
        {
            this.TestDataRepository = new EntityRepository<TestData>(Database, TABLE_TESTDATA);
            return TestDataRepository.GetById(testId);
        }

        /// <summary>
        /// Lấy giai đoạn thi
        /// </summary>
        /// <param name="testDataID">Mã đề thi trong csdl</param>
        /// <returns>trạng thái của Kì thi</returns>
        public TestStepEnum GetTestStep(string testDataID)
        {
            return TestDataRepository.GetById(testDataID)?.TestStep ?? TestStepEnum.Waiting;
        }

        /// <summary>
        /// Lấy thông tin bài thi của thí sinh theo ID
        /// </summary>
        /// <param name="testrsultId">Mã bài thi trong csdl</param>
        /// <returns>Nội dung bài thi</returns>
        public TestResult GetTestResult(string testrsultId)
        {
            this.TestResultRepository = new EntityRepository<TestResult>(Database, TABLE_TESTRESULT);
            return TestResultRepository.GetById(testrsultId);
        }

        /// <summary>
        /// Lưu kết quả thi của HS lần đầu
        /// </summary>
        /// <param name="testResult">Nội dung bài thi</param>
        /// <returns>Mã bài thi trong csdl</returns>
        public string SaveTestResult(TestResult testResult)
        {
            this.TestResultRepository = new EntityRepository<TestResult>(Database, TABLE_TESTRESULT);
            return TestResultRepository.Insert(testResult);
        }


        /// <summary>
        /// Lưu kết quả thi của HS Cập nhât
        /// </summary>
        /// <param name="testResult">Nội dung bài thi</param>
        /// <param name="id">Mã bài thi trong csdl</param>
        public bool UpdateTestResult(TestResult testResult, string id)
        {
            this.TestResultRepository = new EntityRepository<TestResult>(Database, TABLE_TESTRESULT);
            return TestResultRepository.Update(testResult, id);
        }
    }
}
