using MongoDB.Driver;
using Users.Domain.Entities;

namespace Users.Infrastructure.Persistence.Context
{
    public class MongoSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string Database { get; set; } = string.Empty;
    }

    public class MongoContext
    {
        private readonly IMongoDatabase _database;

        public MongoContext(IMongoClient client, MongoSettings settings)
        {
            _database = client.GetDatabase(settings.Database);
        }

        public IMongoCollection<User> Users => _database.GetCollection<User>("users");
    }
}
