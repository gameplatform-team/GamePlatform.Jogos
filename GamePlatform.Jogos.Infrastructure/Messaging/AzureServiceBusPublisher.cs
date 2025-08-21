using System.Text.Json;
using Azure.Messaging.ServiceBus;
using GamePlatform.Jogos.Domain.Interfaces.Messaging;
using Microsoft.Extensions.Options;

namespace GamePlatform.Jogos.Infrastructure.Messaging;

public class AzureServiceBusPublisher : IServiceBusPublisher, IAsyncDisposable
{
    private readonly ServiceBusClient _client;

    public AzureServiceBusPublisher(IOptions<ServiceBusOptions> options)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));
        var connectionString = options.Value.ConnectionString;
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string do Service Bus não está configurada.", nameof(options));

        _client = new ServiceBusClient(connectionString);
    }

    public async Task PublishAsync<T>(string queueName, T message, string? messageId = null, string? correlationId = null, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(queueName))
            throw new ArgumentException("O nome da Queue deve ser informado", nameof(queueName));

        var body = JsonSerializer.SerializeToUtf8Bytes(message, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });

        var sbMessage = new ServiceBusMessage(new BinaryData(body));

        if (!string.IsNullOrWhiteSpace(messageId))
            sbMessage.MessageId = messageId;
        if (!string.IsNullOrWhiteSpace(correlationId))
            sbMessage.CorrelationId = correlationId;

        var sender = _client.CreateSender(queueName);
        try
        {
            await sender.SendMessageAsync(sbMessage, ct);
        }
        finally
        {
            await sender.DisposeAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _client.DisposeAsync();
    }
}