using Microsoft.Extensions.Logging;

namespace JobProviderService.Infrastructure.Messaging
{
    public sealed class NoOpEventBus : IEventBus
    {
        private readonly ILogger<NoOpEventBus> _logger;

        public NoOpEventBus(ILogger<NoOpEventBus> logger)
        {
            _logger = logger;
        }

        public Task PublishAsync<T>(T @event) where T : class
        {
            _logger.LogWarning("Event bus disabled. Dropped event {EventType}.", typeof(T).Name);
            return Task.CompletedTask;
        }
    }
}
