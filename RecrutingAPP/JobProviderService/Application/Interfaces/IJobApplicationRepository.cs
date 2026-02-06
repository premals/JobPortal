using JobProviderService.Domain;
using static System.Net.Mime.MediaTypeNames;

namespace JobProviderService.Application.Interfaces
{
    public interface IJobApplicationRepository
    {
        Task<JobApplication?> GetAsync(string jobId, string jobSeekerId);
        Task ApplyAsync(JobApplication application);
        Task<List<JobApplication>> GetByJobAsync(string jobId);
        Task<List<JobApplication>> GetByProviderAsync(string providerId, int limit);
        Task<long> CountByProviderAsync(string providerId);
        Task<long> CountByProviderAndStatusAsync(string providerId, string status);
        Task UpdateAsync(string jobId,
        string jobSeekerId,
        string newStatus);
    }
}
