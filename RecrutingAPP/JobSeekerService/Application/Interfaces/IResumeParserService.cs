using JobSeekerService.Application.DTOs;

namespace JobSeekerService.Application.Interfaces
{
    public interface IResumeParserService
    {
        Task<ResumeParseResult> ParseAsync(ResumeParseRequest request);
        Task<ResumeParseResult> ParseFileAsync(ResumeFileParseRequest request);
    }
}
