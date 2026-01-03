using Azure.Messaging.ServiceBus;
using System.Text.Json;

namespace IdendityService.Infrastructure.Messaging.AzureServiceBus
{
    public class AzureServiceBusEventBus : IEventBus, IAsyncDisposable
    {
        private readonly ServiceBusSender _sender;

        public AzureServiceBusEventBus(
            ServiceBusClient client,
            IConfiguration configuration)
        {
            // ✅ Topic name MUST match consumer configuration
            var topicName =
                configuration["Messaging:AzureServiceBus:IdentityTopic"]
                ?? "identity-topic";

            _sender = client.CreateSender(topicName);
        }

        // ============================
        // Publish JobSeekerRegisteredEvent
        // ============================
        public async Task PublishAsync<T>(T @event) where T : class
        {
            var json = JsonSerializer.Serialize(@event);

            var message = new ServiceBusMessage(json)
            {
                ContentType = "application/json",
                Subject = typeof(T).Name, // useful for filters
                MessageId = Guid.NewGuid().ToString()
            };

            await _sender.SendMessageAsync(message);
        }

        public async ValueTask DisposeAsync()
        {
            await _sender.DisposeAsync();
        }
    }
}
