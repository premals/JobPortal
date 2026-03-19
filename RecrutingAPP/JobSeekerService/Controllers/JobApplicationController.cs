using JobSeekerService.Application.DTOs;
using JobSeekerService.Application.Interfaces;
using JobSeekerService.Application.UseCases;
using JobSeekerService.Domain.Constants;
using JobSeekerService.Domain.Entities;
using JobSeekerService.Infrastructure.Mongo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JobSeekerService.Controllers
{
    [ApiController]
    [Route("api/jobseeker/applications")]
    [Authorize(Policy = "RequireJobSeeker")]
    public class JobApplicationController : ControllerBase
    {
        private readonly IJobApplicationRepository _repository;
        private readonly IJobSeekerRepository _jobSeekerRepository;
        private readonly ApplyJobUseCase _applyJobUseCase;
        private readonly WithdrawJobApplicationUseCase _withdrawUseCase;

        public JobApplicationController(IJobApplicationRepository repository, 
            IJobSeekerRepository seekerRepository, 
            ApplyJobUseCase applyJobUseCase, 
            WithdrawJobApplicationUseCase withdrawJobApplicationUseCase)
        {
            _repository = repository;
            _jobSeekerRepository = seekerRepository;
            _applyJobUseCase = applyJobUseCase;
            _withdrawUseCase = withdrawJobApplicationUseCase;
        }

        // ======================================================
        // APPLY FOR A JOB
        // POST /api/jobseeker/applications/apply
        // ======================================================
        [HttpPost("apply")]
        public async Task<IActionResult> Apply([FromBody] ApplyJobRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var seekerId = ResolveUserId();
            if (string.IsNullOrEmpty(seekerId))
                return Unauthorized();

            var seeker = await _jobSeekerRepository
                .GetByUserIdAsync(seekerId);

            if (seeker == null)
            {
                // Create profile lazily so apply does not fail when profile sync is delayed.
                seeker = new JobSeekerProfile
                {
                    UserId = seekerId,
                    FullName = User.FindFirstValue("name")
                        ?? User.FindFirstValue(ClaimTypes.Name)
                        ?? string.Empty,
                    Email = User.FindFirstValue("email")
                        ?? User.FindFirstValue(ClaimTypes.Email)
                        ?? string.Empty
                };
                await _jobSeekerRepository.CreateAsync(seeker);
            }

            await _applyJobUseCase.ExecuteAsync(request, seeker);

            return Ok(new { message = "Job applied successfully." });
        }

        // ======================================================
        // GET MY APPLICATIONS
        // GET /api/jobseeker/applications
        // ======================================================
        [HttpGet]
        public async Task<IActionResult> MyApplications()
        {
            var seekerId = ResolveUserId();
            if (string.IsNullOrEmpty(seekerId))
                return Unauthorized();

            var applications = await _repository.GetBySeekerAsync(seekerId);
            return Ok(applications);
        }

        // ======================================================
        // GET APPLICATION STATUS FOR A JOB
        // GET /api/jobseeker/applications/{jobId}
        // ======================================================
        [HttpGet("{jobId}")]
        public async Task<IActionResult> GetStatus(string jobId)
        {
            var seekerId = ResolveUserId();
            if (string.IsNullOrEmpty(seekerId))
                return Unauthorized();

            var application = await _repository.GetAsync(jobId, seekerId);
            if (application == null)
                return NotFound(new { message = "Application not found." });

            return Ok(new
            {
                application.JobId,
                application.Status,
                application.AppliedAt
            });
        }

        // ======================================================
        // WITHDRAW APPLICATION
        // POST /api/jobseeker/applications/{jobId}/withdraw
        // ======================================================
        [HttpPost("{jobId}/withdraw")]
        public async Task<IActionResult> Withdraw(string jobId)
        {
            var seekerId = ResolveUserId();
            if (string.IsNullOrEmpty(seekerId))
                return Unauthorized();

            var application = await _repository.GetAsync(jobId, seekerId);
            if (application == null)
                return NotFound(new { message = "Application not found." });

            if (application.Status == ApplicationStatus.Withdrawn)
                return BadRequest(new { message = "Application already withdrawn." });

            application.Status = "Withdrawn";
            await _repository.UpdateAsync(application);
            await _withdrawUseCase.ExecuteAsync(jobId, seekerId, application.JobProviderId);

            return Ok(new { message = "Application withdrawn successfully." });
        }

        // ======================================================
        // UPDATE APPLICATION STATUS (INTERNAL / FUTURE USE)
        // PATCH /api/jobseeker/applications/{jobId}/status
        // ======================================================
        [HttpPatch("{jobId}/status")]
        public async Task<IActionResult> UpdateStatus(
            string jobId,
            [FromQuery] string status)
        {
            var seekerId = ResolveUserId();
            if (string.IsNullOrEmpty(seekerId))
                return Unauthorized();

            var application = await _repository.GetAsync(jobId, seekerId);
            if (application == null)
                return NotFound(new { message = "Application not found." });

            application.Status = status;
            await _repository.UpdateAsync(application);

            return Ok(new { message = $"Application status updated to {status}" });
        }

        private string? ResolveUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("userId")
                ?? User.FindFirstValue("sub");
        }
    }
}
