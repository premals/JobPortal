using JobSeekerService.Application.Interfaces;
using JobSeekerService.Domain.Entities;
using MongoDB.Driver;

namespace JobSeekerService.Infrastructure.Mongo
{
    public class ResumeDraftRepository : IResumeDraftRepository
    {
        private readonly IMongoCollection<ResumeDraft> _collection;

        public ResumeDraftRepository(MongoDbContext context)
        {
            _collection = context.ResumeDrafts;
        }

        public async Task<ResumeDraft?> GetByUserIdAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return null;

            return await _collection
                .Find(draft => draft.UserId == userId)
                .FirstOrDefaultAsync();
        }

        public async Task UpsertAsync(ResumeDraft draft)
        {
            ArgumentNullException.ThrowIfNull(draft);

            if (draft.CreatedAt == default)
                draft.CreatedAt = DateTime.UtcNow;

            draft.UpdatedAt = DateTime.UtcNow;

            await _collection.ReplaceOneAsync(
                d => d.UserId == draft.UserId,
                draft,
                new ReplaceOptions { IsUpsert = true });
        }
    }
}
