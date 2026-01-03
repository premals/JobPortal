using Azure.Messaging.ServiceBus;
using JobProviderService.Application.EventHandlers;
using static Shared.Contracts.Events.JobEvents;

namespace JobProviderService.Infrastructure.Messaging.Azure
{
    public class JobApplicationServiceBusConsumer : BackgroundService
    {
        private readonly ServiceBusProcessor _processor;
        private readonly IServiceScopeFactory _scopeFactory;

        public JobApplicationServiceBusConsumer(
            ServiceBusClient client,
            IConfiguration configuration,
            IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;

            var topicName =
                configuration["Messaging:AzureServiceBus:JobApplicationsTopic"]
                ?? "job-applications-topic";

            var subscriptionName =
                configuration["Messaging:AzureServiceBus:JobProviderSubscription"]
                ?? "jobprovider-subscription";

            _processor = client.CreateProcessor(
                topicName,
                subscriptionName,
                new ServiceBusProcessorOptions
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
            try
            {
                var evt = args.Message.Body
                    .ToObjectFromJson<JobAppliedEvent>();

                using var scope = _scopeFactory.CreateScope();
                var handler = scope.ServiceProvider
                    .GetRequiredService<JobAppliedEventHandler>();

                await handler.HandleAsync(evt);

                // ✅ Complete only after successful DB save
                await args.CompleteMessageAsync(args.Message);
            }
            catch
            {
                // ❌ Let Azure retry; after max retries → DLQ
                await args.AbandonMessageAsync(args.Message);
            }
        }

        private Task ProcessErrorAsync(ProcessErrorEventArgs args)
        {
            // Log error (ILogger recommended)
            return Task.CompletedTask;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await _processor.StopProcessingAsync(cancellationToken);
            await _processor.DisposeAsync();
        }
    }
}
