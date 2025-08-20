using GamePlatform.Jogos.Application.Interfaces.Services;
using GamePlatform.Jogos.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GamePlatform.Jogos.Application.Configuration;

public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IJogoService, JogoService>();
        services.AddScoped<IUsuarioContextService, UsuarioContextService>();

        return services;
    }
}