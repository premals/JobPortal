using AdminService.Domain.Entities;
using AdminService.Infrastructure.Mongo;
using MongoDB.Driver;

namespace AdminService.Infrastructure.Repositories
{
    public class AdminReadRepository
    {
        private readonly IMongoCollection<AdminJobSeekerProjection> _jobSeekers;
        private readonly IMongoCollection<AdminJobProviderProjection> _jobProviders;
        private readonly IMongoCollection<AdminJobProjection> _jobs;
        private readonly IMongoCollection<AdminApplicationProjection> _applications;

        public AdminReadRepository(MongoDbContext context)
        {
            _jobSeekers = context.JobSeekers;
            _jobProviders = context.JobProviders;
            _jobs = context.Jobs;
            _applications = context.Applications;
        }

        public Task UpsertJobSeekerAsync(AdminJobSeekerProjection projection)
        {
            var filter = Builders<AdminJobSeekerProjection>.Filter.Eq(x => x.UserId, projection.UserId);
            return _jobSeekers.ReplaceOneAsync(filter, projection, new ReplaceOptions { IsUpsert = true });
        }

        public async Task<bool> JobSeekerExistsAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return false;

            var filter = Builders<AdminJobSeekerProjection>.Filter.Eq(x => x.UserId, userId);
            return await _jobSeekers.Find(filter).AnyAsync();
        }

        public Task UpdateJobSeekerLastActiveAsync(string userId, DateTime lastActiveAt)
        {
            var filter = Builders<AdminJobSeekerProjection>.Filter.Eq(x => x.UserId, userId);
            var update = Builders<AdminJobSeekerProjection>.Update
                .Set(x => x.LastActiveAt, lastActiveAt)
                .Set(x => x.UpdatedAt, DateTime.UtcNow);
            return _jobSeekers.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = false });
        }

        public Task UpsertJobProviderAsync(AdminJobProviderProjection projection)
        {
            var filter = Builders<AdminJobProviderProjection>.Filter.Eq(x => x.JobProviderId, projection.JobProviderId);
            return _jobProviders.ReplaceOneAsync(filter, projection, new ReplaceOptions { IsUpsert = true });
        }

        public async Task<bool> JobProviderExistsAsync(string providerId)
        {
            if (string.IsNullOrWhiteSpace(providerId))
                return false;

            var filter = Builders<AdminJobProviderProjection>.Filter.Eq(x => x.JobProviderId, providerId);
            return await _jobProviders.Find(filter).AnyAsync();
        }

        public Task UpsertJobAsync(AdminJobProjection projection)
        {
            var filter = Builders<AdminJobProjection>.Filter.Eq(x => x.JobId, projection.JobId);
            return _jobs.ReplaceOneAsync(filter, projection, new ReplaceOptions { IsUpsert = true });
        }

        public Task ApplyJobPartialUpdateAsync(string jobId, UpdateDefinition<AdminJobProjection> update)
        {
            var filter = Builders<AdminJobProjection>.Filter.Eq(x => x.JobId, jobId);
            return _jobs.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = false });
        }

        public Task CloseJobAsync(string jobId, DateTime closedAt)
        {
            var filter = Builders<AdminJobProjection>.Filter.Eq(x => x.JobId, jobId);
            var update = Builders<AdminJobProjection>.Update
                .Set(x => x.Status, "Closed")
                .Set(x => x.ClosedAt, closedAt);
            return _jobs.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = false });
        }

        public Task UpsertApplicationAsync(AdminApplicationProjection projection)
        {
            var filter = Builders<AdminApplicationProjection>.Filter.Eq(x => x.ApplicationId, projection.ApplicationId);
            return _applications.ReplaceOneAsync(filter, projection, new ReplaceOptions { IsUpsert = true });
        }

        public Task UpdateApplicationStatusAsync(string applicationId, string status, DateTime updatedAt)
        {
            var filter = Builders<AdminApplicationProjection>.Filter.Eq(x => x.ApplicationId, applicationId);
            var update = Builders<AdminApplicationProjection>.Update
                .Set(x => x.Status, status)
                .Set(x => x.UpdatedAt, updatedAt);
            return _applications.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = false });
        }
    }
}
