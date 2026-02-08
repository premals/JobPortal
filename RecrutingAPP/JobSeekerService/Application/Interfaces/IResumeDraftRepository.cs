using JobSeekerService.Domain.Entities;

namespace JobSeekerService.Application.Interfaces
{
    public interface IResumeDraftRepository
    {
        Task<ResumeDraft?> GetByUserIdAsync(string userId);
        Task UpsertAsync(ResumeDraft draft);
    }
}
