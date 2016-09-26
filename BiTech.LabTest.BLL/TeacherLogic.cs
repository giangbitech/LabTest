using BiTech.LabTest.DAL;
using BiTech.LabTest.DAL.Models;
using BiTech.LabTest.DAL.Respository;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BiTech.LabTest.BLL
{
    public class TeacherLogic
    {
        private IEntityRepository<TestData> TeacherRepository { get; set; }
        private IDatabase Database { get; set; }

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

        /// <summary>
        /// Lấy danh sách thí sinh đang trong phòng thi
        /// </summary>
        /// <returns></returns>
        public List<TestResult> GetOnlineStudents(string testId)
        {
            // Danh sách các thí sinh đang chờ thi & đang thi
            var dbOnlineStudents = new OnlineStudentEngine(Database, "onlinestudent").GetOnlineStudents();

            // ViewModel trả về cho caller
            var onlineStudents = new List<TestResult>();
            // Danh sách các thí sinh Đang chờ thi & Đang thi
            foreach (var student in dbOnlineStudents)
            {
                var lastOnlineSeconds = DateTime.Now.Subtract(student.RecordDateTime).TotalSeconds;
                // Lấy khoảng thời gian để xác định User còn Online hay k ?
                int onlineDuration = int.Parse(Tool.GetConfiguration("OnlineDuration"));

                if (lastOnlineSeconds < onlineDuration)
                {
                    onlineStudents.Add(new TestResult
                    {
                        StudentName      = student.StudentName,
                        StudentIPAddress = student.StudentIPAddress,
                        RecordDateTime   = student.RecordDateTime,
                        StudentClass     = student.StudentClass,
                        TestStep         = student.TestStep,
                        Score            = student.Score
                    });
                }
            }

            var testResultEngine = new TestResultEngine(Database, "testresult");
            // Danh sách các thí sinh đã thi xong + điểm
            if (string.IsNullOrEmpty(testId) == false)
            {
                var doneTestStudents = testResultEngine.GetAllByTestId(testId);
                foreach (var student in doneTestStudents)
                {
                    onlineStudents.Add(new TestResult
                    {
                        StudentName      = student.StudentName,
                        StudentIPAddress = student.StudentIPAddress,
                        RecordDateTime   = student.RecordDateTime,
                        StudentClass     = student.StudentClass,
                        TestStep         = TestData.TestStepEnum.Finish,
                        Score            = student.Score
                    });
                }
            }


            // Sắp xếp theo thứ tự chữ cái ABC
            return onlineStudents.OrderBy(m => m.StudentName).ToList();
        }
    }
}
