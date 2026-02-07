using Azure.Messaging.ServiceBus;
using System.Text.Json;
using static Shared.Contracts.Events.JobEvents;

namespace AdminService.Infrastructure.Messaging.AzureServiceBus
{
    public class JobApplicationsServiceBusConsumer : BackgroundService
    {
        private readonly ServiceBusProcessor _processor;
        private readonly IServiceScopeFactory _scopeFactory;

        public JobApplicationsServiceBusConsumer(ServiceBusClient client, IConfiguration configuration, IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;

            var topicName = configuration["Messaging:AzureServiceBus:JobApplicationsTopic"] ?? "job-applications-topic";
            var subscriptionName = configuration["Messaging:AzureServiceBus:AdminJobApplicationsSubscription"] ?? "admin-jobapplications-subscription";

            _processor = client.CreateProcessor(topicName, subscriptionName, new ServiceBusProcessorOptions
            {
                AutoCompleteMessages = false,
                MaxConcurrentCalls = 5
            });
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _processor.ProcessMessageAsync += ProcessMessageAsync;
            _processor.ProcessErrorAsync += ProcessErrorAsync;
            await _processor.StartProcessingAsync(stoppingToken);
        }

        private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
        {
            var json = args.Message.Body.ToString();
            var eventType = GetEventType(json);

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<AdminEventHandler>();

                switch (eventType)
                {
                    case nameof(JobAppliedEvent):
                        await handler.HandleAsync(JsonSerializer.Deserialize<JobAppliedEvent>(json)!);
                        break;
                    case nameof(JobApplicationWithdrawnEvent):
                        await handler.HandleAsync(JsonSerializer.Deserialize<JobApplicationWithdrawnEvent>(json)!);
                        break;
                    case nameof(JobSeekerProfileUpsertedEvent):
                        await handler.HandleAsync(JsonSerializer.Deserialize<JobSeekerProfileUpsertedEvent>(json)!);
                        break;
                    default:
                        break;
                }

                await args.CompleteMessageAsync(args.Message);
            }
            catch
            {
                await args.AbandonMessageAsync(args.Message);
            }
        }

        private Task ProcessErrorAsync(ProcessErrorEventArgs args)
        {
            return Task.CompletedTask;
        }

        private static string? GetEventType(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                return doc.RootElement.TryGetProperty("EventType", out var prop)
                    ? prop.GetString()
                    : null;
            }
            catch
            {
                return null;
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await _processor.StopProcessingAsync(cancellationToken);
            await _processor.DisposeAsync();
        }
    }
}
