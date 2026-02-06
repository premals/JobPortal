using JobProviderService.Domain;

namespace JobProviderService.Application.Interfaces
{
    public interface IJobProviderSettingsRepository
    {
        Task<JobProviderSettings> GetOrCreateAsync(string providerId);
        Task<JobProviderSettings> UpdateAsync(JobProviderSettings settings);
    }
}
