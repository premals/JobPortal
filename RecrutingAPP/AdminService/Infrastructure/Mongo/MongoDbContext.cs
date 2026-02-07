using AdminService.Domain.Entities;
using MongoDB.Driver;

namespace AdminService.Infrastructure.Mongo
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IMongoDatabase database)
        {
            _database = database;
        }

        public IMongoCollection<AdminJobSeekerProjection> JobSeekers =>
            _database.GetCollection<AdminJobSeekerProjection>("AdminJobSeekers");

        public IMongoCollection<AdminJobProviderProjection> JobProviders =>
            _database.GetCollection<AdminJobProviderProjection>("AdminJobProviders");

        public IMongoCollection<AdminJobProjection> Jobs =>
            _database.GetCollection<AdminJobProjection>("AdminJobs");

        public IMongoCollection<AdminApplicationProjection> Applications =>
            _database.GetCollection<AdminApplicationProjection>("AdminApplications");
    }
}
