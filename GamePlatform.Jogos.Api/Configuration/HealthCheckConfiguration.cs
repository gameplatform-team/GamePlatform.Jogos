using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace GamePlatform.Jogos.Api.Configuration;

public static class HealthCheckConfiguration
{
    public static void AddCustomHealthCheck(this WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

        builder.Services.AddHealthChecks().AddNpgSql(
            connectionString!,
            name: "PostgreSQL Games Database",
            failureStatus: HealthStatus.Unhealthy,
            tags: ["db", "sql", "postgres"]);
        
        builder.Services.AddHealthChecksUI(settings =>
        {
            settings.SetEvaluationTimeInSeconds(10);
            settings.MaximumHistoryEntriesPerEndpoint(100);
            settings.SetApiMaxActiveRequests(1);
            settings.AddHealthCheckEndpoint("Games API Health", "http://localhost:8081/health");
        }).AddInMemoryStorage();
    }
}