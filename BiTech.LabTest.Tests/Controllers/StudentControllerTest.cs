using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BiTech.LabTest;
using BiTech.LabTest.Controllers;
using System.Web;
using Moq;
using System.Web.Routing;
using System.Threading;
using System.Diagnostics;

namespace BiTech.LabTest.Tests.Controllers
{
    [TestClass]
    public class StudentControllerTest
    {
        //[TestMethod]
        public void WaitingScreen()
        {
            // Arrange
            StudentController controller = new StudentController();

            // Act
            ViewResult result = controller.WaitingScreen() as ViewResult;
            //controller.ControllerContext = 
            // Assert
            Assert.AreNotEqual(null, result.ViewBag.StudentName);
            Assert.AreNotEqual("", result.ViewBag.StudentName);
        }

        [TestMethod]
        public void SubmitDoTest()
        {
            Thread[] threads = new Thread[2];
            for (int i = 0; i < threads.Length; i++)
            {

                string x = i.ToString();
                ThreadWorker worker = new ThreadWorker(x);
                worker.ThreadDone += HandleThreadDone;

                threads[i] = new Thread(worker.Run);
                threads[i].Start();
            }
        }

        public void submitdotestwork(string i)
        {

            StudentController controller = new StudentController();
            //controller = SetFakeControllerContext(controller, i.ToString());
            // Act
            Console.WriteLine("s" + i);
            Trace.WriteLine("s" + i);
            ViewResult result2 = controller.gxetxxx("57e343b71c5ccc1bf41f4565") as ViewResult;
            Assert.IsNotNull(result2.ViewBag.DoneLoading);
            Console.WriteLine("x" + i);
            Trace.WriteLine("x" + i);


        }

        void HandleThreadDone(object sender, EventArgs e)
        {
            Console.WriteLine("xz");
            Trace.WriteLine("xz");
        }

        class ThreadWorker
        {
            public event EventHandler ThreadDone;
            public string i { get; set; }
            public ThreadWorker(string ix)
            {
                i = ix;
            }
            public void Run()
            {
                StudentController controller = new StudentController();
                //controller = SetFakeControllerContext(controller, i.ToString());
                // Act
                ViewResult result2 = controller.gxetxxx("57e343b71c5ccc1bf41f4565") as ViewResult;
                Assert.IsNotNull(result2.ViewBag.DoneLoading);
                Console.WriteLine("x" + i);
                Trace.WriteLine("x" + i);

                if (ThreadDone != null)
                    ThreadDone(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// A Class to allow simulation of SessionObject
        /// </summary>
        public class MockHttpSession : HttpSessionStateBase
        {
            Dictionary<string, object> m_SessionStorage = new Dictionary<string, object>();

            public override object this[string name]
            {
                get { return m_SessionStorage[name]; }
                set { m_SessionStorage[name] = value; }
            }


        }

        /// <summary>
        ///  Tạo http context giả cho SubmitDoTest với tên giả và lớp giả
        /// </summary>
        /// <param name="i">số thứ tự</param>
        /// <returns></returns>
        public HttpContextBase FakeHttpContext(string i)
        {
            var context = new Mock<HttpContextBase>();
            var request = new Mock<HttpRequestBase>();
            var response = new Mock<HttpResponseBase>();
            var session = new MockHttpSession();
            var server = new Mock<HttpServerUtilityBase>();
            

            //StudentBaseInfo studentBaseInfo = new StudentBaseInfo();
            //studentBaseInfo.StudentName = "ten-" + i;
            //studentBaseInfo.StudentClass = "lop";
            //session["studentbaseinfo"] = studentBaseInfo;

            context.Setup(ctx => ctx.Request).Returns(request.Object);
            context.Setup(ctx => ctx.Response).Returns(response.Object);
            context.Setup(ctx => ctx.Session).Returns(session);
            context.Setup(ctx => ctx.Server).Returns(server.Object);

            return context.Object;
        }

        /// <summary>
        /// tạo giả controller context
        /// </summary>
        /// <param name="controller">StudentController</param>
        /// <param name="i">số thứ tự</param>
        public StudentController SetFakeControllerContext(StudentController controller, string i)
        {
            var httpContext = FakeHttpContext(i);
            ControllerContext context = new ControllerContext(new RequestContext(httpContext, new RouteData()), controller);
            controller.ControllerContext = context;
            return controller;
        }
    }
}
