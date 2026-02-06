using JobProviderService.Application.Interfaces;
using JobProviderService.Domain;
using MongoDB.Driver;

namespace JobProviderService.Infrastructure.Repository
{
    public class JobProviderProfileRepository : IJobProviderProfileRepository
    {
        private readonly IMongoCollection<JobProviderProfile> _collection;

        public JobProviderProfileRepository(MongoDbContext context)
        {
            _collection = context.JobProviderProfiles;
        }

        public async Task<JobProviderProfile?> GetByProviderIdAsync(string providerId)
        {
            return await _collection.Find(x => x.JobProviderId == providerId)
                .FirstOrDefaultAsync();
        }

        public async Task<JobProviderProfile> UpsertAsync(JobProviderProfile profile)
        {
            profile.UpdatedAt = DateTime.UtcNow;

            await _collection.ReplaceOneAsync(
                x => x.JobProviderId == profile.JobProviderId,
                profile,
                new ReplaceOptions { IsUpsert = true });

            return profile;
        }
    }
}
