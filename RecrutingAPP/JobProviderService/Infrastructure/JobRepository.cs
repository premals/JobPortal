using JobProviderService.Application;
using JobProviderService.Domain;
using JobProviderService.DTO;
using JobProviderService.Infrastructure.Messaging;
using MongoDB.Driver;
using static Shared.Contracts.Events.JobEvents;

namespace JobProviderService.Infrastructure
{
    public class JobRepository : IJobRepository
    {
        private readonly MongoDbContext _context;


        public JobRepository(MongoDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CreateAsync(Job job)
        {
            try
            {
                await _context.Jobs.InsertOneAsync(job);
                return true;
            }
            catch (MongoDB.Driver.MongoWriteException)
            {
                // duplicate key, validation, etc.
                return false;
            }
        }

        public async Task<List<Job>> GetByProviderAsync(string providerId)
        {
            return await _context.Jobs
                .Find(j => j.JobProviderId == providerId && j.Status == "Active")
                .ToListAsync();
        }

        public async Task<Job?> GetByIdAsync(string jobId)
        {
            if (string.IsNullOrWhiteSpace(jobId))
                return null;

            return await _context.Jobs
                .Find(j => j.Id == jobId)
                .FirstOrDefaultAsync();
        }

        // ------------------------------------
        // UPDATE JOB
        // ------------------------------------
        public async Task UpdateAsync(Job job)
        {
            ArgumentNullException.ThrowIfNull(job);

            var result = await _context.Jobs.ReplaceOneAsync(
                j => j.Id == job.Id,
                job);

            if (result.MatchedCount == 0)
                throw new InvalidOperationException("Job not found for update");
        }

        // ------------------------------------
        // UPDATE JOB STATUS
        // ------------------------------------
        public async Task UpdateStatusAsync(string jobId, string providerId, string status)
        {
            if (string.IsNullOrWhiteSpace(jobId))
                throw new ArgumentException("JobId is required");

            if (string.IsNullOrWhiteSpace(providerId))
                throw new ArgumentException("ProviderId is required");

            if (string.IsNullOrWhiteSpace(status))
                throw new ArgumentException("Status is required");

            var update = Builders<Job>.Update
                .Set(j => j.Status, status);

            var result = await _context.Jobs.UpdateOneAsync(
                j => j.Id == jobId && j.JobProviderId == providerId,
                update);

            if (result.MatchedCount == 0)
                throw new InvalidOperationException(
                    "Job not found or you are not authorized to update this job");
        }

        public async Task UpdatePartialAsync(
    string jobId,
    string providerId,
    UpdateJobPatchRequest request)
        {
            if (string.IsNullOrWhiteSpace(jobId))
                throw new ArgumentException("JobId is required");

            if (string.IsNullOrWhiteSpace(providerId))
                throw new ArgumentException("ProviderId is required");

            var updates = new List<UpdateDefinition<Job>>();
            var builder = Builders<Job>.Update;

            // ---------------------
            // Build dynamic updates
            // ---------------------
            if (request.Title != null)
                updates.Add(builder.Set(j => j.Title, request.Title));

            if (request.Description != null)
                updates.Add(builder.Set(j => j.Description, request.Description));

            if (request.EmploymentType != null)
                updates.Add(builder.Set(j => j.EmploymentType, request.EmploymentType));

            if (request.WorkMode != null)
                updates.Add(builder.Set(j => j.WorkMode, request.WorkMode));

            if (request.MinExperience.HasValue)
                updates.Add(builder.Set(j => j.MinExperience, request.MinExperience.Value));

            if (request.MaxExperience.HasValue)
                updates.Add(builder.Set(j => j.MaxExperience, request.MaxExperience.Value));

            if (request.City != null)
                updates.Add(builder.Set(j => j.City, request.City));

            if (request.State != null)
                updates.Add(builder.Set(j => j.State, request.State));

            if (request.Country != null)
                updates.Add(builder.Set(j => j.Country, request.Country));

            if (request.MinSalary.HasValue)
                updates.Add(builder.Set(j => j.MinSalary, request.MinSalary.Value));

            if (request.MaxSalary.HasValue)
                updates.Add(builder.Set(j => j.MaxSalary, request.MaxSalary.Value));

            if (request.Currency != null)
                updates.Add(builder.Set(j => j.Currency, request.Currency));

            if (request.SalaryFrequency != null)
                updates.Add(builder.Set(j => j.SalaryFrequency, request.SalaryFrequency));

            if (request.KeySkills != null)
                updates.Add(builder.Set(j => j.KeySkills, request.KeySkills));

            if (request.Education != null)
                updates.Add(builder.Set(j => j.Education, request.Education));

            if (request.Industry != null)
                updates.Add(builder.Set(j => j.Industry, request.Industry));

            if (request.Openings.HasValue)
                updates.Add(builder.Set(j => j.Openings, request.Openings.Value));

            if (request.ExpiryDate.HasValue)
                updates.Add(builder.Set(j => j.ExpiryDate, request.ExpiryDate));

            if (!updates.Any())
                throw new InvalidOperationException("No fields provided for update");

            var result = await _context.Jobs.UpdateOneAsync(
                j => j.Id == jobId && j.JobProviderId == providerId,
                builder.Combine(updates));

            if (result.MatchedCount == 0)
                throw new InvalidOperationException(
                    "Job not found or you are not authorized to update this job");
        }
    }
}
