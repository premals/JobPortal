using JobSeekerService.Application.Interfaces;
using JobSeekerService.Domain.Entities;
using MongoDB.Driver;

namespace JobSeekerService.Infrastructure.Mongo
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly IMongoCollection<Notification> _collection;

        public NotificationRepository(MongoDbContext context)
        {
            _collection = context.Notifications;
        }

        public async Task AddAsync(Notification notification)
        {
            ArgumentNullException.ThrowIfNull(notification);
            await _collection.InsertOneAsync(notification);
        }

        public async Task<List<Notification>> GetBySeekerAsync(string seekerId, int limit = 50)
        {
            if (string.IsNullOrWhiteSpace(seekerId))
                return new List<Notification>();

            return await _collection
                .Find(n => n.JobSeekerId == seekerId)
                .SortByDescending(n => n.CreatedAt)
                .Limit(limit)
                .ToListAsync();
        }

        public async Task MarkReadAsync(string notificationId, string seekerId)
        {
            if (string.IsNullOrWhiteSpace(notificationId) || string.IsNullOrWhiteSpace(seekerId))
                return;

            var filter = Builders<Notification>.Filter.And(
                Builders<Notification>.Filter.Eq(n => n.Id, notificationId),
                Builders<Notification>.Filter.Eq(n => n.JobSeekerId, seekerId)
            );

            var update = Builders<Notification>.Update
                .Set(n => n.IsRead, true);

            await _collection.UpdateOneAsync(filter, update);
        }
    }
}
