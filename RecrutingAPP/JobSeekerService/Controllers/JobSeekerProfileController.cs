using JobSeekerService.Application.DTOs;
using JobSeekerService.Application.Interfaces;
using JobSeekerService.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobSeekerService.Controllers
{
    [ApiController]
    [Route("api/jobseeker/profile")]
    [Authorize(Policy = "RequireJobSeeker")]
    public class JobSeekerProfileController : ControllerBase
    {
        private readonly IJobSeekerRepository _repository;

        public JobSeekerProfileController(IJobSeekerRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var userId = User.FindFirst("userId")?.Value;
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var profile = await _repository.GetByUserIdAsync(userId);
            if (profile == null)
            {
                profile = new JobSeekerProfile
                {
                    UserId = userId,
                    FullName = string.Empty,
                    Email = User.FindFirst("email")?.Value ?? string.Empty
                };
                await _repository.CreateAsync(profile);
            }

            return Ok(profile);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] JobSeekerProfileRequest request)
        {
            var userId = User.FindFirst("userId")?.Value;
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var profile = await _repository.GetByUserIdAsync(userId)
                ?? new JobSeekerProfile
                {
                    UserId = userId
                };

            profile.FullName = request.FullName;
            profile.Email = request.Email;
            profile.Phone = request.Phone;
            profile.Headline = request.Headline;
            profile.Summary = request.Summary;
            profile.Skills = request.Skills;
            profile.ExperienceYears = request.ExperienceYears;
            profile.Education = request.Education;
            profile.Location = request.Location;
            profile.WorkHistory = request.WorkHistory;
            profile.EducationHistory = request.EducationHistory;
            profile.Projects = request.Projects;
            profile.Certifications = request.Certifications;
            profile.Languages = request.Languages;
            profile.ResumeSettings = request.ResumeSettings;
            profile.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(profile);
            return Ok(profile);
        }
    }
}
