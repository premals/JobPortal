using JobSeekerService.Application.Interfaces;
using JobSeekerService.Domain.Entities;
using MongoDB.Driver;

namespace JobSeekerService.Infrastructure.Mongo
{
    public class JobSeekerRepository : IJobSeekerRepository
    {
        private readonly IMongoCollection<JobSeekerProfile> _collection;

        public JobSeekerRepository(MongoDbContext context)
        {
            _collection = context.JobSeekers;
        }

        // ------------------------------------
        // GET PROFILE BY USER ID
        // ------------------------------------
        public async Task<JobSeekerProfile?> GetByUserIdAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return null;

            return await _collection
                .Find(p => p.UserId == userId)
                .FirstOrDefaultAsync();
        }

        // ------------------------------------
        // CREATE PROFILE
        // ------------------------------------
        public async Task CreateAsync(JobSeekerProfile profile)
        {
            ArgumentNullException.ThrowIfNull(profile);

            profile.CreatedAt = DateTime.UtcNow;
            profile.UpdatedAt = DateTime.UtcNow;

            await _collection.InsertOneAsync(profile);
        }

        // ------------------------------------
        // UPDATE PROFILE
        // ------------------------------------
        public async Task UpdateAsync(JobSeekerProfile profile)
        {
            ArgumentNullException.ThrowIfNull(profile);

            profile.UpdatedAt = DateTime.UtcNow;

            var result = await _collection.ReplaceOneAsync(
                p => p.UserId == profile.UserId,
                profile);

            if (result.MatchedCount == 0)
                throw new InvalidOperationException("Job seeker profile not found");
        }
    }
}
