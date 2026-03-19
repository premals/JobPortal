using JobProviderService.Application.EventHandlers;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using static Shared.Contracts.Events.JobEvents;

namespace JobProviderService.Infrastructure.Messaging.RabbitMQ
{
    public class JobApplicationConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<JobApplicationConsumer> _logger;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        private const string Exchange = "job-applications.exchange";
        private const string Queue = "jobprovider.jobapplications.queue";

        public JobApplicationConsumer(
            IServiceScopeFactory scopeFactory,
            IConfiguration configuration,
            ILogger<JobApplicationConsumer> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;

            var rabbitConfig = configuration.GetSection("Messaging:RabbitMQ");
            var factory = new ConnectionFactory
            {
                HostName = rabbitConfig["Host"] ?? "localhost",
                Port = int.Parse(rabbitConfig["Port"] ?? "5672"),
                UserName = rabbitConfig["UserName"] ?? rabbitConfig["Username"] ?? "guest",
                <secret> = rabbitConfig["<secret>"] ?? "guest",
                DispatchConsumersAsync = true
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(Exchange, ExchangeType.Fanout, true);
            _channel.QueueDeclare(Queue, true, false, false);
            _channel.QueueBind(Queue, Exchange, "");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.Received += async (_, ea) =>
            {
                try
                {
                    var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var eventType = ExtractEventType(json);
                    using var scope = _scopeFactory.CreateScope();

                    switch (eventType)
                    {
                        case nameof(JobAppliedEvent):
                        {
                            var evt = JsonSerializer.Deserialize<JobAppliedEvent>(json)!;
                            var handler = scope.ServiceProvider.GetRequiredService<JobAppliedEventHandler>();
                            await handler.HandleAsync(evt);
                            break;
                        }
                        case nameof(JobApplicationWithdrawnEvent):
                        {
                            var evt = JsonSerializer.Deserialize<JobApplicationWithdrawnEvent>(json)!;
                            var handler = scope.ServiceProvider.GetRequiredService<JobApplicationWithdrawnEventHandler>();
                            await handler.HandleAsync(evt);
                            break;
                        }
                        default:
                            _logger.LogDebug("JobApplicationConsumer ignored event type: {EventType}", eventType);
                            break;
                    }

                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "JobApplicationConsumer failed to process message");
                    _channel.BasicNack(ea.DeliveryTag, false, requeue: false);
                }
            };

            _channel.BasicConsume(
                queue: Queue,
                autoAck: false,
                consumer: consumer);

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }

        private static string? ExtractEventType(string json)
        {
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.TryGetProperty("EventType", out var eventType)
                ? eventType.GetString()
                : null;
        }
    }
}
