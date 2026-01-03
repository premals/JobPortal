using JobProviderService.Domain;
using MongoDB.Driver;

namespace JobProviderService.Infrastructure.Indexes
{
    public class JobApplicationIndexSeeder
    {
        private readonly IMongoCollection<JobApplication> _collection;

        public JobApplicationIndexSeeder(IMongoDatabase database)
        {
            _collection = database.GetCollection<JobApplication>("JobApplications");
        }

        public async Task CreateIndexesAsync()
        {
            var indexes = new List<CreateIndexModel<JobApplication>>
        {
            // Prevent duplicate apply
            new CreateIndexModel<JobApplication>(
                Builders<JobApplication>.IndexKeys
                    .Ascending(x => x.JobId)
                    .Ascending(x => x.JobSeekerId),
                new CreateIndexOptions { Unique = true }),

            // Provider view
            new CreateIndexModel<JobApplication>(
                Builders<JobApplication>.IndexKeys
                    .Ascending(x => x.JobProviderId)
                    .Ascending(x => x.JobId)),

            // Candidate history
            new CreateIndexModel<JobApplication>(
                Builders<JobApplication>.IndexKeys
                    .Ascending(x => x.JobSeekerId)
                    .Descending(x => x.AppliedAt)),

            // Status filtering
            new CreateIndexModel<JobApplication>(
                Builders<JobApplication>.IndexKeys
                    .Ascending(x => x.Status))
        };

            await _collection.Indexes.CreateManyAsync(indexes);
        }
    }
}
