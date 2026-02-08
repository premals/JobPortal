using JobSeekerService.Application.DTOs;
using JobSeekerService.Application.UseCases;
using JobSeekerService.Application.Interfaces;
using JobSeekerService.Domain.Entities;
using System.Security.Claims;
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
        private readonly ParseResumeUseCase _parseUseCase;
        private readonly ParseResumeFileUseCase _parseFileUseCase;
        private readonly IResumeDraftRepository _draftRepository;
        private readonly IResumePdfGenerator _pdfGenerator;
        private readonly IJobSeekerRepository _jobSeekerRepository;

        public ResumeController(
            GenerateResumeAiUseCase useCase,
            ParseResumeUseCase parseUseCase,
            ParseResumeFileUseCase parseFileUseCase,
            IResumeDraftRepository draftRepository,
            IResumePdfGenerator pdfGenerator,
            IJobSeekerRepository jobSeekerRepository)
        {
            _useCase = useCase;
            _parseUseCase = parseUseCase;
            _parseFileUseCase = parseFileUseCase;
            _draftRepository = draftRepository;
            _pdfGenerator = pdfGenerator;
            _jobSeekerRepository = jobSeekerRepository;
        }

        [HttpPost("ai-generate")]
        public async Task<IActionResult> Generate(ResumeAiRequest req)
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var result = await _useCase.ExecuteAsync(req);
            await SaveAiResultAsync(userId, req, result);
            return Ok(result);
        }

        [HttpPost("parse")]
        public async Task<IActionResult> Parse(ResumeParseRequest request)
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var result = await _parseUseCase.ExecuteAsync(request);
            await SaveParseResultAsync(userId, result);
            return Ok(result);
        }

        [HttpPost("parse-file")]
        [RequestSizeLimit(10_000_000)]
        public async Task<IActionResult> ParseFile([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Resume file is required.");

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);

            var request = new ResumeFileParseRequest
            {
                Content = stream.ToArray(),
                FileName = file.FileName,
                ContentType = file.ContentType
            };

            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var result = await _parseFileUseCase.ExecuteAsync(request);
            await SaveParseResultAsync(userId, result);
            return Ok(result);
        }

        [HttpGet("draft")]
        public async Task<IActionResult> GetDraft()
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var draft = await _draftRepository.GetByUserIdAsync(userId);
            if (draft == null)
            {
                draft = await BuildDraftFromProfileAsync(userId);
                await _draftRepository.UpsertAsync(draft);
            }

            return Ok(draft);
        }

        [HttpPut("draft")]
        public async Task<IActionResult> SaveDraft([FromBody] ResumeDraftRequest request)
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            if (request == null)
                return BadRequest("Resume draft is required.");

            var draft = await _draftRepository.GetByUserIdAsync(userId) ?? new ResumeDraft
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            ApplyDraftRequest(draft, request);
            await _draftRepository.UpsertAsync(draft);
            return Ok(draft);
        }

        [HttpPost("pdf")]
        public async Task<IActionResult> GeneratePdf([FromBody] ResumeDraftRequest? request)
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            ResumeDraft draft;
            if (request != null)
            {
                draft = await _draftRepository.GetByUserIdAsync(userId) ?? new ResumeDraft
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };
                ApplyDraftRequest(draft, request);
                await _draftRepository.UpsertAsync(draft);
            }
            else
            {
                draft = await _draftRepository.GetByUserIdAsync(userId)
                    ?? await BuildDraftFromProfileAsync(userId);
            }

            var bytes = _pdfGenerator.Generate(draft);
            var fileName = $"resume_{DateTime.UtcNow:yyyyMMddHHmm}.pdf";
            return File(bytes, "application/pdf", fileName);
        }

        private string? GetUserId()
            => User.FindFirst("userId")?.Value ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        private async Task SaveParseResultAsync(string userId, ResumeParseResult result)
        {
            var draft = await _draftRepository.GetByUserIdAsync(userId)
                ?? await BuildDraftFromProfileAsync(userId);

            draft.FullName = result.FullName ?? draft.FullName;
            draft.Headline = result.Headline ?? draft.Headline;
            draft.Email = result.Email ?? draft.Email;
            draft.Phone = result.Phone ?? draft.Phone;
            draft.Summary = result.Summary ?? draft.Summary;
            draft.Skills = MergeList(result.Skills, draft.Skills);
            draft.WorkHistory = MergeList(result.WorkHistory, draft.WorkHistory);
            draft.EducationHistory = MergeList(result.EducationHistory, draft.EducationHistory);
            draft.Projects = MergeList(result.Projects, draft.Projects);
            draft.Certifications = MergeList(result.Certifications, draft.Certifications);
            draft.Languages = MergeList(result.Languages, draft.Languages);
            draft.LastParsedAt = DateTime.UtcNow;

            await _draftRepository.UpsertAsync(draft);
        }

        private async Task SaveAiResultAsync(string userId, ResumeAiRequest request, string aiText)
        {
            var draft = await _draftRepository.GetByUserIdAsync(userId)
                ?? await BuildDraftFromProfileAsync(userId);

            draft.Template = string.IsNullOrWhiteSpace(request.Template)
                ? draft.Template
                : request.Template;
            draft.AtsFriendly = request.AtsFriendly;
            draft.AiGeneratedText = aiText;
            draft.LastGeneratedAt = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(request.FullName))
                draft.FullName = request.FullName;
            if (!string.IsNullOrWhiteSpace(request.Summary))
                draft.Summary = request.Summary;
            if (request.Skills?.Count > 0)
                draft.Skills = request.Skills;
            if (request.WorkHistory?.Count > 0)
                draft.WorkHistory = request.WorkHistory;
            if (request.Projects?.Count > 0)
                draft.Projects = request.Projects;
            if (request.Certifications?.Count > 0)
                draft.Certifications = request.Certifications;

            await _draftRepository.UpsertAsync(draft);
        }

        private async Task<ResumeDraft> BuildDraftFromProfileAsync(string userId)
        {
            var profile = await _jobSeekerRepository.GetByUserIdAsync(userId);
            if (profile == null)
            {
                return new ResumeDraft
                {
                    UserId = userId,
                    Email = User.FindFirst("email")?.Value ?? string.Empty,
                    CreatedAt = DateTime.UtcNow
                };
            }

            return new ResumeDraft
            {
                UserId = userId,
                Template = profile.ResumeSettings?.Template ?? "modern",
                AtsFriendly = profile.ResumeSettings?.AtsFriendly ?? true,
                FullName = profile.FullName,
                Headline = profile.Headline,
                Email = profile.Email,
                Phone = profile.Phone,
                Location = profile.Location,
                Summary = profile.Summary,
                Skills = profile.Skills ?? new List<string>(),
                WorkHistory = profile.WorkHistory ?? new List<WorkExperience>(),
                EducationHistory = profile.EducationHistory ?? new List<EducationRecord>(),
                Projects = profile.Projects ?? new List<ProjectRecord>(),
                Certifications = profile.Certifications ?? new List<CertificationRecord>(),
                Languages = profile.Languages ?? new List<LanguageRecord>(),
                CreatedAt = DateTime.UtcNow
            };
        }

        private static void ApplyDraftRequest(ResumeDraft draft, ResumeDraftRequest request)
        {
            draft.Template = string.IsNullOrWhiteSpace(request.Template) ? draft.Template : request.Template;
            draft.AtsFriendly = request.AtsFriendly;
            draft.FullName = request.FullName ?? string.Empty;
            draft.Headline = request.Headline;
            draft.Email = request.Email ?? string.Empty;
            draft.Phone = request.Phone;
            draft.Location = request.Location;
            draft.Summary = request.Summary;
            draft.Skills = request.Skills ?? new();
            draft.WorkHistory = request.WorkHistory ?? new();
            draft.EducationHistory = request.EducationHistory ?? new();
            draft.Projects = request.Projects ?? new();
            draft.Certifications = request.Certifications ?? new();
            draft.Languages = request.Languages ?? new();
            draft.AiGeneratedText = request.AiGeneratedText;
        }

        private static List<T> MergeList<T>(List<T>? incoming, List<T> fallback)
        {
            return incoming != null && incoming.Count > 0 ? incoming : fallback;
        }
    }
}
