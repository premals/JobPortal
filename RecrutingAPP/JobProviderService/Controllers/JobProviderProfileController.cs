using JobProviderService.Application.Interfaces;
using JobProviderService.Domain;
using JobProviderService.DTO;
using JobProviderService.Infrastructure.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static Shared.Contracts.Events.JobEvents;

namespace JobProviderService.Controllers
{
    [ApiController]
    [Route("api/jobs/provider-profile")]
    [Authorize(Policy = "RequireJobProvider")]
    public class JobProviderProfileController : ControllerBase
    {
        private readonly IJobProviderProfileRepository _repository;
        private readonly IEventBus _eventBus;

        public JobProviderProfileController(IJobProviderProfileRepository repository, IEventBus eventBus)
        {
            _repository = repository;
            _eventBus = eventBus;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var providerId = User.FindFirstValue("userId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(providerId))
                return Unauthorized();

            var profile = await _repository.GetByProviderIdAsync(providerId);
            if (profile == null)
            {
                profile = new JobProviderProfile
                {
                    JobProviderId = providerId
                };
                await _repository.UpsertAsync(profile);
            }

            return Ok(ToDto(profile));
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] JobProviderProfileDto dto)
        {
            var providerId = User.FindFirstValue("userId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(providerId))
                return Unauthorized();

            var profile = await _repository.GetByProviderIdAsync(providerId)
                ?? new JobProviderProfile
                {
                    JobProviderId = providerId
                };

            profile.CompanyName = dto.CompanyName ?? string.Empty;
            profile.BrandName = dto.BrandName ?? string.Empty;
            profile.Industry = dto.Industry ?? string.Empty;
            profile.CompanySize = dto.CompanySize ?? string.Empty;
            profile.Website = dto.Website ?? string.Empty;
            profile.Phone = dto.Phone ?? string.Empty;
            profile.Location = dto.Location ?? string.Empty;
            profile.About = dto.About ?? string.Empty;
            profile.LogoUrl = dto.LogoUrl;
            profile.LinkedInUrl = dto.LinkedInUrl;
            profile.TwitterUrl = dto.TwitterUrl;
            profile.UpdatedAt = DateTime.UtcNow;

            var saved = await _repository.UpsertAsync(profile);

            await _eventBus.PublishAsync(new JobProviderProfileUpsertedEvent
            {
                JobProviderId = saved.JobProviderId,
                CompanyName = saved.CompanyName,
                BrandName = saved.BrandName,
                Industry = saved.Industry,
                CompanySize = saved.CompanySize,
                Website = saved.Website,
                Phone = saved.Phone,
                Location = saved.Location,
                About = saved.About,
                LogoUrl = saved.LogoUrl,
                LinkedInUrl = saved.LinkedInUrl,
                TwitterUrl = saved.TwitterUrl,
                ContactName = User.FindFirstValue(ClaimTypes.Name) ?? string.Empty,
                ContactEmail = User.FindFirstValue("email") ?? string.Empty,
                CreatedAt = saved.UpdatedAt,
                UpdatedAt = saved.UpdatedAt
            });

            return Ok(ToDto(saved));
        }

        private static JobProviderProfileDto ToDto(JobProviderProfile profile)
        {
            return new JobProviderProfileDto
            {
                CompanyName = profile.CompanyName,
                BrandName = profile.BrandName,
                Industry = profile.Industry,
                CompanySize = profile.CompanySize,
                Website = profile.Website,
                Phone = profile.Phone,
                Location = profile.Location,
                About = profile.About,
                LogoUrl = profile.LogoUrl,
                LinkedInUrl = profile.LinkedInUrl,
                TwitterUrl = profile.TwitterUrl
            };
        }
    }
}
