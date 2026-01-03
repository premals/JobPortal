using JobProviderService.Application;
using JobProviderService.Application.Interfaces;
using JobProviderService.Application.UseCases;
using JobProviderService.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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

        public JobsController(CreateJobUseCase createJob, IJobRepository repository, UpdateJobUseCase updateJob,
        DeleteJobUseCase deleteJob, UpdateJobPartialUseCase updateJobPartial, IJobApplicationRepository applicationRepository)
        {
            _createJob = createJob;
            _repository = repository;
            _updateJob = updateJob;
            _deleteJob = deleteJob;
            _updateJobPartial = updateJobPartial;
            _applicationRepository = applicationRepository;
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
