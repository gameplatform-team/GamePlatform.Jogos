namespace GamePlatform.Jogos.Infrastructure.Messaging;

public class ServiceBusOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public string GamePurchaseRequestedQueue { get; set; } = "game-purchase-requested";
    public int MaxConcurrentCalls { get; set; } = 1;
    public int PrefetchCount { get; set; } = 0;
}