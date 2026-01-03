using JobSeekerService.Application.DTOs;
using JobSeekerService.Application.Interfaces;

namespace JobSeekerService.Application.UseCases
{
    public class GenerateResumeAiUseCase
    {
        private readonly IResumeAiService _ai;

        public GenerateResumeAiUseCase(IResumeAiService ai)
        {
            _ai = ai;
        }

        public Task<string> ExecuteAsync(ResumeAiRequest request)
            => _ai.GenerateResumeAsync(request);
    }
}
