using RabbitMQ.Client;
using System.Text;
using System.Text.Json;


namespace IdendityService.Infrastructure.Messaging.RabbitMq
{
    public class RabbitMqEventBus : IEventBus, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private const string Exchange = "identity.exchange";

        public RabbitMqEventBus(IConfiguration configuration)
        {
            var rabbitConfig = configuration.GetSection("Messaging:RabbitMQ");

            var factory = new ConnectionFactory
            {
                HostName = rabbitConfig["Host"] ?? "localhost",
                Port = int.Parse(rabbitConfig["Port"] ?? "5672"),
                UserName = rabbitConfig["Username"] ?? "guest",
                Password = rabbitConfig["Password"] ?? "guest"
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // ✅ Publisher declares ONLY exchange
            _channel.ExchangeDeclare(
                exchange: Exchange,
                type: ExchangeType.Fanout,
                durable: true,
                autoDelete: false);
        }

        // ============================
        // Publish JobSeekerRegisteredEvent
        // ============================
        public Task PublishAsync<T>(T @event) where T : class
        {
            var json = JsonSerializer.Serialize(@event);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;

            _channel.BasicPublish(
                exchange: Exchange,
                routingKey: string.Empty, // fanout
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
