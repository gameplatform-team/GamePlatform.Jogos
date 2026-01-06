using GamePlatform.Jogos.Application.DTOs;
using GamePlatform.Jogos.Application.DTOs.Jogo;
using GamePlatform.Jogos.Application.DTOs.Messaging;
using GamePlatform.Jogos.Application.Interfaces.Elastic;
using GamePlatform.Jogos.Application.Interfaces.Services;
using GamePlatform.Jogos.Domain.Entities;
using GamePlatform.Jogos.Domain.Interfaces;
using GamePlatform.Jogos.Domain.Interfaces.Messaging;
using Microsoft.Extensions.Logging;

namespace GamePlatform.Jogos.Application.Services;

public class JogoService : IJogoService
{
    private const string GamePurchaseRequestedQueue = "game-purchase-requested";
    
    private readonly IJogoRepository _jogoRepository;
    private readonly IUsuarioJogosRepository _usuarioJogosRepository;
    private readonly IServiceBusPublisher _publisher;
    private readonly IJogoElasticClient _elasticClient;
    private readonly ILogger<JogoService> _logger;

    public JogoService(
        IJogoRepository jogoRepository,
        IUsuarioJogosRepository usuarioJogosRepository,
        IServiceBusPublisher publisher,
        IJogoElasticClient elasticClient,
        ILogger<JogoService> logger)
    {
        _jogoRepository = jogoRepository;
        _usuarioJogosRepository = usuarioJogosRepository;
        _publisher = publisher;
        _elasticClient = elasticClient;
        _logger = logger;
    }

    public async Task<BaseResponseDto> CadastrarAsync(CadastrarJogoDto jogoDto)
    {
        _logger.LogInformation("Iniciando cadastro de jogo com título: {Titulo}", jogoDto.Titulo);

        if (await _jogoRepository.ExisteTituloAsync(jogoDto.Titulo))
        {
            _logger.LogWarning("Tentativa de cadastrar jogo já existente: {Titulo}", jogoDto.Titulo);
            return new BaseResponseDto(false, "Jogo já cadastrado");
        }

        var jogo = new Jogo(jogoDto.Titulo, jogoDto.Preco, jogoDto.Descricao, jogoDto.Categoria);
        
        await _jogoRepository.AdicionarAsync(jogo);
        _logger.LogInformation("Jogo persistido no banco com Id: {JogoId}", jogo.Id);

        await _elasticClient.AdicionarAsync(jogo);
        _logger.LogInformation("Jogo indexado no ElasticSearch: {JogoId}", jogo.Id);

        _logger.LogInformation("Jogo cadastrado com sucesso: {JogoId} - {Titulo}", jogo.Id, jogoDto.Titulo);
        return new BaseResponseDto(true, "Jogo cadastrado com sucesso");
    }

    public async Task<BaseResponseDto> ObterPorIdAsync(Guid id)
    {
        _logger.LogInformation("Buscando jogo por Id: {JogoId}", id);

        var jogo = await _jogoRepository.ObterPorIdAsync(id);

        if (jogo == null)
        {
            _logger.LogWarning("Jogo não encontrado: {JogoId}", id);
            return new BaseResponseDto(false, "Jogo não encontrado");
        }

        var jogoDto = new JogoDto
        {
            Id = jogo.Id,
            Titulo = jogo.Titulo,
            Preco = jogo.Preco,
            Descricao = jogo.Descricao,
            Categoria = jogo.Categoria
        };

        _logger.LogInformation("Jogo encontrado e retornado: {JogoId} - {Titulo}", id, jogo.Titulo);
        return new DataResponseDto<JogoDto>(true, string.Empty, jogoDto);
    }

    public async Task<ResultadoPaginadoDto<JogoDto>> ObterTodosAsync(
        string? titulo = null,
        decimal? precoMinimo = null,
        decimal? precoMaximo = null,
        int numeroPagina = 1,
        int tamanhoPagina = 10)
    {
        _logger.LogInformation("Busca paginada de jogos - Página: {Pagina}, Tamanho: {Tamanho}, Título: {Titulo}, Preço Mín: {PrecoMin}, Preço Máx: {PrecoMax}",
            numeroPagina, tamanhoPagina, titulo, precoMinimo, precoMaximo);

        var (jogos, total) = await _elasticClient.ObterTodosAsync(
            numeroPagina,
            tamanhoPagina,
            titulo,
            precoMinimo.HasValue ? Convert.ToDouble(precoMinimo) : null,
            precoMaximo.HasValue ? Convert.ToDouble(precoMaximo) : null);
        
        var result = new ResultadoPaginadoDto<JogoDto>()
        {
            Itens = jogos.Select(jogo => new JogoDto
            {
                Id = Guid.Parse(jogo.Id),
                Titulo = jogo.Titulo,
                Preco = jogo.Preco,
                Descricao = jogo.Descricao,
                Categoria = jogo.Categoria
            }),
            NumeroPagina = numeroPagina,
            TamanhoPagina = tamanhoPagina,
            TotalDeItens = total
        };

        _logger.LogInformation("Retornados {Quantidade} jogos de um total de {Total}", result.Itens.Count(), total);
        return result;
    }

