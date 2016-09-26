using BiTech.LabTest.DAL;
using BiTech.LabTest.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiTech.LabTest.BLL
{
    public class StudentLogic
    {
        private TestDataEngine TestDataEngine { get; set; }
        private OnlineStudentEngine OnlineStudentEngine { get; set; }
        private TestResultEngine TestResultEngine { get; set; }

        private IDatabase Database { get; set; }

        public StudentLogic()
        {
            var connectionString = Tool.GetConfiguration("ConnectionString");
            var databaseName     = Tool.GetConfiguration("DatabaseName");
            Database             = new Database(connectionString, databaseName);


            this.TestDataEngine      = new TestDataEngine(Database, "testdata");
            this.OnlineStudentEngine = new OnlineStudentEngine(Database, "onlinestudent");
            this.TestResultEngine    = new TestResultEngine(Database, "testresult");
        }

        /// <summary>
        /// Kiểm tra bài test cuối cùng đã thi xong hay chưa
        /// </summary>
        /// <returns></returns>
        public bool IsLastTestFinished(out TestData lastTest)
        {
            var testDataEngine = new TestDataEngine(Database,"testdata");
            var dbLastTest       = testDataEngine.GetLastTest();

            if (dbLastTest != null)
            {
                lastTest = dbLastTest;
                return (dbLastTest.TestStep == TestData.TestStepEnum.Finish);
            }

            lastTest = null;
            return true;
        }

        /// <summary>
        /// Kiểm tra Ip hiện tại có nằm trong danh sách đang thi của 1 bài Test được chỉ định hay ko
        /// </summary>
        /// <param name="ipAddress">Địa chỉ Ip cần kiểm tra</param>
        /// <returns></returns>
        public bool IsCurrentIpOnTestProgress(string ipAddress, string testDataId, out TestResult outTestResult)
        {
            var testResult = outTestResult  = TestResultEngine.GetTestResult(ipAddress, testDataId);


            if (testResult == null)
            {
                return false;
            }

            // Test đang thi
            return (testResult.TestStep == TestData.TestStepEnum.OnWorking);
        }

        public void UpdateLastOnline(string ipAddress)
        {
            var lastTest = this.TestDataEngine.GetLastTest();
            if (lastTest != null)
            {
                TestResult testResult = null;
                // Thí sinh đang thi thì update trong bảng "testresult"
                if (IsCurrentIpOnTestProgress(ipAddress, lastTest.Id, out testResult))
                {
                    // Cập nhật Online trong bảng "testresult"
                    var testResultRecord = TestResultEngine.GetTestResult(ipAddress, lastTest.Id);

                    if (testResultRecord != null)
                    {
                        // Cập nhật thời gian Online lại.
                        testResultRecord.RecordDateTime = DateTime.Now;
                        testResultRecord.TestStep = TestData.TestStepEnum.OnWorking;

                        TestResultEngine.Update(testResultRecord);
                    }
                }
                // Thí sinh đang chờ thi, update trong bảng "onlinestudent"
                else
                {
                    var currentRecord = this.OnlineStudentEngine.GetByIpAddress(ipAddress);
                    // Cập nhật thời gian Online lại.
                    currentRecord.RecordDateTime = DateTime.Now;
                    this.OnlineStudentEngine.Update(currentRecord);
                }
            }
            else
            {
                // Update trong bảng "onlinestudent"
                var currentOnlineRecord = this.OnlineStudentEngine.GetByIpAddress(ipAddress);
                // Cập nhật thời gian Online lại.
                currentOnlineRecord.RecordDateTime = DateTime.Now;
                this.OnlineStudentEngine.Update(currentOnlineRecord);
            }
        }

        /// <summary>
        /// Cập nhật thời gian Online gần nhất của User
        /// Nếu chưa có record nào trong database thì Method sẽ tự thêm mới
        /// Nếu ghi đã nhận được đề thi thì cần
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="fullName"></param>
        /// <param name="studentClass"></param>
        /// <param name="testDataId"></param>
        public void UpdateLastOnline(string ipAddress, string fullName, string studentClass, TestData.TestStepEnum testStep, string testDataId = null)
        {
            if (string.IsNullOrEmpty(ipAddress)) return;

            // Nếu TestStep là Waiting thì cập nhật trong data-table "onlinestudent"
            if (testStep == TestData.TestStepEnum.Waiting)
            {
                var currentRecord = this.OnlineStudentEngine.GetByIpAddress(ipAddress);

                // Ip chưa được ghi nhận
                if (currentRecord == null)
                {
                    // Thêm mới vào database
                    this.OnlineStudentEngine.Insert(new TestResult
                    {
                        StudentName = fullName,
                        StudentIPAddress = ipAddress,
                        StudentClass = studentClass,
                        RecordDateTime = DateTime.Now,
                        TestStep = testStep
                    });
                }
                else
                {
                    // Cập nhật thời gian Online lại.
                    currentRecord.StudentName = fullName;
                    currentRecord.StudentClass = studentClass;
                    currentRecord.RecordDateTime = DateTime.Now;
                    currentRecord.TestStep = testStep;
                    this.OnlineStudentEngine.Update(currentRecord);
                }
            }
            // Nếu TestStep là OnWorking thì cập nhật trong data-table "test-result"
            else
            {
                var lastTest = new TestDataEngine(Database, "testdata").GetById(testDataId);

                if (lastTest != null && lastTest.TestStep == TestData.TestStepEnum.OnWorking)
                {
                    // Cập nhật Online trong bảng "testresult"
                    var testResultRecord = TestResultEngine.GetTestResult(ipAddress, lastTest.Id);

                    if (testResultRecord != null)
                    {
                        // Cập nhật thời gian Online lại.
                        testResultRecord.RecordDateTime = DateTime.Now;
                        testResultRecord.TestStep       = TestData.TestStepEnum.OnWorking;

                        TestResultEngine.Update(testResultRecord);
                    }
                }
            }
        }
    }
}
