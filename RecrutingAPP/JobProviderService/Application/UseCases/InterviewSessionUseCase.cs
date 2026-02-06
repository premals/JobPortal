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

        public InterviewSessionUseCase(
            IInterviewRepository interviews,
            IJobRepository jobs,
            IAiInterviewService ai,
            IJobProviderSettingsRepository settings)
        {
            _interviews = interviews;
            _jobs = jobs;
            _ai = ai;
            _settings = settings;
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

            invite.Status = "Accepted";
            invite.SelectedSlot = selectedSlot;
            await _interviews.UpdateInviteAsync(invite);

            var job = await _jobs.GetByIdAsync(invite.JobId);
            var skills = job?.KeySkills ?? new List<string>();

            var settings = await _settings.GetOrCreateAsync(invite.JobProviderId);
            var aiConfig = new AiRuntimeConfig
            {
                Endpoint = settings.Ai.Endpoint,
                Deployment = settings.Ai.Deployment,
                ApiVersion = settings.Ai.ApiVersion
            };

            var questions = settings.Ai.EnableInterviewAi
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
                Questions = questions
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
            }

            await _interviews.UpdateSessionAsync(session);
            return session;
        }
    }
}
