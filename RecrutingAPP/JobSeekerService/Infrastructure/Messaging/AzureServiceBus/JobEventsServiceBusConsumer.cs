using Azure.Messaging.ServiceBus;
using JobSeekerService.Application.EventHandler;
using JobSeekerService.Application.Interfaces;
using System.Text.Json;
using static Shared.Contracts.Events.JobEvents;

namespace JobSeekerService.Infrastructure.Messaging.AzureServiceBus
{
    public class JobEventsServiceBusConsumer : BackgroundService
    {
        private readonly ServiceBusProcessor _processor;
        private readonly IServiceProvider _provider;

        public JobEventsServiceBusConsumer(
            ServiceBusClient client,
            IServiceProvider provider)
        {
            _provider = provider;

            _processor = client.CreateProcessor(
                topicName: "jobs-topic",
                subscriptionName: "jobseeker-sub",
                new ServiceBusProcessorOptions
                {
                    MaxConcurrentCalls = 1,
                    AutoCompleteMessages = false
                });
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _processor.ProcessMessageAsync += HandleMessageAsync;
            _processor.ProcessErrorAsync += _ => Task.CompletedTask;

            await _processor.StartProcessingAsync(stoppingToken);
        }

        private async Task HandleMessageAsync(ProcessMessageEventArgs args)
        {
            var json = args.Message.Body.ToString();

            using var scope = _provider.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IJobReadRepository>();

            try
            {
                if (json.Contains(nameof(JobCreatedEvent)))
                {
                    var evt = JsonSerializer.Deserialize<JobCreatedEvent>(json)!;
                    await repo.UpsertFromEventAsync(evt);
                }
                else if (json.Contains(nameof(JobUpdatedEvent)))
                {
                    var evt = JsonSerializer.Deserialize<JobUpdatedEvent>(json)!;
                    await repo.ApplyPartialUpdateAsync(evt);
                }
                else if (json.Contains(nameof(JobFullyUpdatedEvent)))
                {
                    var evt = JsonSerializer.Deserialize<JobFullyUpdatedEvent>(json)!;
                    await repo.ReplaceFromEventAsync(evt);
                }
                else if (json.Contains(nameof(JobClosedEvent)))
                {
                    var evt = JsonSerializer.Deserialize<JobClosedEvent>(json)!;
                    await repo.MarkClosedAsync(evt.JobId);
                }
                else if (json.Contains(nameof(JobApplicationStatusUpdatedEvent)))
                {
                    var evt = JsonSerializer.Deserialize<JobApplicationStatusUpdatedEvent>(json)!;

                    var handler = scope.ServiceProvider
                        .GetRequiredService<JobApplicationStatusUpdatedEventHandler>();

                    await handler.HandleAsync(evt);
                }
                else if (json.Contains(nameof(InterviewInviteCreatedEvent)))
                {
                    var evt = JsonSerializer.Deserialize<InterviewInviteCreatedEvent>(json)!;

                    var handler = scope.ServiceProvider
                        .GetRequiredService<InterviewInviteCreatedEventHandler>();

                    await handler.HandleAsync(evt);
                }

                await args.CompleteMessageAsync(args.Message);
            }
            catch
            {
                await args.AbandonMessageAsync(args.Message);
            }
        }
    }
}
