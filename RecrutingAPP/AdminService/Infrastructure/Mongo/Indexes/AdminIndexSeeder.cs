using AdminService.Domain.Entities;
using MongoDB.Driver;

namespace AdminService.Infrastructure.Mongo.Indexes
{
    public class AdminIndexSeeder
    {
        private readonly IMongoDatabase _database;

        public AdminIndexSeeder(IMongoDatabase database)
        {
            _database = database;
        }

        public async Task CreateIndexesAsync()
        {
            var jobSeekers = _database.GetCollection<AdminJobSeekerProjection>("AdminJobSeekers");
            var jobProviders = _database.GetCollection<AdminJobProviderProjection>("AdminJobProviders");

            await jobSeekers.Indexes.CreateOneAsync(new CreateIndexModel<AdminJobSeekerProjection>(
                Builders<AdminJobSeekerProjection>.IndexKeys.Ascending(x => x.UserId),
                new CreateIndexOptions { Unique = true, Name = "UX_AdminJobSeeker_UserId" }));

            await jobProviders.Indexes.CreateOneAsync(new CreateIndexModel<AdminJobProviderProjection>(
                Builders<AdminJobProviderProjection>.IndexKeys.Ascending(x => x.JobProviderId),
                new CreateIndexOptions { Unique = true, Name = "UX_AdminJobProvider_ProviderId" }));

            // _id indexes are created automatically by MongoDB; don't create explicit unique indexes for them.
        }
    }
}