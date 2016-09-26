using System.Collections.Generic;
using Microsoft.AspNet.SignalR;
using static BiTech.LabTest.Models.ViewModels.Teacher;
using BiTech.LabTest.DAL.Models;
using BiTech.LabTest.BLL;

namespace BiTech.LabTest.Realtime
{
    public class TestHub : Hub
    {
        public void TeacherAnnounceTestOpened(string testDataId)
        {
            Clients.AllExcept(new string[] { this.Context.ConnectionId }).announceStudentStartTest(testDataId);
        }

        public void StudentUpdateOnline(string ipAddress)
        {
            new BLL.StudentLogic().UpdateLastOnline(ipAddress);
        }

        public void TeacherRequestOnlineStatistics(string testId)
        {
            var dbOnlineStudents = new BLL.TeacherLogic().GetOnlineStudents(testId);

            int orderNumber = 1;
            var viewModel   = new List<OnlineStudentViewModel>();
            foreach (var item in dbOnlineStudents)
            {
                string mark = (item.TestStep == TestData.TestStepEnum.Finish ) ? item.Score.ToString() : "";

                var testStepDescription = "";

                #region Mô tả trạng thái tài khoản của thí sinh
                switch (item.TestStep)
                {
                    case TestData.TestStepEnum.Waiting:
                        testStepDescription = LanguageConfig.Dictionary["Teacher_Description_Waiting_Test"];
                        break;
                    case TestData.TestStepEnum.OnWorking:
                        testStepDescription = LanguageConfig.Dictionary["Teacher_Description_Doing_Test"];
                        break;
                    case TestData.TestStepEnum.Finish:
                        testStepDescription = LanguageConfig.Dictionary["Teacher_Description_Finished_Test"];
                        break;
                    default:
                        break;
                }
                #endregion

                viewModel.Add(new OnlineStudentViewModel
                {
                    OrderNumber         = orderNumber,
                    FullName            = item.StudentName,
                    StudentClass        = item.StudentClass,
                    Mark                = mark,
                    IpAddress           = item.StudentIPAddress,
                    TestStep            = item.TestStep,
                    TestStepDescription = testStepDescription
                });

                orderNumber += 1;
            }

            Clients.Client(this.Context.ConnectionId).teacherRetrieveOnlineStatistics(viewModel);
        }
    }
}