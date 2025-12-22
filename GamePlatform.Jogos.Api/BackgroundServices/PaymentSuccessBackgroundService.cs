using System.Text.Json;
using Azure.Messaging.ServiceBus;
using GamePlatform.Jogos.Application.DTOs.Messaging;
using GamePlatform.Jogos.Application.Interfaces.Services;
using GamePlatform.Jogos.Infrastructure.Messaging;
using Microsoft.Extensions.Options;

namespace GamePlatform.Jogos.Api.BackgroundServices;

public class PaymentSuccessBackgroundService : BackgroundService
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusOptions _options;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PaymentSuccessBackgroundService> _logger;
    private ServiceBusProcessor? _processor;
    
    public PaymentSuccessBackgroundService(
        ServiceBusClient client,
        IOptions<ServiceBusOptions> options,
        IServiceScopeFactory scopeFactory,
        ILogger<PaymentSuccessBackgroundService> logger)
    {
        _client = client;
        _options = options.Value;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (string.IsNullOrWhiteSpace(_options.PaymentSuccessQueue))
        {
            _logger.LogError("AzureServiceBus QueueName não configurado.");
            return;
        }

        _logger.LogInformation(
        "Inicializando PaymentSuccessBackgroundService. Queue={Queue}, MaxConcurrentCalls={MaxConcurrentCalls}, Prefetch={Prefetch}",
        _options.PaymentSuccessQueue,
        _options.MaxConcurrentCalls,
        _options.PrefetchCount);

        var processorOptions = new ServiceBusProcessorOptions
        {
            MaxConcurrentCalls = _options.MaxConcurrentCalls,
            AutoCompleteMessages = false,
            PrefetchCount = _options.PrefetchCount
        };

        _processor = _client.CreateProcessor(_options.PaymentSuccessQueue, processorOptions);
        _processor.ProcessMessageAsync += OnMessageAsync;
        _processor.ProcessErrorAsync += OnErrorAsync;

        _logger.LogInformation("Iniciando o Service Bus Processor para a fila {Queue}", _options.PaymentSuccessQueue);
        await _processor.StartProcessingAsync(stoppingToken);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("CancellationToken acionado. Encerrando PaymentSuccessBackgroundService.");
        }
        finally
        {
            _logger.LogInformation("Parando o Service Bus Processor da fila {Queue}", _options.PaymentSuccessQueue);

            if (_processor is not null)
            {
                try
                {
                    await _processor.StopProcessingAsync(CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao parar o Service Bus Processor.");
                }

                _processor.ProcessMessageAsync -= OnMessageAsync;
                _processor.ProcessErrorAsync -= OnErrorAsync;

                await _processor.DisposeAsync();
                _logger.LogInformation("Service Bus Processor descartado.");
            }
        }
    }
    
    private async Task OnMessageAsync(ProcessMessageEventArgs args)
    {
        var messageId = args.Message.MessageId;
        var correlationId = args.Message.CorrelationId;

        _logger.LogInformation(
            "Mensagem recebida. MessageId={MessageId}, CorrelationId={CorrelationId}",
            messageId,
            correlationId);

        try
        {
            var body = args.Message.Body.ToString();
            var message = JsonSerializer.Deserialize<PaymentSuccessMessage>(body, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (message is null)
            {
                _logger.LogWarning("Falha ao desserializar mensagem. Abandonando MessageId={MessageId}", args.Message.MessageId);
                await args.AbandonMessageAsync(args.Message);
                return;
            }

            using var scope = _scopeFactory.CreateScope();
            var jogoService = scope.ServiceProvider.GetRequiredService<IJogoService>();

            _logger.LogInformation(
             "Processando pagamento com sucesso para UsuarioId={UsuarioId}, JogoId={JogoId}, MessageId={MessageId}",
             message.UsuarioId,
             message.JogoId,
             messageId);

            await jogoService.AdicionaJogoUsuarioAsync(message);

            await args.CompleteMessageAsync(args.Message);

            _logger.LogInformation(
            "Mensagem processada com sucesso. MessageId={MessageId}",
            messageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar a mensagem (MessageId={MessageId}). Abandonando.", args.Message.MessageId);
           
            try 
            { 
                await args.AbandonMessageAsync(args.Message); 
            }
            catch (Exception abandonEx)
            {
                _logger.LogCritical(
                    abandonEx,
                    "Falha crítica ao abandonar mensagem. MessageId={MessageId}",
                    messageId);
            }
        }
    }

    private Task OnErrorAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError(
            args.Exception,
            "Erro no Service Bus Processor. Entity={Entity}, Namespace={Namespace}, ErrorSource={ErrorSource}",
            args.EntityPath,
            args.FullyQualifiedNamespace,
            args.ErrorSource); 
        
        return Task.CompletedTask;
    }
}