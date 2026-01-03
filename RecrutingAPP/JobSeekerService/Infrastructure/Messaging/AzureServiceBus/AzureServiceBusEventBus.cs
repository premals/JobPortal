using Azure.Messaging.ServiceBus;
using System.Text.Json;

namespace JobSeekerService.Infrastructure.Messaging.AzureServiceBus
{
    using System.Text.Json;
    using Azure.Messaging.ServiceBus;

    public class AzureServiceBusEventBus : IEventBus
    {
        private readonly ServiceBusSender _sender;

        public AzureServiceBusEventBus(
            ServiceBusClient client,
            IConfiguration configuration)
        {
            // 🔹 Dedicated topic for job application events
            var topicName =
                configuration["Messaging:AzureServiceBus:JobApplicationsTopic"]
                ?? "job-applications-topic";

            _sender = client.CreateSender(topicName);
        }

        public async Task PublishAsync<T>(T @event) where T : class
        {
            var json = JsonSerializer.Serialize(@event);

            var message = new ServiceBusMessage(json)
            {
                ContentType = "application/json",
                Subject = typeof(T).Name,
                MessageId = Guid.NewGuid().ToString()
            };

            await _sender.SendMessageAsync(message);
        }
    }

}
