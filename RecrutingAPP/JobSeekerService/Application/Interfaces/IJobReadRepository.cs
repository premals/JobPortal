using JobSeekerService.Application.DTOs;
using JobSeekerService.Domain.Entities;
using static Shared.Contracts.Events.JobEvents;

namespace JobSeekerService.Application.Interfaces
{
    public interface IJobReadRepository
    {
        Task<List<JobSnapshot>> GetAllAsync(int page, int pageSize);
        Task<JobSnapshot?> GetByIdAsync(string jobId);
        Task<List<JobSnapshot>> SearchAsync(JobSearchRequest request);
        Task<List<JobSnapshot>> GetBySkillAsync(string skill);
        Task<List<JobSnapshot>> GetByLocationAsync(string city);
        Task<List<JobSnapshot>> GetSimilarAsync(string jobId);
        Task UpsertFromEventAsync(JobCreatedEvent @event);
        Task MarkClosedAsync(string jobId);
        Task ApplyPartialUpdateAsync(JobUpdatedEvent e);
        Task ReplaceFromEventAsync(JobFullyUpdatedEvent e);
    }
}
