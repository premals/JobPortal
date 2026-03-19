using JobProviderService.Application.Interfaces;
using JobProviderService.Domain;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace JobProviderService.Infrastructure.Repository
{
    public class JobProviderSettingsRepository : IJobProviderSettingsRepository
    {
        private readonly IMongoCollection<JobProviderSettings> _collection;
        private readonly string? _defaultAiProvider;

        public JobProviderSettingsRepository(MongoDbContext context, IConfiguration configuration)
        {
            _collection = context.JobProviderSettings;
            _defaultAiProvider = configuration["AI:Provider"]?.Trim();
        }

        public async Task<JobProviderSettings> GetOrCreateAsync(string providerId)
        {
            if (string.IsNullOrWhiteSpace(providerId))
                throw new ArgumentException("ProviderId is required");

            var existing = await _collection
                .Find(x => x.JobProviderId == providerId)
                .FirstOrDefaultAsync();

            if (existing != null)
                return existing;

            var settings = new JobProviderSettings
            {
                JobProviderId = providerId
            };
            if (!string.IsNullOrWhiteSpace(_defaultAiProvider))
                settings.Ai.Provider = _defaultAiProvider;

            await _collection.InsertOneAsync(settings);
            return settings;
        }

        public async Task<JobProviderSettings> UpdateAsync(JobProviderSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);
            settings.UpdatedAt = DateTime.UtcNow;

            await _collection.ReplaceOneAsync(
                x => x.JobProviderId == settings.JobProviderId,
                settings,
                new ReplaceOptions { IsUpsert = true });

            return settings;
        }
    }
}
