using JobProviderService.Domain;
using JobProviderService.DTO;

namespace JobProviderService.Application.Interfaces
{
    public interface IAiInterviewService
    {
        Task<AiShortlistSuggestionResponse> GetShortlistSuggestionAsync(
            Job job,
            JobApplication application,
            AiRuntimeConfig? config = null);

        Task<List<string>> GenerateInterviewQuestionsAsync(
            List<string> skills,
            string difficulty,
            int count,
            AiRuntimeConfig? config = null);

        Task<InterviewEvaluation> EvaluateInterviewAsync(
            List<string> skills,
            string difficulty,
            List<InterviewMessage> transcript,
            AiRuntimeConfig? config = null);
    }

    public class AiRuntimeConfig
    {
        public string? Endpoint { get; set; }
        public string? Deployment { get; set; }
        public string? ApiVersion { get; set; }
    }
}
