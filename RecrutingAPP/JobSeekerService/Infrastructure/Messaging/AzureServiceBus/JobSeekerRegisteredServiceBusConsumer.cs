using Azure.Messaging.ServiceBus;
using JobSeekerService.Application.EventHandler;
using JobSeekerService.Application.Interfaces;
using static Shared.Contracts.Events.JobEvents;

namespace JobSeekerService.Infrastructure.Messaging.AzureServiceBus
{
    public class JobSeekerRegisteredServiceBusConsumer : BackgroundService
    {
        private readonly ServiceBusProcessor _processor;
        private readonly IServiceScopeFactory _scopeFactory;

        public JobSeekerRegisteredServiceBusConsumer(
            ServiceBusClient client,
            IConfiguration config,
            IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;

            _processor = client.CreateProcessor(
                config["Messaging:AzureServiceBus:IdentityTopic"],
                config["Messaging:AzureServiceBus:JobSeekerSubscription"]);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _processor.ProcessMessageAsync += async args =>
            {
                var evt = args.Message.Body
                    .ToObjectFromJson<JobSeekerRegisteredEvent>();

                using var scope = _scopeFactory.CreateScope();
                var handler = scope.ServiceProvider
                    .GetRequiredService<JobSeekerRegisteredEventHandler>();

                await handler.HandleAsync(evt);
                await args.CompleteMessageAsync(args.Message);
            };

            await _processor.StartProcessingAsync(stoppingToken);
        }
    }
}
