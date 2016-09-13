namespace BiTech.LabTest.BLL.Interfaces
{
    public interface ITeacherLogic
    {
        /// <summary>
        /// Bắt đầu tính giờ làm bài
        /// </summary>
        /// <param name="testData">Nội dung bài thi cần tổ chức thi</param>
        /// <returns>Id của bài test sau khi lưu vào database</returns>
        string StartTest(string testData);
    }
}
