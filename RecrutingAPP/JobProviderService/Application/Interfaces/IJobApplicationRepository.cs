using JobProviderService.Domain;
using static System.Net.Mime.MediaTypeNames;

namespace JobProviderService.Application.Interfaces
{
    public interface IJobApplicationRepository
    {
        Task<JobApplication?> GetAsync(string jobId, string jobSeekerId);
        Task ApplyAsync(JobApplication application);
        Task<List<JobApplication>> GetByJobAsync(string jobId);
        Task UpdateAsync(string jobId,
        string jobSeekerId,
        string newStatus);
    }
}
