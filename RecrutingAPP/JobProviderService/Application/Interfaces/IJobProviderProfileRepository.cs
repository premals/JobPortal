using JobProviderService.Domain;

namespace JobProviderService.Application.Interfaces
{
    public interface IJobProviderProfileRepository
    {
        Task<JobProviderProfile?> GetByProviderIdAsync(string providerId);
        Task<JobProviderProfile> UpsertAsync(JobProviderProfile profile);
    }
}
