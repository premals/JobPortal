using JobProviderService.Domain;
using MongoDB.Driver;

namespace JobProviderService.Infrastructure
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IMongoClient client, IConfiguration config)
        {
            var baseName = config["Mongo:Database"];
            _database = client.GetDatabase(baseName);
        }

        public IMongoCollection<Job> Jobs =>
            _database.GetCollection<Job>("Jobs");

        public IMongoCollection<JobApplication> JobApplication =>
           _database.GetCollection<JobApplication>("JobApplication");

        public IMongoCollection<InterviewInvite> InterviewInvites =>
            _database.GetCollection<InterviewInvite>("InterviewInvites");

        public IMongoCollection<InterviewSession> InterviewSessions =>
            _database.GetCollection<InterviewSession>("InterviewSessions");

        public IMongoCollection<JobProviderSettings> JobProviderSettings =>
            _database.GetCollection<JobProviderSettings>("JobProviderSettings");

        public IMongoCollection<JobProviderProfile> JobProviderProfiles =>
            _database.GetCollection<JobProviderProfile>("JobProviderProfiles");

        
    }
}
