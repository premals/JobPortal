using JobProviderService.Domain;
using MongoDB.Driver;

namespace JobProviderService.Infrastructure
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IMongoClient client, IConfiguration config)
        {
            _database = client.GetDatabase(config["Mongo:Database"]);
        }

        public IMongoCollection<Job> Jobs =>
            _database.GetCollection<Job>("Jobs");

        public IMongoCollection<JobApplication> JobApplication =>
           _database.GetCollection<JobApplication>("JobApplication");
    }
}
