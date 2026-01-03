using JobSeekerService.Application.DTOs;
using JobSeekerService.Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JobSeekerService.Controllers
{
    [ApiController]
    [Route("api/jobseeker/resume")]
    [Authorize(Policy = "RequireJobSeeker")]
    public class ResumeController : ControllerBase
    {
        private readonly GenerateResumeAiUseCase _useCase;

        public ResumeController(GenerateResumeAiUseCase useCase)
        {
            _useCase = useCase;
        }

        [HttpPost("ai-generate")]
        public async Task<IActionResult> Generate(ResumeAiRequest req)
            => Ok(await _useCase.ExecuteAsync(req));
    }
}
