namespace GamePlatform.Jogos.Domain.Interfaces.Messaging;

public interface IServiceBusPublisher
{
    public Task PublishAsync<T>(string queueName, T message, string? messageId = null, string? correlationId = null, CancellationToken ct = default);
}