using GamePlatform.Jogos.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GamePlatform.Jogos.Infrastructure.Seed;

public static class DbInitializer
{
    public static async Task SeedAsync(DataContext context)
    {
        await context.Database.MigrateAsync();
    }
}