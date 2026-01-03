using JobSeekerService.Domain.Entities;
using MongoDB.Driver;

namespace JobSeekerService.Infrastructure.Mongo.Indexes
{
    public class JobSeekerIndexSeeder
    {
        private readonly IMongoCollection<JobSeekerProfile> _collection;

        public JobSeekerIndexSeeder(IMongoDatabase database)
        {
            _collection = database.GetCollection<JobSeekerProfile>("JobSeekers");
        }

        public async Task CreateIndexesAsync()
        {
            var indexModels = new List<CreateIndexModel<JobSeekerProfile>>
        {
            new(
                Builders<JobSeekerProfile>.IndexKeys
                    .Ascending(p => p.UserId),
                new CreateIndexOptions
                {
                    Unique = true,
                    Name = "IX_JobSeekers_UserId"
                }
            )
        };

            await _collection.Indexes.CreateManyAsync(indexModels);
        }
    }
}
