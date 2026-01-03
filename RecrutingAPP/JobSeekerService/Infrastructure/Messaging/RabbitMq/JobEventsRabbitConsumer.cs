using JobSeekerService.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver.Core.Connections;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using static MongoDB.Driver.WriteConcern;
using static Shared.Contracts.Events.JobEvents;

namespace JobSeekerService.Infrastructure.Messaging.RabbitMq
{
    public class JobEventsRabbitConsumer : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private RabbitMQ.Client.IConnection? _connection;
        private IModel? _channel;
        private IConfigurationSection rabbitConfig;

        public JobEventsRabbitConsumer(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
             rabbitConfig = configuration.GetSection("Messaging:RabbitMQ");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
          
            var factory = new ConnectionFactory
            {
                HostName = rabbitConfig["Host"] ?? "localhost",
                Port = int.Parse(rabbitConfig["Port"] ?? "5672"),
                UserName = rabbitConfig["Username"] ?? "guest",
                Password = rabbitConfig["Password"] ?? "guest",
                DispatchConsumersAsync = true
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare("jobs.exchange", ExchangeType.Fanout, durable: true);
            var queue = _channel.QueueDeclare().QueueName;
            _channel.QueueBind(queue, "jobs.exchange", "");

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += HandleMessageAsync;

            _channel.BasicConsume(queue, autoAck: false, consumer);

            return Task.CompletedTask;
        }

        private async Task HandleMessageAsync(object sender, BasicDeliverEventArgs args)
        {
            var json = Encoding.UTF8.GetString(args.Body.ToArray());

            using var scope = _serviceProvider.CreateScope();
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

                _channel!.BasicAck(args.DeliveryTag, false);
            }
            catch
            {
                _channel!.BasicNack(args.DeliveryTag, false, requeue: true);
            }
        }

        public override void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
            base.Dispose();
        }
    }
}
