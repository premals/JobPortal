using JobProviderService.Application.Interfaces;
using JobProviderService.Domain;
using JobProviderService.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Linq;

namespace JobProviderService.Controllers
{
    [ApiController]
    [Route("api/jobs/settings")]
    [Authorize(Policy = "RequireJobProvider")]
    public class SettingsController : ControllerBase
    {
        private readonly IJobProviderSettingsRepository _repository;

        public SettingsController(IJobProviderSettingsRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var providerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(providerId))
                return Unauthorized();

            var settings = await _repository.GetOrCreateAsync(providerId);
            return Ok(ToDto(settings));
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] JobProviderSettingsDto dto)
        {
            var providerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(providerId))
                return Unauthorized();

            var settings = await _repository.GetOrCreateAsync(providerId);

            settings.Interview.DifficultyLevels = dto.Interview.DifficultyLevels
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList();

            if (settings.Interview.DifficultyLevels.Count == 0)
                settings.Interview.DifficultyLevels = new List<string> { "Easy", "Medium", "High" };

            settings.Interview.DefaultDifficulty = dto.Interview.DefaultDifficulty;
            if (!settings.Interview.DifficultyLevels.Contains(settings.Interview.DefaultDifficulty))
                settings.Interview.DefaultDifficulty = settings.Interview.DifficultyLevels.First();

            settings.Interview.QuestionsCount = Math.Max(1, dto.Interview.QuestionsCount);
            settings.Interview.SlotDurationMinutes = Math.Max(15, dto.Interview.SlotDurationMinutes);
            settings.Interview.SlotCount = Math.Max(1, dto.Interview.SlotCount);

            settings.Ai.EnableShortlistSuggestions = dto.Ai.EnableShortlistSuggestions;
            settings.Ai.EnableInterviewAi = dto.Ai.EnableInterviewAi;
            settings.Ai.Provider = string.IsNullOrWhiteSpace(dto.Ai.Provider)
                ? "OpenAI"
                : dto.Ai.Provider.Trim();
            settings.Ai.Endpoint = dto.Ai.Endpoint;
            settings.Ai.Deployment = dto.Ai.Deployment;
            settings.Ai.ApiVersion = dto.Ai.ApiVersion;
            settings.Ai.EnableAvatar = dto.Ai.EnableAvatar;
            settings.Ai.AvatarProvider = dto.Ai.AvatarProvider;

            settings.Email.InviteSubject = dto.Email.InviteSubject;
            settings.Email.InviteBody = dto.Email.InviteBody;

            var saved = await _repository.UpdateAsync(settings);
            return Ok(ToDto(saved));
        }

        private static JobProviderSettingsDto ToDto(JobProviderSettings settings)
        {
            return new JobProviderSettingsDto
            {
                Interview = new InterviewSettingsDto
                {
                    DifficultyLevels = settings.Interview.DifficultyLevels,
                    DefaultDifficulty = settings.Interview.DefaultDifficulty,
                    QuestionsCount = settings.Interview.QuestionsCount,
                    SlotDurationMinutes = settings.Interview.SlotDurationMinutes,
                    SlotCount = settings.Interview.SlotCount
                },
                Ai = new AiSettingsDto
                {
                    EnableShortlistSuggestions = settings.Ai.EnableShortlistSuggestions,
                    EnableInterviewAi = settings.Ai.EnableInterviewAi,
                    Provider = string.IsNullOrWhiteSpace(settings.Ai.Provider)
                        ? "OpenAI"
                        : settings.Ai.Provider,
                    Endpoint = settings.Ai.Endpoint,
                    Deployment = settings.Ai.Deployment,
                    ApiVersion = settings.Ai.ApiVersion,
                    EnableAvatar = settings.Ai.EnableAvatar,
                    AvatarProvider = settings.Ai.AvatarProvider
                },
                Email = new EmailTemplateSettingsDto
                {
                    InviteSubject = settings.Email.InviteSubject,
                    InviteBody = settings.Email.InviteBody
                }
            };
        }
    }
}
