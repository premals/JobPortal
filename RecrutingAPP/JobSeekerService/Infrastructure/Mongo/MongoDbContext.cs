using JobSeekerService.Domain.Entities;
using MongoDB.Driver;

namespace JobSeekerService.Infrastructure.Mongo
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IMongoClient client, IConfiguration config)
        {
            _database = client.GetDatabase(config["Mongo:DatabaseName"]);
        }

        public IMongoCollection<JobSnapshot> Jobs =>
            _database.GetCollection<JobSnapshot>("Jobs");

        public IMongoCollection<JobSeekerProfile> JobSeekers =>
       _database.GetCollection<JobSeekerProfile>("JobSeekers");

        public IMongoCollection<JobApplication> JobApplications =>
        _database.GetCollection<JobApplication>("JobApplications");
    }
}
