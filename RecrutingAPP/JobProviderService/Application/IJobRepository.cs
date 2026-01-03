using JobProviderService.Domain;
using JobProviderService.DTO;

namespace JobProviderService.Application
{
    public interface IJobRepository
    {
        Task<bool> CreateAsync(Job job);
        Task<List<Job>> GetByProviderAsync(string providerId);

        Task<Job?> GetByIdAsync(string jobId);
        Task UpdateAsync(Job job);
        Task UpdateStatusAsync(string jobId, string providerId, string status);
        Task UpdatePartialAsync(
    string jobId,
    string providerId,
    UpdateJobPatchRequest request);
    }
}
