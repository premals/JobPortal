using JobProviderService.Application.Interfaces;
using JobProviderService.Domain;
using MongoDB.Driver;

namespace JobProviderService.Infrastructure.Repository
{
    public class JobApplicationRepository: IJobApplicationRepository
    {
        private readonly IMongoCollection<JobApplication> _collection;

        public JobApplicationRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<JobApplication>("JobApplications");
        }

        public async Task<JobApplication?> GetAsync(string jobId, string jobSeekerId)
        {
            return await _collection.Find(x =>
                x.JobId == jobId &&
                x.JobSeekerId == jobSeekerId)
                .FirstOrDefaultAsync();
        }

        public async Task ApplyAsync(JobApplication application)
        {
            await _collection.InsertOneAsync(application);
        }

        public async Task<List<JobApplication>> GetByJobAsync(string jobId)
        {
            return await _collection
                .Find(x => x.JobId == jobId)
                .SortByDescending(x => x.AppliedAt)
                .ToListAsync();
        }

        public async Task UpdateAsync(
        string jobId,
        string jobSeekerId,
        string newStatus)
        {
            if (string.IsNullOrWhiteSpace(jobId))
                throw new ArgumentException("JobId is required");

            if (string.IsNullOrWhiteSpace(jobSeekerId))
                throw new ArgumentException("JobSeekerId is required");

            var filter = Builders<JobApplication>.Filter.And(
                Builders<JobApplication>.Filter.Eq(x => x.JobId, jobId),
                Builders<JobApplication>.Filter.Eq(x => x.JobSeekerId, jobSeekerId),
                Builders<JobApplication>.Filter.Ne(x => x.Status, newStatus) // idempotent
            );

            var update = Builders<JobApplication>.Update
                .Set(x => x.Status, newStatus)
                .Set(x => x.UpdatedAt, DateTime.UtcNow);

            var result = await _collection.UpdateOneAsync(filter, update);

            // If MatchedCount == 0 → either already withdrawn or not found
            if (result.MatchedCount == 0)
                return; // safe no-op
        }
    }
}
