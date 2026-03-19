using JobProviderService.Application.Interfaces;
using JobProviderService.Domain;
using System.Linq;
using System.Collections.Generic;

namespace JobProviderService.Application.UseCases
{
    public class InterviewSessionUseCase
    {
        private readonly IInterviewRepository _interviews;
        private readonly IJobRepository _jobs;
        private readonly IAiInterviewService _ai;
        private readonly IJobProviderSettingsRepository _settings;
        private readonly UpdateApplicationStatusUseCase _applicationStatus;
        private const double AutoDecisionThresholdPercent = 60.0;

        public InterviewSessionUseCase(
            IInterviewRepository interviews,
            IJobRepository jobs,
            IAiInterviewService ai,
            IJobProviderSettingsRepository settings,
            UpdateApplicationStatusUseCase applicationStatus)
        {
            _interviews = interviews;
            _jobs = jobs;
            _ai = ai;
            _settings = settings;
            _applicationStatus = applicationStatus;
        }

        public async Task<InterviewSession> AcceptInviteAsync(
            string inviteId,
            string seekerId,
            DateTime selectedSlot)
        {
            var invite = await _interviews.GetInviteAsync(inviteId)
                ?? throw new InvalidOperationException("Invite not found");

            if (invite.JobSeekerId != seekerId)
                throw new UnauthorizedAccessException("Not allowed");

            return await AcceptInviteInternalAsync(invite, selectedSlot);
        }

        public async Task<InterviewSession> AcceptInviteByTokenAsync(string token, DateTime selectedSlot)
        {
            var invite = await _interviews.GetInviteByTokenAsync(token)
                ?? throw new InvalidOperationException("Invite not found");

            if (invite.TokenUsedAt.HasValue)
                throw new InvalidOperationException("Invite token already used");

            if (invite.TokenExpiresAt.HasValue && invite.TokenExpiresAt.Value < DateTime.UtcNow)
                throw new InvalidOperationException("Invite token expired");

            return await AcceptInviteInternalAsync(invite, selectedSlot);
        }

        public async Task<bool> MarkNoShowAsync(InterviewInvite invite, InterviewSession? session)
        {
            ArgumentNullException.ThrowIfNull(invite);

            if (string.Equals(invite.Status, "Completed", StringComparison.OrdinalIgnoreCase)
                || string.Equals(invite.Status, "NoShow", StringComparison.OrdinalIgnoreCase)
                || invite.TokenUsedAt.HasValue)
            {
                return false;
            }

            invite.Status = "NoShow";
            invite.TokenUsedAt = DateTime.UtcNow;
            await _interviews.UpdateInviteAsync(invite);

            if (session != null && string.Equals(session.Status, "Scheduled", StringComparison.OrdinalIgnoreCase))
            {
                session.Status = "NoShow";
                await _interviews.UpdateSessionAsync(session);
            }

            await _applicationStatus.ExecuteAsync(
                invite.JobId,
                invite.JobSeekerId,
                invite.JobProviderId,
                "Rejected");

            return true;
        }

        private async Task<InterviewSession> AcceptInviteInternalAsync(InterviewInvite invite, DateTime selectedSlot)
        {
            if (string.Equals(invite.Status, "Accepted", StringComparison.OrdinalIgnoreCase))
            {
                var existing = await _interviews.GetSessionByInviteIdAsync(invite.Id);
                if (existing != null)
                    return existing;
            }

            invite.Status = "Accepted";
            invite.SelectedSlot = selectedSlot;
            await _interviews.UpdateInviteAsync(invite);

            var job = await _jobs.GetByIdAsync(invite.JobId);
            var skills = job?.KeySkills ?? new List<string>();

            var settings = await _settings.GetOrCreateAsync(invite.JobProviderId);
            var aiConfig = new AiRuntimeConfig
            {
                Provider = settings.Ai.Provider,
                Endpoint = settings.Ai.Endpoint,
                Deployment = settings.Ai.Deployment,
                ApiVersion = settings.Ai.ApiVersion
            };

            var customQuestions = invite.CustomQuestions?.Where(q => !string.IsNullOrWhiteSpace(q)).ToList()
                ?? new List<string>();

            var questions = customQuestions.Count > 0
                ? customQuestions
                : settings.Ai.EnableInterviewAi
                    ? await _ai.GenerateInterviewQuestionsAsync(
                        skills,
                        invite.Difficulty,
                        invite.QuestionsCount,
                        aiConfig)
                    : new List<string>
                    {
                        "Tell us about your most relevant project.",
                        "What challenges did you face and how did you solve them?"
                    };

            var avatarProvider = (settings.Ai.EnableAvatar || !string.IsNullOrWhiteSpace(settings.Ai.AvatarProvider))
                ? settings.Ai.AvatarProvider
                : null;

            var session = new InterviewSession
            {
                InviteId = invite.Id,
                JobId = invite.JobId,
                JobProviderId = invite.JobProviderId,
                JobSeekerId = invite.JobSeekerId,
                Difficulty = invite.Difficulty,
                Skills = skills,
                Status = "Scheduled",
                TotalQuestions = questions.Count,
                Questions = questions,
                ScheduledStart = invite.SelectedSlot,
                ScheduledEnd = invite.SelectedSlot?.AddMinutes(settings.Interview.SlotDurationMinutes),
                AvatarProvider = avatarProvider
            };

            await _interviews.CreateSessionAsync(session);
            return session;
        }

