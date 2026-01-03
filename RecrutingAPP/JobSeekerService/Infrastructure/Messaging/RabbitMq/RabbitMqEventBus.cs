using Microsoft.Azure.Amqp;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace JobSeekerService.Infrastructure.Messaging.RabbitMq
{
    public class RabbitMqEventBus : IEventBus, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _exchangeName;

        public RabbitMqEventBus(IConfiguration configuration)
        {
            var rabbitConfig = configuration.GetSection("Messaging:RabbitMQ");

            // 🔹 Dedicated exchange for JobApplication events
            _exchangeName =
                rabbitConfig["JobApplicationsExchange"]
                ?? "job-applications.exchange";

            var factory = new ConnectionFactory
            {
                HostName = rabbitConfig["Host"] ?? "localhost",
                Port = int.Parse(rabbitConfig["Port"] ?? "5672"),
                UserName = rabbitConfig["Username"] ?? "guest",
                Password = rabbitConfig["Password"] ?? "guest"
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // ✅ Exchange only (publisher responsibility)
            _channel.ExchangeDeclare(
                exchange: _exchangeName,
                type: ExchangeType.Fanout,
                durable: true,
                autoDelete: false);
        }

        public Task PublishAsync<T>(T @event) where T : class
        {
            var message = JsonSerializer.Serialize(@event);
            var body = Encoding.UTF8.GetBytes(message);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;

            _channel.BasicPublish(
                exchange: _exchangeName,
                routingKey: string.Empty,
                basicProperties: properties,
                body: body);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
        }
    }
}
