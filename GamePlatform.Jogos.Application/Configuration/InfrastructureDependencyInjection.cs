using GamePlatform.Jogos.Domain.Interfaces;
using GamePlatform.Jogos.Infrastructure.Data;
using GamePlatform.Jogos.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GamePlatform.Jogos.Application.Configuration;

public static class InfrastructureDependencyInjection
{
    public static void AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<DataContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IJogoRepository, JogoRepository>();
        services.AddScoped<IUsuarioJogosRepository, UsuarioJogosRepository>();
    }
}