        public async Task<string> StartSessionAsync(string sessionId, string seekerId)
        {
            var session = await _interviews.GetSessionAsync(sessionId)
                ?? throw new InvalidOperationException("Session not found");

            if (session.JobSeekerId != seekerId)
                throw new UnauthorizedAccessException("Not allowed");

            if (session.Status == "Scheduled")
            {
                session.Status = "InProgress";
                await _interviews.UpdateSessionAsync(session);
            }

            return session.Questions.First();
        }

        public async Task<InterviewSession> AnswerAsync(string sessionId, string seekerId, string answer)
        {
            var session = await _interviews.GetSessionAsync(sessionId)
                ?? throw new InvalidOperationException("Session not found");

            if (session.JobSeekerId != seekerId)
                throw new UnauthorizedAccessException("Not allowed");

            if (session.Status != "InProgress")
                throw new InvalidOperationException("Interview not in progress");

            var questionIndex = session.CurrentQuestionIndex;
            var question = session.Questions.ElementAtOrDefault(questionIndex);

            session.Transcript.Add(new InterviewMessage
            {
                Role = "assistant",
                Content = question ?? string.Empty
            });
            session.Transcript.Add(new InterviewMessage
            {
                Role = "user",
                Content = answer
            });

            session.CurrentQuestionIndex++;

            if (session.CurrentQuestionIndex >= session.TotalQuestions)
            {
                session.Status = "Completed";
                var settings = await _settings.GetOrCreateAsync(session.JobProviderId);
                var aiConfig = new AiRuntimeConfig
                {
                    Provider = settings.Ai.Provider,
                    Endpoint = settings.Ai.Endpoint,
                    Deployment = settings.Ai.Deployment,
                    ApiVersion = settings.Ai.ApiVersion
                };

                session.Evaluation = settings.Ai.EnableInterviewAi
                    ? await _ai.EvaluateInterviewAsync(
                        session.Skills,
                        session.Difficulty,
                        session.Transcript,
                        aiConfig)
                    : new InterviewEvaluation
                    {
                        OverallScore = 5,
                        Summary = "AI evaluation disabled. Please review manually.",
                        SkillScores = session.Skills.Select(skill => new SkillScore
                        {
                            Skill = skill,
                            Score = 5,
                            Feedback = "Manual review needed."
                        }).ToList()
                    };

                if (settings.Ai.EnableInterviewAi && session.Evaluation != null && string.IsNullOrWhiteSpace(session.AutoDecisionStatus))
                {
                    var percentScore = session.Evaluation.OverallScore * 10.0;
                    var targetStatus = percentScore >= AutoDecisionThresholdPercent ? "Shortlisted" : "Rejected";
                    await _applicationStatus.ExecuteAsync(
                        session.JobId,
                        session.JobSeekerId,
                        session.JobProviderId,
                        targetStatus);

                    session.AutoDecisionStatus = targetStatus;
                    session.AutoDecisionScore = percentScore;
                    session.AutoDecisionAt = DateTime.UtcNow;
                }

                var invite = await _interviews.GetInviteAsync(session.InviteId);
                if (invite != null && !invite.TokenUsedAt.HasValue)
                {
                    invite.TokenUsedAt = DateTime.UtcNow;
                    invite.Status = "Completed";
                    await _interviews.UpdateInviteAsync(invite);
                }
            }

            await _interviews.UpdateSessionAsync(session);
            return session;
        }
    }
}
