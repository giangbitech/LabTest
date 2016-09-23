using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace BiTech.LabTest.Realtime
{
    public class TestHub : Hub
    {
        public void Send(string name, string message)
        {
            // Call the addNewMessageToPage method to update clients.
            Clients.All.broadcastMessage(name, message);
        }

        public void TeacherAnnounceTestOpened(string testDataId)
        {
            Clients.AllExcept(new string[] { this.Context.ConnectionId }).announceStudentStartTest(testDataId);
        }


    }
}