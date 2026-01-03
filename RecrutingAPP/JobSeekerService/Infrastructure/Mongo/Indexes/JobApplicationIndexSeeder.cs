using JobSeekerService.Domain.Entities;
using MongoDB.Driver;

namespace JobSeekerService.Infrastructure.Mongo.Indexes
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
            var indexModels = new List<CreateIndexModel<JobApplication>>
        {
            // Prevent duplicate applications for same job by same seeker
            new(
                Builders<JobApplication>.IndexKeys
                    .Ascending(a => a.JobId)
                    .Ascending(a => a.JobSeekerId),
                new CreateIndexOptions
                {
                    Unique = true,
                    Name = "UX_JobId_JobSeekerId"
                }
            ),

            // Fast lookup by JobSeeker
            new(
                Builders<JobApplication>.IndexKeys
                    .Ascending(a => a.JobSeekerId)
                    .Descending(a => a.AppliedAt),
                new CreateIndexOptions
                {
                    Name = "IX_JobSeeker_AppliedAt"
                }
            )
        };

            await _collection.Indexes.CreateManyAsync(indexModels);
        }
    }
}
