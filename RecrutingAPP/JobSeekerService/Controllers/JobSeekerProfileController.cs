using JobSeekerService.Application.DTOs;
using JobSeekerService.Application.Interfaces;
using JobSeekerService.Domain.Entities;
using JobSeekerService.Infrastructure.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static Shared.Contracts.Events.JobEvents;

namespace JobSeekerService.Controllers
{
    [ApiController]
    [Route("api/jobseeker/profile")]
    [Authorize(Policy = "RequireJobSeeker")]
    public class JobSeekerProfileController : ControllerBase
    {
        private readonly IJobSeekerRepository _repository;
        private readonly IEventBus _eventBus;

        public JobSeekerProfileController(IJobSeekerRepository repository, IEventBus eventBus)
        {
            _repository = repository;
            _eventBus = eventBus;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var userId = ResolveUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var profile = await _repository.GetByUserIdAsync(userId);
            if (profile == null)
            {
                profile = new JobSeekerProfile
                {
                    UserId = userId,
                    FullName = string.Empty,
                    Email = User.FindFirst("email")?.Value
                        ?? User.FindFirstValue(ClaimTypes.Email)
                        ?? string.Empty
                };
                await _repository.CreateAsync(profile);
            }

            return Ok(profile);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] JobSeekerProfileRequest request)
        {
            var userId = ResolveUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var profile = await _repository.GetByUserIdAsync(userId)
                ?? new JobSeekerProfile
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };

            profile.FullName = request.FullName;
            profile.Email = request.Email;
            profile.Phone = request.Phone;
            profile.Gender = request.Gender;
            profile.Headline = request.Headline;
            profile.Summary = request.Summary;
            profile.Skills = request.Skills ?? new();
            profile.ExperienceYears = request.ExperienceYears;
            profile.Education = request.Education;
            profile.Location = request.Location;
            profile.WorkHistory = request.WorkHistory ?? new();
            profile.EducationHistory = request.EducationHistory ?? new();
            profile.Projects = request.Projects ?? new();
            profile.Certifications = request.Certifications ?? new();
            profile.Languages = request.Languages ?? new();
            profile.ResumeSettings = request.ResumeSettings ?? new();
            profile.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(profile);

            await _eventBus.PublishAsync(new JobSeekerProfileUpsertedEvent
            {
                UserId = profile.UserId,
                FullName = profile.FullName,
                Email = profile.Email,
                Phone = profile.Phone,
                Headline = profile.Headline,
                Summary = profile.Summary,
                Skills = profile.Skills,
                ExperienceYears = profile.ExperienceYears,
                Education = profile.Education,
                Location = profile.Location,
                WorkHistory = profile.WorkHistory.Select(item => new JobSeekerWorkHistoryItem
                {
                    Company = item.Company,
                    Role = item.Role,
                    StartDate = item.StartDate,
                    EndDate = item.EndDate,
                    Description = item.Description,
                    Skills = item.Skills
                }).ToList(),
                EducationHistory = profile.EducationHistory.Select(item => new JobSeekerEducationRecordItem
                {
                    School = item.School,
                    Degree = item.Degree,
                    Field = item.Field,
                    GraduationYear = item.GraduationYear
                }).ToList(),
                Projects = profile.Projects.Select(item => new JobSeekerProjectRecordItem
                {
                    Name = item.Name,
                    Role = item.Role,
                    Description = item.Description,
                    Link = item.Link
                }).ToList(),
                Certifications = profile.Certifications.Select(item => new JobSeekerCertificationRecordItem
                {
                    Name = item.Name,
                    Issuer = item.Issuer,
                    Year = item.Year
                }).ToList(),
                Languages = profile.Languages.Select(item => new JobSeekerLanguageRecordItem
                {
                    Name = item.Name,
                    Proficiency = item.Proficiency
                }).ToList(),
                ResumeSettings = new JobSeekerResumeSettings
                {
                    AtsFriendly = profile.ResumeSettings.AtsFriendly,
                    Template = profile.ResumeSettings.Template
                },
                CreatedAt = profile.CreatedAt,
                UpdatedAt = profile.UpdatedAt
            });

            return Ok(profile);
        }

        private string? ResolveUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("userId")
                ?? User.FindFirstValue("sub");
        }
    }
}
