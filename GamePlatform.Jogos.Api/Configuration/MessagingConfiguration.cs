using Azure.Messaging.ServiceBus;
using GamePlatform.Jogos.Api.BackgroundServices;
using GamePlatform.Jogos.Domain.Interfaces.Messaging;
using GamePlatform.Jogos.Infrastructure.Messaging;
using Microsoft.Extensions.Options;

namespace GamePlatform.Jogos.Api.Configuration;

public static class MessagingConfiguration
{
    public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ServiceBusOptions>(configuration.GetSection("ServiceBus"));
        services.AddSingleton<IServiceBusPublisher, AzureServiceBusPublisher>();
        services.AddSingleton<ServiceBusClient>(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<ServiceBusOptions>>().Value;
            if (string.IsNullOrWhiteSpace(opts.ConnectionString))
                throw new InvalidOperationException("AzureServiceBus:ConnectionString não configurado.");
            return new ServiceBusClient(opts.ConnectionString);
        });
        services.AddHostedService<PaymentSuccessBackgroundService>();
        return services;
    }
}