    public async Task<BaseResponseDto> AtualizarAsync(AtualizarJogoDto jogoDto)
    {
        _logger.LogInformation("Iniciando atualização do jogo Id: {JogoId}", jogoDto.Id);

        var jogo = await _jogoRepository.ObterPorIdAsync(jogoDto.Id);

        if (jogo == null)
        {
            _logger.LogWarning("Tentativa de atualizar jogo inexistente: {JogoId}", jogoDto.Id);
            return new BaseResponseDto(false, "Jogo não encontrado");
        }

        var jogosComMesmoTitulo = await _jogoRepository.ObterTodosAsync(
            j => j.Titulo.ToLower() == jogoDto.Titulo.ToLower() && j.Id != jogoDto.Id);

        if (jogosComMesmoTitulo.Any())
        {
            _logger.LogWarning("Conflito de título único ao atualizar jogo {JogoId}. Título já usado por outro jogo: {Titulo}", jogoDto.Id, jogoDto.Titulo);
            return new BaseResponseDto(false, "Já existe outro jogo com este título");
        }

        jogo.Atualizar(jogoDto.Titulo, jogoDto.Preco, jogoDto.Descricao, jogoDto.Categoria);

        await _jogoRepository.AtualizarAsync(jogo);
        _logger.LogInformation("Jogo atualizado no banco: {JogoId}", jogo.Id);

        await _elasticClient.AtualizarAsync(jogo);
        _logger.LogInformation("Jogo atualizado no ElasticSearch: {JogoId}", jogo.Id);

        _logger.LogInformation("Jogo atualizado com sucesso: {JogoId} - {Titulo}", jogo.Id, jogoDto.Titulo);
        return new BaseResponseDto(true, "Jogo atualizado com sucesso");
    }

    public async Task<BaseResponseDto> RemoverAsync(Guid id)
    {
        _logger.LogInformation("Iniciando remoção do jogo Id: {JogoId}", id);

        var jogo = await _jogoRepository.ObterPorIdAsync(id);

        if (jogo == null)
        {
            _logger.LogWarning("Tentativa de remover jogo inexistente: {JogoId}", id);
            return new BaseResponseDto(false, "Jogo não encontrado");
        }

        await _jogoRepository.RemoverAsync(jogo);
        _logger.LogInformation("Jogo removido do banco: {JogoId}", id);

        await _elasticClient.RemoverAsync(jogo.Id);
        _logger.LogInformation("Jogo removido do ElasticSearch: {JogoId}", id);

        _logger.LogInformation("Jogo removido com sucesso: {JogoId}", id);
        return new BaseResponseDto(true, "Jogo removido com sucesso");
    }

    public async Task<BaseResponseDto> ComprarJogoAsync(Guid usuarioId, ComprarJogoDto comprarJogoDto)
    {
        _logger.LogInformation("Usuário {UsuarioId} iniciando compra do jogo {JogoId}", usuarioId, comprarJogoDto.JogoId);

        var jogo = await _jogoRepository.ObterPorIdAsync(comprarJogoDto.JogoId);
        if (jogo == null)
        {
            _logger.LogWarning("Jogo não encontrado ao tentar comprar: {JogoId}", comprarJogoDto.JogoId);
            return new BaseResponseDto(false, "Jogo não encontrado");
        }

        var usuarioJogo = await _usuarioJogosRepository.ExisteUsuarioJogoAsync(usuarioId, comprarJogoDto.JogoId);
        if (usuarioJogo)
        {
            _logger.LogWarning("Usuário {UsuarioId} já possui o jogo {JogoId}", usuarioId, comprarJogoDto.JogoId);
            return new BaseResponseDto(false, "Usuário já possui este jogo");
        }

        var message = new GamePurchaseRequestedMessage
        {
            UsuarioId = usuarioId,
            JogoId = comprarJogoDto.JogoId,
            Preco = jogo.Preco,
            SolicitadoEm = DateTime.UtcNow
        };

        var messageId = Guid.NewGuid().ToString();
        await _publisher.PublishAsync(
            queueName: GamePurchaseRequestedQueue,
            message: message,
            messageId: messageId,
            correlationId: usuarioId.ToString(),
            ct: CancellationToken.None
        );

        _logger.LogInformation("Mensagem de solicitação de compra publicada na fila {GamePurchaseRequestedQueue}. MessageId: {MessageId}, Usuario: {UsuarioId}, Jogo: {JogoId}",
            GamePurchaseRequestedQueue, messageId, usuarioId, comprarJogoDto.JogoId);

        var responseDto = new ComprarJogoResponseDto("Compra iniciada. Aguarde confirmação.", "Pendente", comprarJogoDto.JogoId);
        return new DataResponseDto<ComprarJogoResponseDto>(true, string.Empty, responseDto);
    }

