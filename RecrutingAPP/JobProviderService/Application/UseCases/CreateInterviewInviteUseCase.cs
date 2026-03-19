using JobProviderService.Application.Interfaces;
using JobProviderService.Domain;
using JobProviderService.DTO;
using JobProviderService.Infrastructure.Messaging;
using JobProviderService.Infrastructure.Options;
using Microsoft.Extensions.Options;
using System.Linq;
using static Shared.Contracts.Events.JobEvents;

namespace JobProviderService.Application.UseCases
{
    public class CreateInterviewInviteUseCase
    {
        private readonly IJobRepository _jobs;
        private readonly IJobApplicationRepository _applications;
        private readonly IInterviewRepository _interviews;
        private readonly IJobProviderSettingsRepository _settings;
        private readonly IEmailService _emailService;
        private readonly IEventBus _eventBus;
        private readonly AppUrlOptions _appUrls;
        private const int DefaultTokenExpiryDays = 7;

        public CreateInterviewInviteUseCase(
            IJobRepository jobs,
            IJobApplicationRepository applications,
            IInterviewRepository interviews,
            IJobProviderSettingsRepository settings,
            IEmailService emailService,
            IEventBus eventBus,
            IOptions<AppUrlOptions> appUrls)
        {
            _jobs = jobs;
            _applications = applications;
            _interviews = interviews;
            _settings = settings;
            _emailService = emailService;
            _eventBus = eventBus;
            _appUrls = appUrls.Value;
        }

        public async Task<InterviewInvite> ExecuteAsync(
            string jobId,
            string jobSeekerId,
            string providerId,
            InterviewInviteRequest request)
        {
            if (request.ProposedSlots == null || request.ProposedSlots.Count == 0)
                throw new ArgumentException("At least one time slot is required");

            var job = await _jobs.GetByIdAsync(jobId)
                ?? throw new InvalidOperationException("Job not found");

            if (job.JobProviderId != providerId)
                throw new UnauthorizedAccessException("You are not allowed to invite for this job");

            var application = await _applications.GetAsync(jobId, jobSeekerId)
                ?? throw new InvalidOperationException("Application not found");

            if (!string.Equals(application.Status, "Shortlisted", StringComparison.OrdinalIgnoreCase))
            {
                await _applications.UpdateAsync(jobId, jobSeekerId, "Shortlisted");
                application.Status = "Shortlisted";
            }

            var settings = await _settings.GetOrCreateAsync(providerId);

            var difficulty = settings.Interview.DifficultyLevels.Contains(request.Difficulty)
                ? request.Difficulty
                : settings.Interview.DefaultDifficulty;

            var customQuestions = request.CustomQuestions?
                .Where(q => !string.IsNullOrWhiteSpace(q))
                .Select(q => q.Trim())
                .Distinct()
                .ToList() ?? new List<string>();

            var questionsCount = request.QuestionsCount > 0
                ? request.QuestionsCount
                : settings.Interview.QuestionsCount;

            if (customQuestions.Count > 0)
            {
                questionsCount = customQuestions.Count;
            }

            var invite = new InterviewInvite
            {
                JobId = jobId,
                JobTitle = job.Title,
                JobProviderId = providerId,
                JobSeekerId = jobSeekerId,
                CandidateName = application.FullName,
                CandidateEmail = application.Email,
                Difficulty = difficulty,
                QuestionsCount = questionsCount,
                CustomQuestions = customQuestions,
                ProposedSlots = request.ProposedSlots.Select(slot => new InterviewTimeSlot
                {
                    Start = slot,
                    End = slot.AddMinutes(settings.Interview.SlotDurationMinutes)
                }).ToList(),
                PublicToken = GenerateToken(),
                TokenExpiresAt = DateTime.UtcNow.AddDays(DefaultTokenExpiryDays)
            };

            await _interviews.CreateInviteAsync(invite);

            var interviewLink = BuildInterviewLink(invite.PublicToken);
            var subject = ApplyTemplate(settings.Email.InviteSubject, job.Title, application.FullName, interviewLink);
            var body = ApplyTemplate(settings.Email.InviteBody, job.Title, application.FullName, interviewLink);
            await _emailService.SendAsync(application.Email, subject, body);

            await _eventBus.PublishAsync(new InterviewInviteCreatedEvent
            {
                InviteId = invite.Id,
                JobId = jobId,
                JobTitle = job.Title,
                JobProviderId = providerId,
                JobSeekerId = jobSeekerId,
                CandidateName = application.FullName,
                CandidateEmail = application.Email,
                Difficulty = invite.Difficulty,
                ProposedSlots = request.ProposedSlots
            });

            return invite;
        }

        private string BuildInterviewLink(string? token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return string.Empty;

            if (string.IsNullOrWhiteSpace(_appUrls.FrontendBaseUrl))
                return string.Empty;

            var baseUrl = _appUrls.FrontendBaseUrl.TrimEnd('/');
            var path = string.IsNullOrWhiteSpace(_appUrls.PublicInterviewPath)
                ? "/public-interview"
                : _appUrls.PublicInterviewPath.Trim();
            if (!path.StartsWith("/"))
                path = "/" + path;

            return $"{baseUrl}{path}?token={Uri.EscapeDataString(token)}";
        }

        private static string ApplyTemplate(string template, string jobTitle, string candidateName, string? interviewLink)
        {
            var rendered = template
                .Replace("{JobTitle}", jobTitle)
                .Replace("{CandidateName}", candidateName);

            if (!string.IsNullOrWhiteSpace(interviewLink))
            {
                if (rendered.Contains("{InterviewLink}"))
                {
                    rendered = rendered.Replace("{InterviewLink}", interviewLink);
                }
                else
                {
                    rendered = $"{rendered}\n\nInterview Link: {interviewLink}";
                }
            }

            return rendered;
        }

        private static string GenerateToken()
        {
            return Guid.NewGuid().ToString("N");
        }
    }
}
