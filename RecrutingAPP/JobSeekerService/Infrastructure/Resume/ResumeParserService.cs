using System.Net.Http.Headers;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Azure.Core;
using Azure.Identity;
using JobSeekerService.Application.DTOs;
using JobSeekerService.Application.Interfaces;
using JobSeekerService.Domain.Entities;
using JobSeekerService.Infrastructure.AI;
using Microsoft.Extensions.Options;
using NPOI.XWPF.UserModel;
using UglyToad.PdfPig;

namespace JobSeekerService.Infrastructure.Resume
{
    public class ResumeParserService : IResumeParserService
    {
        private readonly HttpClient _http;
        private readonly OpenAiOptions _openAiOptions;
        private readonly AzureOpenAiOptions _azureOptions;
        private readonly AiProviderOptions _providerOptions;
        private readonly DefaultAzureCredential _azureCredential;
        private readonly TokenRequestContext _azureTokenContext;
        private readonly ILogger<ResumeParserService> _logger;

        public ResumeParserService(
            HttpClient http,
            IOptions<OpenAiOptions> openAiOptions,
            IOptions<AzureOpenAiOptions> azureOptions,
            IOptions<AiProviderOptions> providerOptions,
            ILogger<ResumeParserService> logger)
        {
            _http = http;
            _openAiOptions = openAiOptions.Value;
            _azureOptions = azureOptions.Value;
            _providerOptions = providerOptions.Value;
            _azureCredential = new DefaultAzureCredential();
            var scope = string.IsNullOrWhiteSpace(_azureOptions.TokenScope)
                ? "https://cognitiveservices.azure.com/.default"
                : _azureOptions.TokenScope.Trim();
            _azureTokenContext = new TokenRequestContext(new[] { scope });
            _logger = logger;
        }

        public async Task<ResumeParseResult> ParseAsync(ResumeParseRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Text))
                throw new ArgumentException("Resume text is required.");

            var aiResult = await TryParseWithAiAsync(request.Text);
            if (aiResult != null)
                return Normalize(aiResult);

