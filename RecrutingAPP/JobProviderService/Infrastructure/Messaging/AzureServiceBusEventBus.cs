using Azure.Messaging.ServiceBus;
using System.Text.Json;

namespace JobProviderService.Infrastructure.Messaging
{
    public class AzureServiceBusEventBus : IEventBus
    {
        private readonly ServiceBusSender _sender;

        public AzureServiceBusEventBus(
            ServiceBusClient client,
            IConfiguration configuration)
        {
            var topicName = configuration["Messaging:AzureServiceBus:Topic"]
                            ?? "jobs-topic";

            _sender = client.CreateSender(topicName);
        }

        public async Task PublishAsync<T>(T @event) where T : class
        {
            var json = JsonSerializer.Serialize(@event);

            var message = new ServiceBusMessage(json)
            {
                ContentType = "application/json"
            };

            await _sender.SendMessageAsync(message);
        }
    }
}
