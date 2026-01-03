using JobSeekerService.Application.DTOs;
using JobSeekerService.Application.Interfaces;

namespace JobSeekerService.Infrastructure.AI
{
    public class ResumeAiService : IResumeAiService
    {
        public Task<string> GenerateResumeAsync(ResumeAiRequest req)
        {
            var resume = $"""
        {req.FullName}
        Role: {req.TargetRole}
        Experience: {req.ExperienceYears} years
        Skills: {string.Join(", ", req.Skills)}
        Education: {req.Education}
        """;

            return Task.FromResult(resume);
        }
    }
}
