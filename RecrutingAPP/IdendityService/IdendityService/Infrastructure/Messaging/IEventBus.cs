namespace IdendityService.Infrastructure.Messaging
{
    public interface IEventBus
    {
        Task PublishAsync<T>(T @event) where T : class;
    }
}
