using Azure.Core;
using JobSeekerService.Application.Interfaces;
using JobSeekerService.Domain.Entities;
using JobSeekerService.Infrastructure.Messaging;
using MongoDB.Driver;
using static Shared.Contracts.Events.JobEvents;

namespace JobSeekerService.Infrastructure.Mongo
{
    public class JobApplicationRepository : IJobApplicationRepository
    {
        private readonly IMongoCollection<JobApplication> _collection;

        public JobApplicationRepository(MongoDbContext context)
        {
            _collection = context.JobApplications;
        }

        // ------------------------------------
        // GET APPLICATION (JOB + SEEKER)
        // ------------------------------------
        public async Task<JobApplication?> GetAsync(string jobId, string seekerId)
        {
            if (string.IsNullOrWhiteSpace(jobId) || string.IsNullOrWhiteSpace(seekerId))
                return null;

            return await _collection
                .Find(a => a.JobId == jobId && a.JobSeekerId == seekerId)
                .FirstOrDefaultAsync();
        }

        // ------------------------------------
        // GET ALL APPLICATIONS BY SEEKER
        // ------------------------------------
        public async Task<List<JobApplication>> GetBySeekerAsync(string seekerId)
        {
            if (string.IsNullOrWhiteSpace(seekerId))
                return new List<JobApplication>();

            return await _collection
                .Find(a => a.JobSeekerId == seekerId)
                .SortByDescending(a => a.AppliedAt)
                .ToListAsync();
        }

        // ------------------------------------
        // APPLY FOR JOB
        // ------------------------------------
        public async Task ApplyAsync(JobApplication application)
        {
            ArgumentNullException.ThrowIfNull(application);

            application.AppliedAt = DateTime.UtcNow;
            application.UpdatedAt = DateTime.UtcNow;

            await _collection.InsertOneAsync(application);
        }

        // ------------------------------------
        // UPDATE APPLICATION STATUS
        // ------------------------------------
        public async Task UpdateAsync(JobApplication application)
        {
            ArgumentNullException.ThrowIfNull(application);

            application.UpdatedAt = DateTime.UtcNow;

            var result = await _collection.ReplaceOneAsync(
                a => a.Id == application.Id,
                application);

            if (result.MatchedCount == 0)
                throw new InvalidOperationException("Job application not found");
        }
    }
}
