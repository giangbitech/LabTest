using MongoDB.Driver;

namespace BiTech.LabTest.DAL
{
    public class Database : IDatabase
    {
        public static IMongoClient _client;
        public static IMongoDatabase _database;
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }

        public Database(string connectionString, string databaseName)
        {
            if (_client == null)
            {
                _client = new MongoClient(connectionString);
            }

            if (_database == null)
            {
                _database = _client.GetDatabase(databaseName);
            }

        }

        public object GetConnection()
        {
            return _database;
        }
    }
}
