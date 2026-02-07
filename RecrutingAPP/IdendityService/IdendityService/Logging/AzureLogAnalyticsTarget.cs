using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using NLog.Common;
using NLog.Targets;

namespace IdendityService.Logging
{
    [Target("AzureLogAnalytics")]
    public sealed class AzureLogAnalyticsTarget : AsyncTaskTarget
    {
        private static readonly HttpClient Http = new HttpClient();

        public string? WorkspaceId { get; set; }
        public string? SharedKey { get; set; }
        public string LogType { get; set; } = "IdendityService";
        public string ServiceName { get; set; } = "IdendityService";

        protected override async Task WriteAsyncTask(LogEventInfo logEvent, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(WorkspaceId) || string.IsNullOrWhiteSpace(SharedKey))
            {
                return;
            }

            try
            {
                var payload = new
                {
                    TimeGenerated = logEvent.TimeStamp.ToUniversalTime(),
                    Service = ServiceName,
                    Level = logEvent.Level.Name,
                    Logger = logEvent.LoggerName,
                    Message = logEvent.FormattedMessage,
                    Exception = logEvent.Exception?.ToString(),
                    TraceId = MappedDiagnosticsLogicalContext.Get("traceId") ?? string.Empty,
                    RequestId = MappedDiagnosticsLogicalContext.Get("requestId") ?? string.Empty
                };

                var json = JsonSerializer.Serialize(new[] { payload });
                var date = DateTime.UtcNow.ToString("r");
                var signature = BuildSignature(date, json.Length);

                using var request = new HttpRequestMessage(HttpMethod.Post,
                    $"https://{WorkspaceId}.ods.opinsights.azure.com/api/logs?api-version=2016-04-01");
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                request.Headers.Add("Log-Type", LogType);
                request.Headers.Add("x-ms-date", date);
                request.Headers.Add("Authorization", $"SharedKey {WorkspaceId}:{signature}");
                request.Headers.Add("time-generated-field", "TimeGenerated");

                using var response = await Http.SendAsync(request, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                InternalLogger.Warn(ex, "Failed to send log to Azure Log Analytics.");
            }
        }

        private string BuildSignature(string date, int contentLength)
        {
            var stringToSign = $"POST\n{contentLength}\napplication/json\nx-ms-date:{date}\n/api/logs";
            var keyBytes = Convert.FromBase64String(SharedKey!);
            using var hasher = new HMACSHA256(keyBytes);
            var hash = hasher.ComputeHash(Encoding.UTF8.GetBytes(stringToSign));
            return Convert.ToBase64String(hash);
        }
    }
}
