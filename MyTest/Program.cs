using BiTech.LabTest.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTest
{
    class Program
    {
        static void Main(string[] args)
        {

        }

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
            controller.gxetxxx("57e343b71c5ccc1bf41f4565");
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

    }
}
