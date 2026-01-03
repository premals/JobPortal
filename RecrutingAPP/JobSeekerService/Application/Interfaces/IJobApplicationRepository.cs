using JobSeekerService.Domain.Entities;

namespace JobSeekerService.Application.Interfaces
{
    public interface IJobApplicationRepository
    {
        Task<JobApplication?> GetAsync(string jobId, string seekerId);
        Task<List<JobApplication>> GetBySeekerAsync(string seekerId);
        Task ApplyAsync(JobApplication application);
        Task UpdateAsync(JobApplication application);
    }
}
