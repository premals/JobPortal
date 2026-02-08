using JobSeekerService.Domain.Entities;
using MongoDB.Driver;

namespace JobSeekerService.Infrastructure.Mongo.Indexes
{
    public class ResumeDraftIndexSeeder
    {
        private readonly IMongoCollection<ResumeDraft> _collection;

        public ResumeDraftIndexSeeder(IMongoDatabase database)
        {
            _collection = database.GetCollection<ResumeDraft>("ResumeDrafts");
        }

        public async Task CreateIndexesAsync()
        {
            var indexModels = new List<CreateIndexModel<ResumeDraft>>
            {
                new(
                    Builders<ResumeDraft>.IndexKeys.Ascending(draft => draft.UserId),
                    new CreateIndexOptions
                    {
                        Unique = true,
                        Name = "IX_ResumeDrafts_UserId"
                    }
                )
            };

            await _collection.Indexes.CreateManyAsync(indexModels);
        }
    }
}
