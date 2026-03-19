using JobProviderService.Application.Interfaces;
using System.Net.Http.Json;

namespace JobProviderService.Infrastructure.Services
{
    public class HttpEmailService : IEmailService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public HttpEmailService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task SendAsync(string to, string subject, string html)
        {
            if (_httpClient.BaseAddress == null)
            {
                var baseUrl = _configuration["EmailService:BaseUrl"];
                if (string.IsNullOrWhiteSpace(baseUrl))
                    throw new InvalidOperationException("EmailService:BaseUrl is required.");

                _httpClient.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
            }

            var payload = new
            {
                to,
                subject,
                html
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, "api/email/send")
            {
                Content = JsonContent.Create(payload)
            };

            var apiKey = _configuration["EmailService:ApiKey"];
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                request.Headers.Add("X-Email-Api-Key", apiKey);
            }

            using var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }
    }
}
