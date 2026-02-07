using System.Text.Json;
using System.Linq;
using JobProviderService.Application.Interfaces;
using JobProviderService.Domain;
using JobProviderService.DTO;
using Microsoft.Extensions.Options;
using Azure;
using Azure.AI.OpenAI;
using Azure.Core;
using Azure.Identity;
using OpenAI.Chat;
using System.ClientModel;

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
            _useEntraAuth = UsesEntraAuth(_options.AuthMode);
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
            try
            {
                var chatClient = CreateChatClient(config);
                var messages = new ChatMessage[]
                {
                    new SystemChatMessage("Return valid JSON only. No markdown."),
                    new UserChatMessage(prompt)
                };

                ChatCompletion completion = await chatClient.CompleteChatAsync(messages);
                var text = ExtractContentText(completion);
                if (string.IsNullOrWhiteSpace(text))
                {
                    throw new InvalidOperationException("Azure OpenAI returned empty content.");
                }

                return text;
            }
            catch (RequestFailedException ex)
            {
                throw new HttpRequestException(
                    $"Azure OpenAI call failed ({ex.Status}): {ex.Message}", ex);
            }
        }

        private ChatClient CreateChatClient(AiRuntimeConfig? config)
        {
            var endpoint = ResolveEndpoint(config);
            var deployment = FirstNonEmpty(config?.Deployment, _options.Deployment);
            if (string.IsNullOrWhiteSpace(deployment))
                throw new InvalidOperationException("Azure OpenAI deployment name is required.");

            var clientOptions = BuildClientOptions(config);
            var uri = new Uri(endpoint);

            AzureOpenAIClient azureClient;
            if (IsApiKeyAuth(_options.AuthMode))
            {
                if (string.IsNullOrWhiteSpace(_options.ApiKey))
                    throw new InvalidOperationException("ApiKey auth selected but AzureOpenAI:ApiKey is missing.");

                azureClient = new AzureOpenAIClient(uri, new ApiKeyCredential(_options.ApiKey), clientOptions);
            }
            else
            {
                var credential = _credential ?? CreateCredential(_options);
                azureClient = new AzureOpenAIClient(uri, credential, clientOptions);
            }

            return azureClient.GetChatClient(deployment);
        }

        private static string ExtractContentText(ChatCompletion completion)
        {
            if (completion.Content == null || completion.Content.Count == 0)
            {
                return string.Empty;
            }

            if (completion.Content.Count == 1)
            {
                return completion.Content[0].Text ?? string.Empty;
            }

            return string.Concat(completion.Content.Select(part => part.Text));
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

            var credentialOptions = new DefaultAzureCredentialOptions();
            if (!string.IsNullOrWhiteSpace(options.TenantId))
            {
                credentialOptions.TenantId = options.TenantId;
                credentialOptions.AdditionallyAllowedTenants.Add(options.TenantId);
                credentialOptions.AdditionallyAllowedTenants.Add("*");
                credentialOptions.InteractiveBrowserTenantId = options.TenantId;
                credentialOptions.SharedTokenCacheTenantId = options.TenantId;
                credentialOptions.VisualStudioTenantId = options.TenantId;
                credentialOptions.VisualStudioCodeTenantId = options.TenantId;
            }

            return new DefaultAzureCredential(credentialOptions);
        }

        private static bool IsApiKeyAuth(string? authMode)
        {
            return string.Equals(authMode, "ApiKey", StringComparison.OrdinalIgnoreCase);
        }

        private static bool UsesEntraAuth(string? authMode)
        {
            return string.Equals(authMode, "EntraId", StringComparison.OrdinalIgnoreCase)
                || string.Equals(authMode, "DefaultCredential", StringComparison.OrdinalIgnoreCase)
                || string.Equals(authMode, "DefaultAzureCredential", StringComparison.OrdinalIgnoreCase)
                || string.Equals(authMode, "ManagedIdentity", StringComparison.OrdinalIgnoreCase);
        }

        private string ResolveEndpoint(AiRuntimeConfig? config)
        {
            var configured = config?.Endpoint?.Trim();
            if (string.IsNullOrWhiteSpace(configured))
                return _options.Endpoint.TrimEnd('/');

            // When using Entra ID auth locally, prefer the app-configured OpenAI endpoint
            // to avoid hitting a different Azure AI Services resource without permissions.
            if (_useEntraAuth && !EndpointsMatch(configured, _options.Endpoint))
                return _options.Endpoint.TrimEnd('/');

            return configured.TrimEnd('/');
        }

        private static bool EndpointsMatch(string left, string right)
        {
            return string.Equals(NormalizeEndpoint(left), NormalizeEndpoint(right), StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeEndpoint(string endpoint)
        {
            return endpoint.Trim().TrimEnd('/');
        }

        private static string FirstNonEmpty(string? primary, string? fallback)
        {
            return !string.IsNullOrWhiteSpace(primary)
                ? primary
                : (fallback ?? string.Empty);
        }

        private AzureOpenAIClientOptions BuildClientOptions(AiRuntimeConfig? config)
        {
            var apiVersion = FirstNonEmpty(config?.ApiVersion, _options.ApiVersion);
            if (TryMapServiceVersion(apiVersion, out var version))
            {
                return new AzureOpenAIClientOptions(version);
            }

            return new AzureOpenAIClientOptions();
        }

        private static bool TryMapServiceVersion(string? apiVersion, out AzureOpenAIClientOptions.ServiceVersion version)
        {
            version = default;
            if (string.IsNullOrWhiteSpace(apiVersion))
                return false;

            var enumName = ToServiceVersionEnumName(apiVersion);
            return Enum.TryParse(enumName, ignoreCase: false, out version);
        }

        private static string ToServiceVersionEnumName(string apiVersion)
        {
            var normalized = apiVersion.Trim().ToLowerInvariant();
            if (normalized.EndsWith("-preview", StringComparison.Ordinal))
            {
                normalized = normalized[..^("-preview".Length)];
                normalized = normalized.Replace("-", "_", StringComparison.Ordinal);
                return $"V{normalized}_Preview";
            }

            normalized = normalized.Replace("-", "_", StringComparison.Ordinal);
            return $"V{normalized}";
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
