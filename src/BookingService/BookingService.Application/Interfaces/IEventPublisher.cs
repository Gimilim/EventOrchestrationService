namespace BookingService.Application.Interfaces;

public interface IEventPublisher : IDisposable
{
    Task PublishAsync<T>(string topic, T message, string? key = null, CancellationToken cancellationToken = default);
}