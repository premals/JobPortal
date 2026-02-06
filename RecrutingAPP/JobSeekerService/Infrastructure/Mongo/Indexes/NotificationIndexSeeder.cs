using JobSeekerService.Domain.Entities;
using MongoDB.Driver;

namespace JobSeekerService.Infrastructure.Mongo.Indexes
{
    public class NotificationIndexSeeder
    {
        private readonly IMongoCollection<Notification> _collection;

        public NotificationIndexSeeder(IMongoDatabase database)
        {
            _collection = database.GetCollection<Notification>("Notifications");
        }

        public async Task CreateIndexesAsync()
        {
            var indexModels = new List<CreateIndexModel<Notification>>
            {
                new(
                    Builders<Notification>.IndexKeys
                        .Ascending(n => n.JobSeekerId)
                        .Descending(n => n.CreatedAt),
                    new CreateIndexOptions
                    {
                        Name = "IX_JobSeeker_CreatedAt"
                    }
                )
            };

            await _collection.Indexes.CreateManyAsync(indexModels);
        }
    }
}
