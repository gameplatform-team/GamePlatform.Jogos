using System.Text.Json.Serialization;
using GamePlatform.Jogos.Api.Configuration;
using GamePlatform.Jogos.Api.Middlewares;
using GamePlatform.Jogos.Application.Configuration;
using GamePlatform.Jogos.Infrastructure.Data;
using GamePlatform.Jogos.Infrastructure.Seed;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddCustomHttpLogging();

// JWT
builder.Services.AddJwtAuthentication(builder.Configuration);

// Health check
builder.AddCustomHealthCheck();

// Adicionar configuracoes do banco de dados e servicos da infraestrutura
builder.Services.AddInfrastructureServices(builder.Configuration);

// Adicionar servicos da camada de aplica��o
builder.Services.AddApplicationServices();

// Controllers e Swagger
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerWithOptions();

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Middleware de log
app.UseHttpLogging();

// Chamar o Seed
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<DataContext>();

    await DbInitializer.SeedAsync(context);
}

// Middleware do Swagger
app.UseSwagger();
app.UseSwaggerUI();

// Health check
app.UseHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
}).UseHealthChecksUI(options =>
{
    options.UIPath = "/health-ui";
});

// Middlewares HTTP
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ErrorHandlingMiddleware>();

app.MapControllers();

app.Run();
