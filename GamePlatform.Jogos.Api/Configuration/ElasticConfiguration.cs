using GamePlatform.Jogos.Application.Elastic;
using GamePlatform.Jogos.Application.Interfaces.Elastic;
using GamePlatform.Jogos.Domain.Interfaces.Elastic;
using GamePlatform.Jogos.Infrastructure.Elastic;

namespace GamePlatform.Jogos.Api.Configuration;

public static class ElasticConfiguration
{
    public static IServiceCollection AddElasticsearch(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ElasticSettings>(configuration.GetSection("ElasticSettings"));
        services.AddScoped(typeof(IElasticClient<>), typeof(ElasticClient<>));
        services.AddScoped<IJogoElasticClient, JogoElasticClient>();

        return services;
    }
}