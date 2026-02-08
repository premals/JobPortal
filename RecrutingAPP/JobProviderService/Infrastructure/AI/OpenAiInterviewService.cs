using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using JobProviderService.Application.Interfaces;
using JobProviderService.Domain;
using JobProviderService.DTO;
using Microsoft.Extensions.Options;

namespace JobProviderService.Infrastructure.AI
{
    public class OpenAiInterviewService : IAiInterviewService
    {
        private readonly HttpClient _http;
        private readonly OpenAiOptions _options;

        public OpenAiInterviewService(HttpClient http, IOptions<OpenAiOptions> options)
        {
            _http = http;
            _options = options.Value;
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
            var apiKey = _options.ApiKey;
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("OpenAI ApiKey is required.");

            var baseUrl = FirstNonEmpty(config?.Endpoint, _options.BaseUrl, "https://api.openai.com/v1");
            var model = FirstNonEmpty(config?.Deployment, _options.Model);
            if (string.IsNullOrWhiteSpace(model))
                throw new InvalidOperationException("OpenAI model is required.");

            var requestUri = $"{baseUrl.TrimEnd('/')}/chat/completions";
            var payload = new
            {
                model,
                temperature = 0.2,
                messages = new[]
                {
                    new { role = "system", content = "Return valid JSON only. No markdown." },
                    new { role = "user", content = prompt }
                }
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            if (!string.IsNullOrWhiteSpace(_options.Organization))
                request.Headers.Add("OpenAI-Organization", _options.Organization);
            if (!string.IsNullOrWhiteSpace(_options.Project))
                request.Headers.Add("OpenAI-Project", _options.Project);

            request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            using var response = await _http.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"OpenAI {label} call failed ({(int)response.StatusCode}): {responseBody}");
            }

            var text = ExtractContentText(responseBody);
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new InvalidOperationException($"OpenAI {label} returned empty content.");
            }

            return text;
        }

        private static string ExtractContentText(string json)
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
                        return contentElement.GetString() ?? string.Empty;
                    }
                }
            }
            catch
            {
                // ignore parse errors
            }

            return json;
        }

        private static string FirstNonEmpty(params string?[] values)
        {
            foreach (var value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                    return value;
            }

            return string.Empty;
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
