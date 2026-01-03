using JobSeekerService.Domain.Entities;
using MongoDB.Driver;

namespace JobSeekerService.Infrastructure.Mongo.Indexes
{
    public class JobReadIndexSeeder
    {
        private readonly IMongoCollection<JobSnapshot> _collection;

        public JobReadIndexSeeder(IMongoDatabase database)
        {
            _collection = database.GetCollection<JobSnapshot>("Jobs");
        }

        public async Task CreateIndexesAsync()
        {
            var indexModels = new List<CreateIndexModel<JobSnapshot>>
        {
            new(
                Builders<JobSnapshot>.IndexKeys
                    .Ascending(j => j.Status)
                    .Descending(j => j.PostedAt)
            ),
            new(
                Builders<JobSnapshot>.IndexKeys
                    .Ascending(j => j.City)
            )
        };

            await _collection.Indexes.CreateManyAsync(indexModels);
        }
    }
}
