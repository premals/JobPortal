using JobProviderService.Application.Interfaces;
using JobProviderService.Domain;
using JobProviderService.DTO;
using JobProviderService.Infrastructure.Messaging;
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

        public CreateInterviewInviteUseCase(
            IJobRepository jobs,
            IJobApplicationRepository applications,
            IInterviewRepository interviews,
            IJobProviderSettingsRepository settings,
            IEmailService emailService,
            IEventBus eventBus)
        {
            _jobs = jobs;
            _applications = applications;
            _interviews = interviews;
            _settings = settings;
            _emailService = emailService;
            _eventBus = eventBus;
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

            var questionsCount = request.QuestionsCount > 0
                ? request.QuestionsCount
                : settings.Interview.QuestionsCount;

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
                ProposedSlots = request.ProposedSlots.Select(slot => new InterviewTimeSlot
                {
                    Start = slot,
                    End = slot.AddMinutes(settings.Interview.SlotDurationMinutes)
                }).ToList()
            };

            await _interviews.CreateInviteAsync(invite);

            var subject = ApplyTemplate(settings.Email.InviteSubject, job.Title, application.FullName);
            var body = ApplyTemplate(settings.Email.InviteBody, job.Title, application.FullName);
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

        private static string ApplyTemplate(string template, string jobTitle, string candidateName)
        {
            return template
                .Replace("{JobTitle}", jobTitle)
                .Replace("{CandidateName}", candidateName);
        }
    }
}
