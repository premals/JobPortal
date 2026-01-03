using JobSeekerService.Domain.Entities;

namespace JobSeekerService.Application.Interfaces
{
    public interface IJobSeekerRepository
    {
        Task<JobSeekerProfile?> GetByUserIdAsync(string userId);
        Task CreateAsync(JobSeekerProfile profile);
        Task UpdateAsync(JobSeekerProfile profile);
    }
}
