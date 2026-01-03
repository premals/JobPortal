using JobSeekerService.Application.DTOs;
using JobSeekerService.Application.Interfaces;
using JobSeekerService.Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JobSeekerService.Controllers
{
    [ApiController]
    [Route("api/jobs")]
    [Authorize(Policy = "RequireJobSeeker")]
    public class JobBrowseController : ControllerBase
    {
        private readonly IJobReadRepository _repository;

        public JobBrowseController(IJobReadRepository repository)
        {
            _repository = repository;
        }

        // ======================================================
        // GET LATEST JOBS (PAGINATED)
        // GET /api/jobs?page=1&pageSize=20
        // ======================================================
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var jobs = await _repository.GetAllAsync(page, pageSize);
            return Ok(jobs);
        }

        // ======================================================
        // GET JOB DETAILS
        // GET /api/jobs/{jobId}
        // ======================================================
        [HttpGet("{jobId}")]
        public async Task<IActionResult> GetById(string jobId)
        {
            var job = await _repository.GetByIdAsync(jobId);
            if (job == null)
                return NotFound(new { message = "Job not found" });

            return Ok(job);
        }

        // ======================================================
        // SEARCH JOBS
        // GET /api/jobs/search
        // ======================================================
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] JobSearchRequest request)
        {
            var jobs = await _repository.SearchAsync(request);
            return Ok(jobs);
        }

        // ======================================================
        // JOBS BY SKILL
        // GET /api/jobs/by-skill?skill=.net
        // ======================================================
        [HttpGet("by-skill")]
        public async Task<IActionResult> BySkill([FromQuery] string skill)
        {
            if (string.IsNullOrWhiteSpace(skill))
                return BadRequest("Skill is required");

            return Ok(await _repository.GetBySkillAsync(skill));
        }

        // ======================================================
        // JOBS BY LOCATION
        // GET /api/jobs/by-location?city=Bangalore
        // ======================================================
        [HttpGet("by-location")]
        public async Task<IActionResult> ByLocation([FromQuery] string city)
        {
            if (string.IsNullOrWhiteSpace(city))
                return BadRequest("City is required");

            return Ok(await _repository.GetByLocationAsync(city));
        }

        // ======================================================
        // SIMILAR JOBS
        // GET /api/jobs/similar/{jobId}
        // ======================================================
        [HttpGet("similar/{jobId}")]
        public async Task<IActionResult> SimilarJobs(string jobId)
        {
            return Ok(await _repository.GetSimilarAsync(jobId));
        }

        // ======================================================
        // BASIC RECOMMENDATION
        // GET /api/jobs/recommended
        // ======================================================
        [HttpGet("recommended")]
        public async Task<IActionResult> Recommended()
        {
            // Simple logic: latest jobs (can later plug AI)
            return Ok(await _repository.GetAllAsync(1, 10));
        }
    }
}
