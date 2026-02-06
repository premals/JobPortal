using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Linq;
using JobProviderService.Application.Interfaces;
using JobProviderService.Domain;
using JobProviderService.DTO;
using Microsoft.Extensions.Options;
using Azure.Core;
using Azure.Identity;

namespace JobProviderService.Infrastructure.AI
{
    public class AzureOpenAiInterviewService : IAiInterviewService
    {
        private readonly HttpClient _http;
        private readonly AzureOpenAiOptions _options;
        private readonly TokenCredential? _credential;
        private readonly bool _useEntraAuth;

        public AzureOpenAiInterviewService(HttpClient http, IOptions<AzureOpenAiOptions> options)
        {
            _http = http;
            _options = options.Value;
            _useEntraAuth = string.Equals(_options.AuthMode, "EntraId", StringComparison.OrdinalIgnoreCase);
            if (_useEntraAuth)
            {
                _credential = CreateCredential(_options);
            }
        }

        public async Task<AiShortlistSuggestionResponse> GetShortlistSuggestionAsync(
            Job job,
            JobApplication application,
            AiRuntimeConfig? config = null)
        {
            var prompt = $@"
You are an expert recruiter assistant. Provide a JSON object with fields:
recommendation (Shortlist|Hold|Reject), score (1-10), reasoning (short), risks (array).

Job:
Title: {job.Title}
Description: {job.Description}
KeySkills: {string.Join(", ", job.KeySkills)}
Experience: {job.MinExperience}-{job.MaxExperience} years

Candidate:
Name: {application.FullName}
Email: {application.Email}
ResumeUrl: {application.ResumeUrl}
Status: {application.Status}
";

            var json = await SendPromptAsync("Shortlist recommendation", prompt, config);
            if (TryDeserialize(json, out AiShortlistSuggestionResponse? result) && result != null)
                return result;

            return new AiShortlistSuggestionResponse
            {
                Recommendation = "Hold",
                Score = 5,
                Reasoning = "Unable to generate AI recommendation. Please review manually."
            };
        }

        public async Task<List<string>> GenerateInterviewQuestionsAsync(
            List<string> skills,
            string difficulty,
            int count,
            AiRuntimeConfig? config = null)
        {
            var prompt = $@"
Create {count} interview questions as a JSON array of strings.
Difficulty: {difficulty}
Skills: {string.Join(", ", skills)}
";

            var json = await SendPromptAsync("Interview question generation", prompt, config);
            if (TryDeserialize(json, out List<string>? questions) && questions != null && questions.Count > 0)
                return questions;

            return new List<string>
            {
                "Tell me about your most relevant project and the role you played.",
                "Describe a challenge you faced and how you resolved it."
            };
        }

        public async Task<InterviewEvaluation> EvaluateInterviewAsync(
            List<string> skills,
            string difficulty,
            List<InterviewMessage> transcript,
            AiRuntimeConfig? config = null)
        {
            var transcriptText = string.Join("\n", transcript.Select(t => $"{t.Role}: {t.Content}"));

            var prompt = $@"
You are an interviewer. Evaluate the candidate interview.
Return a JSON object with fields:
overallScore (1-10),
summary (short),
skillScores (array of objects with skill, score (1-10), feedback).

Difficulty: {difficulty}
Skills: {string.Join(", ", skills)}
Transcript:
{transcriptText}
";

            var json = await SendPromptAsync("Interview evaluation", prompt, config);
            if (TryDeserialize(json, out InterviewEvaluation? evaluation) && evaluation != null)
            {
                evaluation.EvaluatedAt = DateTime.UtcNow;
                return evaluation;
            }

            return new InterviewEvaluation
            {
                OverallScore = 5,
                Summary = "Evaluation not available. Please review manually.",
                SkillScores = skills.Select(skill => new SkillScore
                {
                    Skill = skill,
                    Score = 5,
                    Feedback = "No automated feedback available."
                }).ToList(),
                EvaluatedAt = DateTime.UtcNow
            };
        }

        private async Task<string> SendPromptAsync(string label, string prompt, AiRuntimeConfig? config)
        {
            var url = BuildUrl(config);

            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            await AddAuthHeaderAsync(request);

            var payload = new
            {
                messages = new[]
                {
                    new { role = "system", content = "Return valid JSON only. No markdown." },
                    new { role = "user", content = prompt }
                },
                temperature = 0.2,
                max_tokens = 800
            };

            request.Content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            var response = await _http.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Azure OpenAI call failed ({(int)response.StatusCode}): {content}");
            }

            return ExtractText(content);
        }

        private string BuildUrl(AiRuntimeConfig? config)
        {
            var endpoint = (config?.Endpoint ?? _options.Endpoint).TrimEnd('/');
            var deployment = config?.Deployment ?? _options.Deployment;
            if (string.IsNullOrWhiteSpace(deployment))
                throw new InvalidOperationException("Azure OpenAI deployment name is required.");

            var apiVersion = config?.ApiVersion ?? _options.ApiVersion;
            return $"{endpoint}/openai/deployments/{deployment}/chat/completions?api-version={apiVersion}";
        }

        private async Task AddAuthHeaderAsync(HttpRequestMessage request)
        {
            if (string.Equals(_options.AuthMode, "ApiKey", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(_options.ApiKey))
                    throw new InvalidOperationException("ApiKey auth selected but AzureOpenAI:ApiKey is missing.");

                request.Headers.Add("api-key", _options.ApiKey);
                return;
            }

            if (!_useEntraAuth || _credential == null)
                throw new InvalidOperationException("Entra ID auth selected but no credential could be created.");

            var scope = string.IsNullOrWhiteSpace(_options.TokenScope)
                ? "https://cognitiveservices.azure.com/.default"
                : _options.TokenScope;

            var token = await _credential.GetTokenAsync(
                new TokenRequestContext(new[] { scope }),
                CancellationToken.None);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
        }

        private static TokenCredential CreateCredential(AzureOpenAiOptions options)
        {
            if (!string.IsNullOrWhiteSpace(options.TenantId) &&
                !string.IsNullOrWhiteSpace(options.ClientId) &&
                !string.IsNullOrWhiteSpace(options.ClientSecret))
            {
                return new ClientSecretCredential(options.TenantId, options.ClientId, options.ClientSecret);
            }

            if (!string.IsNullOrWhiteSpace(options.ManagedIdentityClientId))
            {
                return new ManagedIdentityCredential(options.ManagedIdentityClientId);
            }

            return new DefaultAzureCredential();
        }

        private static string ExtractText(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("choices", out var choices) &&
                    choices.ValueKind == JsonValueKind.Array &&
                    choices.GetArrayLength() > 0)
                {
                    var message = choices[0].GetProperty("message");
                    if (message.TryGetProperty("content", out var contentElement))
                    {
                        if (contentElement.ValueKind == JsonValueKind.String)
                            return contentElement.GetString() ?? json;

                        if (contentElement.ValueKind == JsonValueKind.Array &&
                            contentElement.GetArrayLength() > 0)
                        {
                            var first = contentElement[0];
                            if (first.TryGetProperty("text", out var text))
                                return text.GetString() ?? json;
                        }
                    }
                }
            }
            catch
            {
                // ignore parse errors
            }

            return json;
        }

        private static bool TryDeserialize<T>(string json, out T? result)
        {
            try
            {
                result = JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return result != null;
            }
            catch
            {
                result = default;
                return false;
            }
        }
    }
}
