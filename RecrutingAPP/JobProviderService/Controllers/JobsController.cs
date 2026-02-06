using JobProviderService.Application;
using JobProviderService.Application.Interfaces;
using JobProviderService.Application.UseCases;
using JobProviderService.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Linq;

namespace JobProviderService.Controllers
{
    [Route("api/jobs")]
    [Authorize(Policy = "RequireJobProvider")]
    [ApiController]
    public class JobsController : ControllerBase
    {
        private readonly CreateJobUseCase _createJob;
        private readonly IJobRepository _repository;
        private readonly IJobApplicationRepository _applicationRepository;
        private readonly UpdateJobUseCase _updateJob;
        private readonly DeleteJobUseCase _deleteJob;
        private readonly UpdateJobPartialUseCase _updateJobPartial;
        private readonly UpdateApplicationStatusUseCase _updateApplicationStatus;
        private readonly CreateInterviewInviteUseCase _createInterviewInvite;
        private readonly GetAiShortlistSuggestionUseCase _aiShortlist;

        public JobsController(CreateJobUseCase createJob, IJobRepository repository, UpdateJobUseCase updateJob,
        DeleteJobUseCase deleteJob, UpdateJobPartialUseCase updateJobPartial, IJobApplicationRepository applicationRepository,
        UpdateApplicationStatusUseCase updateApplicationStatus,
        CreateInterviewInviteUseCase createInterviewInvite,
        GetAiShortlistSuggestionUseCase aiShortlist)
        {
            _createJob = createJob;
            _repository = repository;
            _updateJob = updateJob;
            _deleteJob = deleteJob;
            _updateJobPartial = updateJobPartial;
            _applicationRepository = applicationRepository;
            _updateApplicationStatus = updateApplicationStatus;
            _createInterviewInvite = createInterviewInvite;
            _aiShortlist = aiShortlist;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateJobRequest request)
        {
            var providerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(providerId))
                return Unauthorized();

            await _createJob.ExecuteAsync(providerId, request);
            return Ok(new { message = "Job created successfully" });
        }

        [HttpGet("my")]
        public async Task<IActionResult> MyJobs()
        {
            var providerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(providerId))
                return Unauthorized();

            var jobs = await _repository.GetByProviderAsync(providerId);
            return Ok(jobs);
        }

        // ----------------------
        // GET JOB BY ID
        // ----------------------
        [HttpGet("{jobId}")]
        public async Task<IActionResult> GetById(string jobId)
        {
            var providerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var job = await _repository.GetByIdAsync(jobId);
            if (job == null || job.JobProviderId != providerId)
                return NotFound();

            return Ok(job);
        }

        [HttpGet("applications/{jobId}")]
        public async Task<IActionResult> GetApplications(string jobId)
        {
            return Ok(await _applicationRepository.GetByJobAsync(jobId));
        }

        [HttpPost("{jobId}/applications/{jobSeekerId}/ai-suggest")]
        public async Task<IActionResult> GetAiSuggestion(
            string jobId,
            string jobSeekerId)
        {
            var providerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(providerId))
                return Unauthorized();

            var suggestion = await _aiShortlist.ExecuteAsync(jobId, jobSeekerId, providerId);
            return Ok(suggestion);
        }

        [HttpPost("{jobId}/applications/{jobSeekerId}/invite")]
        public async Task<IActionResult> CreateInvite(
            string jobId,
            string jobSeekerId,
            [FromBody] InterviewInviteRequest request)
        {
            var providerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(providerId))
                return Unauthorized();

            var invite = await _createInterviewInvite.ExecuteAsync(jobId, jobSeekerId, providerId, request);
            return Ok(invite);
        }

        [HttpGet("applications/summary")]
        public async Task<IActionResult> GetApplicationsSummary([FromQuery] int limit = 6)
        {
            var providerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(providerId))
                return Unauthorized();

            var total = await _applicationRepository.CountByProviderAsync(providerId);
            var applied = await _applicationRepository.CountByProviderAndStatusAsync(providerId, "Applied");
            var shortlisted = await _applicationRepository.CountByProviderAndStatusAsync(providerId, "Shortlisted");
            var rejected = await _applicationRepository.CountByProviderAndStatusAsync(providerId, "Rejected");
            var hired = await _applicationRepository.CountByProviderAndStatusAsync(providerId, "Hired");

            var recentApps = await _applicationRepository.GetByProviderAsync(providerId, limit);
            var jobs = await _repository.GetByProviderAsync(providerId);
            var jobTitleMap = jobs.ToDictionary(j => j.Id, j => j.Title);

            var recent = recentApps.Select(app => new JobApplicationSummaryItem
            {
                JobId = app.JobId,
                JobTitle = jobTitleMap.TryGetValue(app.JobId, out var title) ? title : "Job",
                JobSeekerId = app.JobSeekerId,
                FullName = app.FullName,
                Email = app.Email,
                Phone = app.Phone,
                ResumeUrl = app.ResumeUrl,
                Status = app.Status,
                AppliedAt = app.AppliedAt
            }).ToList();

            var response = new JobApplicationSummaryResponse
            {
                TotalApplicants = total,
                AppliedCount = applied,
                ShortlistedCount = shortlisted,
                RejectedCount = rejected,
                HiredCount = hired,
                Recent = recent
            };

            return Ok(response);
        }

        [HttpPatch("{jobId}/applications/{jobSeekerId}/status")]
        public async Task<IActionResult> UpdateApplicationStatus(
            string jobId,
            string jobSeekerId,
            [FromQuery] string status)
        {
            var providerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(providerId))
                return Unauthorized();

            await _updateApplicationStatus.ExecuteAsync(jobId, jobSeekerId, providerId, status);
            return Ok(new { message = $"Application status updated to {status}" });
        }

        // ----------------------
        // UPDATE JOB
        // ----------------------
        [HttpPut("{jobId}")]
        public async Task<IActionResult> Update(string jobId, UpdateJobRequest request)
        {
            var providerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await _updateJob.ExecuteAsync(jobId, providerId, request);
            return Ok(new { message = "Job updated successfully" });
        }

        // ----------------------
        // DELETE (CLOSE) JOB
        // ----------------------
        [HttpDelete("{jobId}")]
        public async Task<IActionResult> Delete(string jobId)
        {
            var providerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await _deleteJob.ExecuteAsync(jobId, providerId);
            return Ok(new { message = "Job closed successfully" });
        }

        // ----------------------
        // CHANGE JOB STATUS
        // ----------------------
        [HttpPatch("{jobId}/status")]
        public async Task<IActionResult> ChangeStatus(
            string jobId,
            [FromQuery] string status)
        {
            var providerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await _repository.UpdateStatusAsync(jobId, providerId, status);
            return Ok(new { message = $"Job status updated to {status}" });
        }


        [HttpPatch("{jobId}")]
        public async Task<IActionResult> PatchUpdate(
    string jobId,
    UpdateJobPatchRequest request)
        {
            var providerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(providerId))
                return Unauthorized();

            await _updateJobPartial.ExecuteAsync(jobId, providerId, request);
            return Ok(new { message = "Job updated successfully" });
        }
    }
}
