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
        private string TABLE_TESTRESULT_TEMP = "testresulttemp";
        //private string TABLE_STUDENT_BASE_INFO = "studentbaseinfo";

        public StudentLogic()
        {
            var connectionString = Tool.GetConfiguration("ConnectionString");
            if (connectionString == null)
            {
                connectionString = @"mongodb://127.0.0.1:27017";
            }
            var databaseName = Tool.GetConfiguration("DatabaseName");
            if (databaseName == null)
            {
                databaseName = "labtest";
            }
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
            this.TestDataRepository = new EntityRepository<TestData>(Database, TABLE_TESTDATA);
            var testData = this.TestDataRepository.GetById(testDataID);
            return TestDataRepository.GetById(testDataID)?.TestStep ?? TestStepEnum.Waiting;
        }

        /// <summary>
        /// Lấy thông tin bài thi của thí sinh theo ID
        /// </summary>
        /// <param name="testResultId">Mã bài thi trong csdl</param>
        /// <returns>Nội dung bài thi</returns>
        public TestResult GetTestResult(string testResultId)
        {
            this.TestResultRepository = new EntityRepository<TestResult>(Database, TABLE_TESTRESULT);
            return TestResultRepository.GetById(testResultId);
        }

        /// <summary>
        /// Lưu kết quả thi của thí sinh
        /// </summary>
        /// <param name="testResult">Nội dung bài thi</param>
        /// <returns>Mã bài thi trong csdl</returns>
        public string SaveTestResult(TestResult testResult)
        {
            this.TestResultRepository = new EntityRepository<TestResult>(Database, TABLE_TESTRESULT);
            return TestResultRepository.Insert(testResult);
        }

        /// <summary>
        /// Lấy test result theo IP của học sinh
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public TestResult GetTestResultByIp(string ip)
        {
            TempResultEngine tempResultEngine = new TempResultEngine(Database, TABLE_TESTRESULT);
            return tempResultEngine.GetByIpAddress(ip);
        }

        /// <summary>
        /// Cập nhật kết quả thi của thí sinh
        /// </summary>
        /// <param name="testResult">Nội dung bài thi</param>
        /// <param name="id">Mã bài thi trong csdl</param>
        public bool UpdateTestResult(TestResult testResult, string id)
        {
            this.TestResultRepository = new EntityRepository<TestResult>(Database, TABLE_TESTRESULT);
            return TestResultRepository.Update(testResult, id);
        }

        /// <summary>
        /// Lấy thông tin bài thi lưu tạm của thí sinh theo IP máy
        /// </summary>
        /// <param name="testrsultId">Mã bài thi trong csdl</param>
        /// <returns>Nội dung bài thi</returns>
        public TestResult GetTestResultTempByIp(string ip)
        {
            TempResultEngine tempResultEngine = new TempResultEngine(Database, TABLE_TESTRESULT_TEMP);            
            return tempResultEngine.GetByIpAddress(ip);
        }

        /// <summary>
        /// Lấy thông tin bài thi lưu tạm của thí sinh theo ID
        /// </summary>
        /// <param name="testResultId">Mã bài thi trong csdl</param>
        /// <returns>Nội dung bài thi</returns>
        public TestResult GetTestResultTempByID(string testResultId)
        {
            this.TestResultRepository = new EntityRepository<TestResult>(Database, TABLE_TESTRESULT_TEMP);
            return TestResultRepository.GetById(testResultId);
        }
        
        /// <summary>
        /// Lưu kết quả thi lưu tạm của thí sinh
        /// </summary>
        /// <param name="testResult">Nội dung bài thi</param>
        /// <returns>Mã bài thi trong csdl</returns>
        public string SaveTestResultTemp(TestResult testResult)
        {
            this.TestResultRepository = new EntityRepository<TestResult>(Database, TABLE_TESTRESULT_TEMP);
            return TestResultRepository.Insert(testResult);
        }

        /// <summary>
        /// Cập nhật kết quả thi lưu tạm của thí sinh
        /// </summary>
        /// <param name="testResult">Nội dung bài thi</param>
        /// <param name="id">Mã bài thi trong csdl</param>
        public bool UpdateTestResultTemp(TestResult testResult, string id)
        {
            this.TestResultRepository = new EntityRepository<TestResult>(Database, TABLE_TESTRESULT_TEMP);
            return TestResultRepository.Update(testResult, id);
        }

        /// <summary>
        /// Xóa kết quả thi lưu tạm của thí sinh
        /// </summary>
        /// <param name="testResult">Nội dung bài thi</param>
        /// <param name="id">Mã bài thi trong csdl</param>
        public bool DeleteTestResultTemp(string id)
        {
            this.TestResultRepository = new EntityRepository<TestResult>(Database, TABLE_TESTRESULT_TEMP);
            return TestResultRepository.Remove(id);
        }
    }
}
