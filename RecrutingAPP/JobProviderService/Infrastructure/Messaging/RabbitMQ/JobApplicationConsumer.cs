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
        private IModel _channel;

        private const string Exchange = "job-applications.exchange";
        private const string Queue = "jobprovider.jobapplications.queue";

        public JobApplicationConsumer(
            IServiceScopeFactory scopeFactory,
            IConfiguration rabbitConfig)
        {
            _scopeFactory = scopeFactory;

            var factory = new ConnectionFactory
            {
                HostName = rabbitConfig["Host"] ?? "localhost",
                Port = int.Parse(rabbitConfig["Port"] ?? "5672"),
                UserName =  "guest",
                Password =  "guest",
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
                try
                {
                    var json = Encoding.UTF8.GetString(ea.Body.ToArray());

                    using var scope = _scopeFactory.CreateScope();

                    // 🔹 Route by EventType
                    var baseEvent = JsonSerializer.Deserialize<BaseEvent>(json);

                    switch (baseEvent?.EventType)
                    {
                        case nameof(JobAppliedEvent):
                            {
                                var evt = JsonSerializer.Deserialize<JobAppliedEvent>(json)!;

                                var handler = scope.ServiceProvider
                                    .GetRequiredService<JobAppliedEventHandler>();

                                await handler.HandleAsync(evt);
                                break;
                            }

                        case nameof(JobApplicationWithdrawnEvent):
                            {
                                var evt = JsonSerializer.Deserialize<JobApplicationWithdrawnEvent>(json)!;

                                var handler = scope.ServiceProvider
                                    .GetRequiredService<JobApplicationWithdrawnEventHandler>();

                                await handler.HandleAsync(evt);
                                break;
                            }

                        default:
                            // Unknown event → ignore safely
                            break;
                    }

                    // ✅ ACK only after successful processing
                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception)
                {
                    // ❌ Requeue message on failure
                    _channel.BasicNack(ea.DeliveryTag, false, requeue: true);
                }
            };

            _channel.BasicConsume(
                queue: Queue,
                autoAck: false,
                consumer: consumer);

            return Task.CompletedTask;
        }
    }
}