            return ParseText(request.Text);
        }

        public async Task<ResumeParseResult> ParseFileAsync(ResumeFileParseRequest request)
        {
            if (request == null || request.Content == null || request.Content.Length == 0)
                throw new ArgumentException("Resume file is required.");

            var text = ExtractTextFromFile(request.Content, request.FileName, request.ContentType);
            if (string.IsNullOrWhiteSpace(text))
                throw new InvalidOperationException("Unable to extract text from the resume file.");

            var aiResult = await TryParseWithAiAsync(text);
            if (aiResult != null)
                return Normalize(aiResult);

            return ParseText(text);
        }

        private static ResumeParseResult ParseText(string text)
        {
            var normalized = text.Replace("\r", "\n");
            var lines = normalized
                .Split('\n')
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToList();

            var result = new ResumeParseResult
            {
                Email = ExtractEmail(normalized),
                Phone = ExtractPhone(normalized)
            };

            result.FullName = ExtractName(lines, result.Email, result.Phone);
            result.Headline = ExtractHeadline(lines, result.FullName, result.Email, result.Phone);
            result.Summary = ExtractSectionText(lines, new[]
            {
                new Regex(@"^summary\b", RegexOptions.IgnoreCase),
                new Regex(@"^profile\b", RegexOptions.IgnoreCase),
                new Regex(@"^objective\b", RegexOptions.IgnoreCase)
            });

            var skills = ExtractSkills(lines);
            if (skills.Count > 0)
                result.Skills = skills;

            var experienceYears = ExtractExperienceYears(normalized);
            if (!experienceYears.HasValue || experienceYears.Value == 0)
                experienceYears = ExtractExperienceYearsFromDates(lines);
            if (experienceYears.HasValue && experienceYears.Value > 0)
                result.ExperienceYears = experienceYears.Value;

            var education = ExtractEducationRecord(lines);
            if (education != null)
            {
                result.EducationHistory.Add(education);
                result.Education = !string.IsNullOrWhiteSpace(education.Degree) ? education.Degree : education.School;
            }

            return result;
        }

        private async Task<ResumeParseResult?> TryParseWithAiAsync(string text)
        {
            return IsAzureProvider(_providerOptions.Provider)
                ? await TryParseWithAzureAiAsync(text)
                : await TryParseWithOpenAiAsync(text);
        }

        private async Task<ResumeParseResult?> TryParseWithOpenAiAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(_openAiOptions.ApiKey) || string.IsNullOrWhiteSpace(_openAiOptions.Model))
                return null;

            var baseUrl = string.IsNullOrWhiteSpace(_openAiOptions.BaseUrl)
                ? "https://api.openai.com/v1"
                : _openAiOptions.BaseUrl.TrimEnd('/');

            var payload = new
            {
                model = _openAiOptions.Model,
                temperature = 0.0,
                response_format = BuildResumeResponseSchema(),
                messages = BuildResumeMessages(text)
            };

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/chat/completions");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _openAiOptions.ApiKey);
                if (!string.IsNullOrWhiteSpace(_openAiOptions.Organization))
                    request.Headers.Add("OpenAI-Organization", _openAiOptions.Organization);
                if (!string.IsNullOrWhiteSpace(_openAiOptions.Project))
                    request.Headers.Add("OpenAI-Project", _openAiOptions.Project);

                request.Content = new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json");

                using var response = await _http.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("OpenAI resume parse failed: {Status} {Body}", response.StatusCode, responseBody);
                    return null;
                }

                return DeserializeResumeResult(responseBody);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "OpenAI resume parse threw an exception.");
                return null;
            }
        }

        private async Task<ResumeParseResult?> TryParseWithAzureAiAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(_azureOptions.Endpoint) ||
                string.IsNullOrWhiteSpace(_azureOptions.Deployment) ||
                string.IsNullOrWhiteSpace(_azureOptions.ApiVersion))
            {
                return null;
            }

            var endpoint = _azureOptions.Endpoint.TrimEnd('/');
            var requestUri =
                $"{endpoint}/openai/deployments/{_azureOptions.Deployment}/chat/completions?api-version={_azureOptions.ApiVersion}";

            var payload = new
            {
                temperature = 0.0,
                response_format = BuildResumeResponseSchema(),
                messages = BuildResumeMessages(text)
            };

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
                var token = await _azureCredential.GetTokenAsync(_azureTokenContext, default);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
                request.Content = new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json");

                using var response = await _http.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Azure OpenAI resume parse failed: {Status} {Body}", response.StatusCode, responseBody);
                    return null;
                }

                return DeserializeResumeResult(responseBody);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Azure OpenAI resume parse threw an exception.");
                return null;
            }
        }

        private static object BuildResumeResponseSchema()
        {
            return new
            {
                type = "json_schema",
                json_schema = new
                {
                    name = "resume_parse",
                    strict = false,
                    schema = new
                    {
                        type = "object",
                        properties = new
                        {
                            fullName = new { type = new[] { "string", "null" } },
                            email = new { type = new[] { "string", "null" } },
                            phone = new { type = new[] { "string", "null" } },
                            headline = new { type = new[] { "string", "null" } },
                            summary = new { type = new[] { "string", "null" } },
                            experienceYears = new { type = new[] { "integer", "null" } },
                            skills = new { type = new[] { "array", "null" }, items = new { type = "string" } },
                            education = new { type = new[] { "string", "null" } },
                            workHistory = new
                            {
                                type = new[] { "array", "null" },
                                items = new
                                {
                                    type = "object",
                                    properties = new
                                    {
                                        company = new { type = new[] { "string", "null" } },
                                        role = new { type = new[] { "string", "null" } },
                                        startDate = new { type = new[] { "string", "null" } },
                                        endDate = new { type = new[] { "string", "null" } },
                                        description = new { type = new[] { "string", "null" } },
                                        skills = new { type = new[] { "array", "null" }, items = new { type = "string" } }
                                    }
                                }
                            },
                            educationHistory = new
                            {
                                type = new[] { "array", "null" },
                                items = new
                                {
                                    type = "object",
                                    properties = new
                                    {
                                        school = new { type = new[] { "string", "null" } },
                                        degree = new { type = new[] { "string", "null" } },
                                        field = new { type = new[] { "string", "null" } },
                                        graduationYear = new { type = new[] { "string", "null" } }
                                    }
                                }
                            },
                            projects = new
                            {
                                type = new[] { "array", "null" },
                                items = new
                                {
                                    type = "object",
                                    properties = new
                                    {
                                        name = new { type = new[] { "string", "null" } },
                                        role = new { type = new[] { "string", "null" } },
                                        description = new { type = new[] { "string", "null" } },
                                        link = new { type = new[] { "string", "null" } }
                                    }
                                }
                            },
                            certifications = new
                            {
                                type = new[] { "array", "null" },
                                items = new
                                {
                                    type = "object",
                                    properties = new
                                    {
                                        name = new { type = new[] { "string", "null" } },
                                        issuer = new { type = new[] { "string", "null" } },
                                        year = new { type = new[] { "string", "null" } }
                                    }
                                }
                            },
                            languages = new
                            {
                                type = new[] { "array", "null" },
                                items = new
                                {
                                    type = "object",
                                    properties = new
                                    {
                                        name = new { type = new[] { "string", "null" } },
                                        proficiency = new { type = new[] { "string", "null" } }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        private static object[] BuildResumeMessages(string text)
        {
            return new[]
            {
                new
                {
                    role = "system",
                    content = "You extract structured resume data. Return JSON only. Use empty arrays for missing lists and null for unknown strings."
                },
                new
                {
                    role = "user",
                    content = $"Extract resume details from the text below. Fill skills and experience years carefully.\n\nResume:\n{text}"
                }
            };
        }

        private ResumeParseResult? DeserializeResumeResult(string responseBody)
        {
            var json = ExtractContentText(responseBody);
            if (string.IsNullOrWhiteSpace(json))
                return null;

            return System.Text.Json.JsonSerializer.Deserialize<ResumeParseResult>(
                json,
                new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }

        private static bool IsAzureProvider(string? provider)
        {
            if (string.IsNullOrWhiteSpace(provider))
                return false;

            var normalized = provider.Trim();
            return normalized.Equals("AzureAI", StringComparison.OrdinalIgnoreCase)
                || normalized.Equals("AzureOpenAI", StringComparison.OrdinalIgnoreCase)
                || normalized.Equals("Azure", StringComparison.OrdinalIgnoreCase)
                || normalized.Equals("Azure AI Foundry", StringComparison.OrdinalIgnoreCase);
        }

        private static string ExtractContentText(string json)
        {
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("choices", out var choices) &&
                    choices.ValueKind == System.Text.Json.JsonValueKind.Array &&
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

        private static ResumeParseResult Normalize(ResumeParseResult result)
        {
            result.Skills ??= new List<string>();
            result.WorkHistory ??= new List<WorkExperience>();
            result.EducationHistory ??= new List<EducationRecord>();
            result.Projects ??= new List<ProjectRecord>();
            result.Certifications ??= new List<CertificationRecord>();
            result.Languages ??= new List<LanguageRecord>();

            result.Skills = result.Skills
                .Where(skill => !string.IsNullOrWhiteSpace(skill))
                .Select(skill => skill.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (!result.ExperienceYears.HasValue || result.ExperienceYears.Value <= 0)
            {
                var computed = ComputeExperienceYearsFromWorkHistory(result.WorkHistory);
                if (computed.HasValue)
                    result.ExperienceYears = computed.Value;
            }

            return result;
        }

        private static int? ComputeExperienceYearsFromWorkHistory(List<WorkExperience> workHistory)
        {
            if (workHistory == null || workHistory.Count == 0)
                return null;

            var years = new List<int>();
            var hasPresent = workHistory.Any(item =>
                (item.EndDate ?? string.Empty).Contains("present", StringComparison.OrdinalIgnoreCase) ||
                (item.EndDate ?? string.Empty).Contains("current", StringComparison.OrdinalIgnoreCase));

            foreach (var item in workHistory)
            {
                foreach (Match match in Regex.Matches($"{item.StartDate} {item.EndDate}", @"\b(19|20)\d{2}\b"))
                {
                    if (int.TryParse(match.Value, out var year))
                        years.Add(year);
                }
            }

            if (years.Count == 0)
                return null;

            var minYear = years.Min();
            var maxYear = hasPresent ? DateTime.UtcNow.Year : years.Max();
            var diff = Math.Max(0, maxYear - minYear);
            return diff == 0 ? 1 : diff;
        }

        private static string ExtractTextFromFile(byte[] content, string fileName, string? contentType)
        {
            var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
            var isText = extension == ".txt" || extension == ".md" || (contentType?.StartsWith("text/") ?? false);

            if (isText)
                return Encoding.UTF8.GetString(content);

            return extension switch
            {
                ".pdf" => ExtractPdfText(content),
                ".docx" => ExtractDocxText(content),
                ".doc" => throw new InvalidOperationException("Legacy .doc files are not supported. Please upload .docx instead."),
                _ => throw new InvalidOperationException("Unsupported resume file type. Use .pdf, .docx, .txt, or .md.")
            };
        }

        private static string ExtractPdfText(byte[] content)
        {
            using var stream = new MemoryStream(content);
            using var pdf = PdfDocument.Open(stream);
            var builder = new StringBuilder();
            foreach (var page in pdf.GetPages())
            {
                builder.AppendLine(page.Text);
            }
            return builder.ToString();
        }

        private static string ExtractDocxText(byte[] content)
        {
            using var stream = new MemoryStream(content);
            var document = new XWPFDocument(stream);
            var builder = new StringBuilder();

            foreach (var paragraph in document.Paragraphs)
            {
                if (!string.IsNullOrWhiteSpace(paragraph.Text))
                    builder.AppendLine(paragraph.Text);
            }

            foreach (var table in document.Tables)
            {
                foreach (var row in table.Rows)
                {
                    foreach (var cell in row.GetTableCells())
                    {
                        foreach (var paragraph in cell.Paragraphs)
                        {
                            if (!string.IsNullOrWhiteSpace(paragraph.Text))
                                builder.AppendLine(paragraph.Text);
                        }
                    }
                }
            }

            return builder.ToString();
        }

        private static string? ExtractEmail(string text)
        {
            var match = Regex.Match(text, @"[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}", RegexOptions.IgnoreCase);
            return match.Success ? match.Value : null;
        }

        private static string? ExtractPhone(string text)
        {
            var match = Regex.Match(text, @"(\+?\d[\d\s().-]{7,}\d)");
            return match.Success ? match.Groups[1].Value : null;
        }

        private static string? ExtractName(List<string> lines, string? email, string? phone)
        {
            var phoneCompact = phone == null ? string.Empty : Regex.Replace(phone, @"\s+", string.Empty);
            foreach (var line in lines)
            {
                var lower = line.ToLowerInvariant();
                if (!string.IsNullOrWhiteSpace(email) && line.Contains(email, StringComparison.OrdinalIgnoreCase))
                    continue;
                if (!string.IsNullOrWhiteSpace(phoneCompact) &&
                    Regex.Replace(line, @"\s+", string.Empty).Contains(phoneCompact, StringComparison.OrdinalIgnoreCase))
                    continue;
                if (IsSectionHeading(line))
                    continue;
                if (Regex.IsMatch(lower, @"resume|curriculum|cv", RegexOptions.IgnoreCase))
                    continue;

                var words = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (words.Length >= 2 && words.Length <= 4 &&
                    words.All(word => Regex.IsMatch(word, @"^[A-Za-z.'-]+$")))
                {
                    return line;
                }
            }

            return null;
        }

        private static string? ExtractHeadline(List<string> lines, string? name, string? email, string? phone)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            var startIndex = lines.FindIndex(line => line.Equals(name, StringComparison.Ordinal));
            if (startIndex < 0)
                return null;

            var phoneCompact = phone == null ? string.Empty : Regex.Replace(phone, @"\s+", string.Empty);

            for (var i = startIndex + 1; i < lines.Count; i++)
            {
                var line = lines[i];
                var lower = line.ToLowerInvariant();
                if (!string.IsNullOrWhiteSpace(email) && line.Contains(email, StringComparison.OrdinalIgnoreCase))
                    continue;
                if (!string.IsNullOrWhiteSpace(phoneCompact) &&
                    Regex.Replace(line, @"\s+", string.Empty).Contains(phoneCompact, StringComparison.OrdinalIgnoreCase))
                    continue;
                if (IsSectionHeading(line))
                    continue;
                if (Regex.IsMatch(lower, @"resume|curriculum|cv", RegexOptions.IgnoreCase))
                    continue;
                if (line.Length > 5 && line.Length < 80)
                    return line;
            }

            return null;
        }

        private static readonly Regex[] SkillHeadings =
        {
            new Regex(@"^skills?\b", RegexOptions.IgnoreCase),
            new Regex(@"^key skills?\b", RegexOptions.IgnoreCase),
            new Regex(@"^technical skills?\b", RegexOptions.IgnoreCase),
            new Regex(@"^core competencies\b", RegexOptions.IgnoreCase),
            new Regex(@"^technologies\b", RegexOptions.IgnoreCase),
            new Regex(@"^tools\b", RegexOptions.IgnoreCase)
        };

        private static readonly Regex[] ExperienceHeadings =
        {
            new Regex(@"^experience\b", RegexOptions.IgnoreCase),
            new Regex(@"^work experience\b", RegexOptions.IgnoreCase),
            new Regex(@"^professional experience\b", RegexOptions.IgnoreCase),
            new Regex(@"^employment history\b", RegexOptions.IgnoreCase)
        };

        private static List<string> ExtractSkills(List<string> lines)
        {
            var directLine = lines.FirstOrDefault(line =>
                SkillHeadings.Any(regex => regex.IsMatch(line)) && line.Contains(':'));

            var text = string.Empty;
            if (directLine != null)
            {
                var index = directLine.IndexOf(':');
                if (index >= 0 && index + 1 < directLine.Length)
                    text = directLine[(index + 1)..];
            }
            else
            {
                var section = FindSectionLines(lines, SkillHeadings);
                if (section.Count > 0)
                    text = string.Join(' ', section);
            }

            text = text.Replace('\u2022', ',').Replace('\u00B7', ',');

            var candidates = text
                .Split(new[] { ',', ';', '|', '/' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(skill => skill.Trim())
                .Where(skill => skill.Length > 1)
                .ToList();

            if (candidates.Count == 0)
            {
                var section = FindSectionLines(lines, SkillHeadings);
                foreach (var line in section)
                {
                    candidates.AddRange(line
                        .Split(new[] { ',', ';', '|', '/' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(skill => skill.Trim()));
                }
            }

            return candidates
                .Where(skill => skill.Length > 1 && skill.Length <= 40)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static int? ExtractExperienceYears(string text)
        {
            var match = Regex.Match(text, @"(\d{1,2})\+?\s*(years|yrs)", RegexOptions.IgnoreCase);
            if (!match.Success)
                return null;

            if (int.TryParse(match.Groups[1].Value, out var years))
                return years;

            return null;
        }

        private static int? ExtractExperienceYearsFromDates(List<string> lines)
        {
            var section = FindSectionLines(lines, ExperienceHeadings);
            if (section.Count == 0)
                return null;

            var years = new List<int>();
            var hasPresent = section.Any(line =>
                line.Contains("present", StringComparison.OrdinalIgnoreCase) ||
                line.Contains("current", StringComparison.OrdinalIgnoreCase));

            foreach (var line in section)
            {
                foreach (Match match in Regex.Matches(line, @"\b(19|20)\d{2}\b"))
                {
                    if (int.TryParse(match.Value, out var year))
                        years.Add(year);
                }
            }

            if (years.Count == 0)
                return null;

            var minYear = years.Min();
            var maxYear = hasPresent ? DateTime.UtcNow.Year : years.Max();
            var diff = Math.Max(0, maxYear - minYear);
            return diff == 0 && years.Count > 0 ? 1 : diff;
        }

        private static EducationRecord? ExtractEducationRecord(List<string> lines)
        {
            var section = FindSectionLines(lines, new[]
            {
                new Regex(@"^education\b", RegexOptions.IgnoreCase)
            });

            if (section.Count == 0)
                return null;

            var line = section[0];
            var yearMatch = Regex.Match(line, @"\b(19|20)\d{2}\b");
            var graduationYear = yearMatch.Success ? yearMatch.Value : string.Empty;
            var cleaned = yearMatch.Success ? line.Replace(yearMatch.Value, string.Empty) : line;
            cleaned = cleaned.Replace('\u2022', '-').Replace('\u00B7', '-');

            var parts = cleaned
                .Split(new[] { '-', '|' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(part => part.Trim())
                .Where(part => part.Length > 0)
                .ToList();

            var school = parts.Count > 0 ? parts[0] : string.Empty;
            var degree = parts.Count > 1 ? parts[1] : school;
            var field = parts.Count > 2 ? parts[2] : string.Empty;

            return new EducationRecord
            {
                School = school,
                Degree = degree,
                Field = field,
                GraduationYear = graduationYear
            };
        }

        private static string ExtractSectionText(List<string> lines, Regex[] headings)
        {
            var section = FindSectionLines(lines, headings);
            return string.Join(' ', section).Trim();
        }

        private static List<string> FindSectionLines(List<string> lines, Regex[] headings)
        {
            var startIndex = -1;
            for (var i = 0; i < lines.Count; i++)
            {
                if (headings.Any(regex => regex.IsMatch(lines[i])))
                {
                    startIndex = i + 1;
                    break;
                }
            }

            if (startIndex == -1)
                return new List<string>();

            var endIndex = lines.Count;
            for (var i = startIndex; i < lines.Count; i++)
            {
                if (IsSectionHeading(lines[i]))
                {
                    endIndex = i;
                    break;
                }
            }

            return lines
                .Skip(startIndex)
                .Take(endIndex - startIndex)
                .Where(line => !IsSectionHeading(line))
                .ToList();
        }

        private static bool IsSectionHeading(string line)
        {
            var cleaned = line.Replace(":", string.Empty).Replace("-", string.Empty).Trim();
            return Regex.IsMatch(cleaned,
                @"^(summary|profile|objective|skills?|key skills?|technical skills?|core competencies|technologies|tools|experience|work experience|professional experience|employment history|education|projects|certifications|languages)\b",
                RegexOptions.IgnoreCase);
        }
    }
}
