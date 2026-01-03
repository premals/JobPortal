using JobSeekerService.Application.DTOs;

namespace JobSeekerService.Application.Interfaces
{
    public interface IResumeAiService
    {
        Task<string> GenerateResumeAsync(ResumeAiRequest request);
    }
}
