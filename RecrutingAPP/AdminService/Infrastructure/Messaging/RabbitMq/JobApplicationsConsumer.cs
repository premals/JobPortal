using AdminService.Infrastructure.Messaging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using static Shared.Contracts.Events.JobEvents;

namespace AdminService.Infrastructure.Messaging.RabbitMq
{
    public class JobApplicationsConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IModel _channel;

        private const string Exchange = "job-applications.exchange";
        private const string Queue = "admin.jobapplications.queue";

        public JobApplicationsConsumer(IServiceScopeFactory scopeFactory, IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            var rabbitConfig = configuration.GetSection("Messaging:RabbitMQ");

            var factory = new ConnectionFactory
            {
                HostName = rabbitConfig["Host"] ?? "localhost",
                Port = int.Parse(rabbitConfig["Port"] ?? "5672"),
                UserName = rabbitConfig["Username"] ?? "guest",
                <secret> = rabbitConfig["<secret>"] ?? "guest",
                DispatchConsumersAsync = true
            };

            var connection = factory.CreateConnection();
            _channel = connection.CreateModel();

            _channel.ExchangeDeclare(Exchange, ExchangeType.Fanout, durable: true);
            _channel.QueueDeclare(Queue, durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind(Queue, Exchange, string.Empty);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += HandleMessageAsync;

            _channel.BasicConsume(Queue, autoAck: false, consumer: consumer);
            return Task.CompletedTask;
        }

        private async Task HandleMessageAsync(object sender, BasicDeliverEventArgs args)
        {
            var json = Encoding.UTF8.GetString(args.Body.ToArray());
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

                _channel.BasicAck(args.DeliveryTag, false);
            }
            catch
            {
                _channel.BasicNack(args.DeliveryTag, false, requeue: true);
            }
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
    }
}
