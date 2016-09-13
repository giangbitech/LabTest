namespace BiTech.LabTest.DAL
{
    public interface IDatabase
    {
        /// <summary>
        /// Thông số kế nối database
        /// </summary>
        string ConnectionString { get; set; }

        /// <summary>
        /// Tên Database
        /// </summary>
        string DatabaseName { get; set; }

        object GetConnection();
    }
}
