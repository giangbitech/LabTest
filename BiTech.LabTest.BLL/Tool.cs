using BiTech.LabTest.DAL.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using static BiTech.LabTest.DAL.Models.TestData;

namespace BiTech.LabTest.BLL
{
    public class Tool
    {
        public static string GetConfiguration(string key)
        {
            return ConfigurationManager.AppSettings[key]?.ToString();
        }


        /// <summary>
        /// Thead lấy random int an toàn
        /// </summary>
        public static class ThreadSafeRandom
        {
            [ThreadStatic]
            private static Random Local;

            public static Random ThisThreadsRandom
            {
                get { return Local ?? (Local = new Random(unchecked(Environment.TickCount * 31 + System.Threading.Thread.CurrentThread.ManagedThreadId))); }
            }
        }

        /// <summary>
        /// Trộn random List<object>
        /// </summary>
        /// <param name="list"></param>
        public static void ShuffleList(List<object> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
                object value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }


        /// <summary>
        /// Mã hóa Base 64
        /// </summary>
        /// <param name="plainText"></param>
        /// <returns></returns>
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        /// <summary>
        /// Giải mã Base 64
        /// </summary>
        /// <param name="base64EncodedData"></param>
        /// <returns></returns>
        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }


        /// <summary>
        /// Kiểm tra cookie backup, coi có đúng là học sinh bị mất kết nôi
        /// </summary>
        /// <returns>có phải học sinh bị mất kết nối không</returns>
        public static StudentBaseInfo CheckReconnection(string ip)
        {
            StudentBaseInfo studentBaseInfo = new StudentBaseInfo();
            StudentLogic studentLogic = new StudentLogic();
            

            TestResult testResult = studentLogic.GetTestResultTempByIp(ip);
            TestResult testResultFinal = studentLogic.GetTestResultByIp(ip);
            if (testResultFinal != null)
            {
                studentBaseInfo.Score = testResultFinal.Score;
            }
            if (testResult == null)
            {
                return null;
            }
            else
            {
                studentBaseInfo = new StudentBaseInfo();
                studentBaseInfo.TestResultID = testResult.Id.ToString();
                studentBaseInfo.TestDataID = testResult.TestDataID;
                studentBaseInfo.StudentName = testResult.StudentName;
                studentBaseInfo.StudentClass = testResult.StudentClass;
                studentBaseInfo.Score = testResult.Score;
                studentBaseInfo.StudentIPAdd = ip;

                TestStepEnum testStep = TestStepEnum.Waiting;
                if (studentBaseInfo.TestDataID != null)
                    testStep = studentLogic.GetTestStep(studentBaseInfo.TestDataID);

                if (testStep == TestStepEnum.Finish) //Nếu thi xong rồi thi xóa hết cookie
                {
                    return null;
                }
                return studentBaseInfo;
            }
        }

        /// <summary>
        /// Trộn random List<PossibleAnswerViewModel>
        /// </summary>
        /// <param name="list"></param>
        //private void ShuffleList(List<PossibleAnswerViewModel> list)
        //{
        //    int n = list.Count;
        //    while (n > 1)
        //    {
        //        n--;
        //        int k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
        //        PossibleAnswerViewModel value = list[k];
        //        list[k] = list[n];
        //        list[n] = value;
        //    }
        //}

    }
}
