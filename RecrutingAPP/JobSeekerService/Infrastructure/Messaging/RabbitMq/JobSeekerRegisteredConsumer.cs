using JobSeekerService.Application.EventHandler;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using static Shared.Contracts.Events.JobEvents;

namespace JobSeekerService.Infrastructure.Messaging.RabbitMq
{
    public class JobSeekerRegisteredConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private IModel _channel;

        private const string Exchange = "identity.exchange";
        private const string Queue = "jobseeker.identity.queue";

        public JobSeekerRegisteredConsumer(
            IServiceScopeFactory scopeFactory,
            IConfiguration rabbitConfig)
        {
            _scopeFactory = scopeFactory;

            var factory = new ConnectionFactory
            {
                HostName = rabbitConfig["Host"] ?? "localhost",
                Port = int.Parse(rabbitConfig["Port"] ?? "5672"),
                UserName = "guest",
                <secret> = "guest",
                DispatchConsumersAsync = true
            };

            var connection = factory.CreateConnection();
            _channel = connection.CreateModel();

            _channel.ExchangeDeclare(Exchange, ExchangeType.Fanout, true);
            _channel.QueueDeclare(Queue, true, false, false);
            _channel.QueueBind(Queue, Exchange, "");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.Received += async (_, ea) =>
            {
                var evt = JsonSerializer.Deserialize<JobSeekerRegisteredEvent>(
                    Encoding.UTF8.GetString(ea.Body.ToArray()))!;

                using var scope = _scopeFactory.CreateScope();
                var handler = scope.ServiceProvider
                    .GetRequiredService<JobSeekerRegisteredEventHandler>();

                await handler.HandleAsync(evt);

                _channel.BasicAck(ea.DeliveryTag, false);
            };

            _channel.BasicConsume(Queue, false, consumer);
            return Task.CompletedTask;
        }
    }
}
