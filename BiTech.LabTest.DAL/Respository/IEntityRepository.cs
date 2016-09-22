using BiTech.LabTest.DAL.Models;
using MongoDB.Driver;

namespace BiTech.LabTest.DAL.Respository
{
    public interface IEntityRepository<IEntity> where IEntity : IModel
    {
        /// <summary>
        /// Database connection
        /// </summary>
        IMongoDatabase Database { get; set; }

        /// <summary>
        /// Thêm mới vào Database
        /// </summary>
        /// <param name="entity">Entity cần thêm vào Database</param>
        /// <returns>Id của Entity đã được thêm vào Database</returns>
        string Insert(IEntity entity);

        /// <summary>
        /// Xóa Entity trong database
        /// </summary>
        /// <param name="id">Id của entity cần xóa</param>
        /// <returns>Kết quả xóa entity</returns>
        bool Remove(string id);

        /// <summary>
        /// Cập nhật entity trong database
        /// </summary>
        /// <param name="entity">Entity cần cập nhật</param>
        /// <param name="Id">Id của Entity</param>
        /// <returns>Kết quả cập nhật entity</returns>
        bool Update(IEntity entity, string Id);

        /// <summary>
        /// Lấy entity ra bằng ID
        /// </summary>
        /// <param name="id">Id của entity cần tìm</param>
        /// <returns>Entity đã tìm được</returns>
        IEntity GetById(string id);
    }
}
