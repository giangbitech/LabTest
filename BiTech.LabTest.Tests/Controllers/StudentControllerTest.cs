using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BiTech.LabTest;
using BiTech.LabTest.Controllers;

namespace BiTech.LabTest.Tests.Controllers
{
    [TestClass]
    public class StudentControllerTest
    {
        [TestMethod]
        public void WaitingScreen()
        {
            // Arrange
            StudentController controller = new StudentController();

            // Act
            ViewResult result = controller.WaitingScreen() as ViewResult;

            // Assert
            Assert.AreNotEqual(null, result.ViewBag.StudentName);
            Assert.AreNotEqual("", result.ViewBag.StudentName);
        }
    }
}
