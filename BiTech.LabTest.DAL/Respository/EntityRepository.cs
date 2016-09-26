using BiTech.LabTest.DAL.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;

namespace BiTech.LabTest.DAL.Respository
{
    public class EntityRepository<T> : IEntityRepository<T> where T : IModel
    {
        /// <summary>
        /// Table name in Database
        /// </summary>
        private string TableName { get; set; }

        private IMongoCollection<T> DatabaseCollection { get; set; }

        public IMongoDatabase Database { get; set; }


        public EntityRepository(IDatabase database, string tableName)
        {
            Database = (IMongoDatabase)database.GetConnection();
            DatabaseCollection = Database.GetCollection<T>(tableName);
        }


        public T GetById(string id)
        {
            return DatabaseCollection.Find(m => m.Id == id).FirstOrDefault();
        }

        public string Insert(T entity)
        {
            DatabaseCollection.InsertOne(entity);
            return entity.Id.ToString();
        }

        public bool Remove(string id)
        {
            throw new NotImplementedException();
        }

        public bool Update(T entity)
        {
            var updateResult = DatabaseCollection.ReplaceOne<T>(m => m.Id == entity.Id, entity);
            return (updateResult.ModifiedCount > 0);
        }
    }
}
