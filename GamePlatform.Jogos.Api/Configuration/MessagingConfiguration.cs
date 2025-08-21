using GamePlatform.Jogos.Domain.Interfaces.Messaging;
using GamePlatform.Jogos.Infrastructure.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GamePlatform.Jogos.Api.Configuration;

public static class MessagingConfiguration
{
    public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ServiceBusOptions>(configuration.GetSection("ServiceBus"));
        services.AddSingleton<IServiceBusPublisher, AzureServiceBusPublisher>();
        return services;
    }
}