    public async Task<BaseResponseDto> ObterJogosDoUsuarioAsync(Guid usuarioId)
    {
        _logger.LogInformation("Obtendo jogos comprados do usuário: {UsuarioId}", usuarioId);

        var usuarioJogos = await _usuarioJogosRepository.ObterJogosDoUsuarioAsync(usuarioId);
        
        var jogosDto = usuarioJogos.Select(uj => new MeuJogoDto
        {
            Id = uj.JogoId,
            Titulo = uj.Jogo.Titulo,
            Descricao = uj.Jogo.Descricao,
            Categoria = uj.Jogo.Categoria,
            CompradoEm = uj.CompradoEm
        }).ToList();
        
        var mensagem = jogosDto.Count == 0 ? "Nenhum jogo comprado" : string.Empty;

        _logger.LogInformation("Usuário {UsuarioId} possui {Quantidade} jogo(s)", usuarioId, jogosDto.Count);

        return new DataResponseDto<List<MeuJogoDto>>(true, mensagem, jogosDto);
    }

    public async Task AdicionaJogoUsuarioAsync(PaymentSuccessMessage message)
    {
        _logger.LogInformation("Processando pagamento confirmado - Usuário: {UsuarioId}, Jogo: {JogoId}", message.UsuarioId, message.JogoId);

        var usuarioJogo = new UsuarioJogo(message.UsuarioId, message.JogoId);
        await _usuarioJogosRepository.AdicionarAsync(usuarioJogo);
        _logger.LogInformation("Relação usuário-jogo persistida: {UsuarioId} - {JogoId}", message.UsuarioId, message.JogoId);

        await _elasticClient.IncrementarPopularidadeAsync(message.JogoId);
        _logger.LogInformation("Popularidade incrementada no ElasticSearch para o jogo: {JogoId}", message.JogoId);
    }

    public async Task<ResultadoPaginadoDto<JogoDto>> ObterJogosPorPopularidadeAsync(int numeroPagina = 1, int tamanhoPagina = 10)
    {
        _logger.LogInformation("Buscando jogos por popularidade - Página: {Pagina}, Tamanho: {Tamanho}", numeroPagina, tamanhoPagina);

        var (jogos, total) = await _elasticClient.ObterTodosPorPopularidadeAsync(
            numeroPagina,
            tamanhoPagina);
        
        var result = new ResultadoPaginadoDto<JogoDto>()
        {
            Itens = jogos.Select(jogo => new JogoDto
            {
                Id = Guid.Parse(jogo.Id),
                Titulo = jogo.Titulo,
                Preco = jogo.Preco,
                Descricao = jogo.Descricao,
                Categoria = jogo.Categoria
            }),
            NumeroPagina = numeroPagina,
            TamanhoPagina = tamanhoPagina,
            TotalDeItens = total
        };

        _logger.LogInformation("Retornados {Quantidade} jogos mais populares de um total de {Total}", result.Itens.Count(), total);
        return result;
    }

    public async Task<BaseResponseDto> ObterJogosRecomendadosAsync(Guid usuarioId)
    {
        _logger.LogInformation("Gerando recomendações de jogos para o usuário: {UsuarioId}", usuarioId);

        var usuarioJogos = await _usuarioJogosRepository.ObterJogosDoUsuarioAsync(usuarioId);
        
        var jogosIds = usuarioJogos.Select(uj => uj.JogoId.ToString());
        var categorias = usuarioJogos.Select(uj => uj.Jogo.Categoria).Distinct();

        var jogosRecomendados = await _elasticClient.ObterJogosRecomendadosAsync(categorias);
        
        var jogosDto = jogosRecomendados
            .Where(jogo => !jogosIds.Contains(jogo.Id))
            .Select(jogo => new JogoDto
            {
                Id = Guid.Parse(jogo.Id),
                Titulo = jogo.Titulo,
                Preco = jogo.Preco,
                Descricao = jogo.Descricao,
                Categoria = jogo.Categoria
            }).ToList();
        
        var mensagem = jogosDto.Count == 0 ? "Nenhum jogo recomendado" : string.Empty;
        _logger.LogInformation("Retornadas {Quantidade} recomendações para o usuário {UsuarioId}", jogosDto.Count, usuarioId);

        return new DataResponseDto<List<JogoDto>>(true, mensagem, jogosDto);
    }
}