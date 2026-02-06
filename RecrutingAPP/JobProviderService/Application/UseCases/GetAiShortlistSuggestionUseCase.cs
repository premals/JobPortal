using JobProviderService.Application.Interfaces;
using JobProviderService.DTO;
using System.Collections.Generic;

namespace JobProviderService.Application.UseCases
{
    public class GetAiShortlistSuggestionUseCase
    {
        private readonly IJobRepository _jobs;
        private readonly IJobApplicationRepository _applications;
        private readonly IAiInterviewService _ai;
        private readonly IJobProviderSettingsRepository _settings;

        public GetAiShortlistSuggestionUseCase(
            IJobRepository jobs,
            IJobApplicationRepository applications,
            IAiInterviewService ai,
            IJobProviderSettingsRepository settings)
        {
            _jobs = jobs;
            _applications = applications;
            _ai = ai;
            _settings = settings;
        }

        public async Task<AiShortlistSuggestionResponse> ExecuteAsync(
            string jobId,
            string jobSeekerId,
            string providerId)
        {
            var job = await _jobs.GetByIdAsync(jobId)
                ?? throw new InvalidOperationException("Job not found");

            if (job.JobProviderId != providerId)
                throw new UnauthorizedAccessException("Not allowed");

            var application = await _applications.GetAsync(jobId, jobSeekerId)
                ?? throw new InvalidOperationException("Application not found");

            var settings = await _settings.GetOrCreateAsync(providerId);
            if (!settings.Ai.EnableShortlistSuggestions)
            {
                return new AiShortlistSuggestionResponse
                {
                    Recommendation = "Hold",
                    Score = 5,
                    Reasoning = "AI suggestions are disabled in settings.",
                    Risks = new List<string>()
                };
            }

            var config = new AiRuntimeConfig
            {
                Endpoint = settings.Ai.Endpoint,
                Deployment = settings.Ai.Deployment,
                ApiVersion = settings.Ai.ApiVersion
            };

            return await _ai.GetShortlistSuggestionAsync(job, application, config);
        }
    }
}
