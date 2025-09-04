using System.Linq.Expressions;
using GamePlatform.Jogos.Application.DTOs;
using GamePlatform.Jogos.Application.DTOs.Elastic;
using GamePlatform.Jogos.Application.DTOs.Jogo;
using GamePlatform.Jogos.Application.DTOs.Messaging;
using GamePlatform.Jogos.Application.Interfaces.Elastic;
using GamePlatform.Jogos.Application.Interfaces.Services;
using GamePlatform.Jogos.Domain.Entities;
using GamePlatform.Jogos.Domain.Interfaces;
using GamePlatform.Jogos.Domain.Interfaces.Messaging;
using Microsoft.EntityFrameworkCore;

namespace GamePlatform.Jogos.Application.Services;

public class JogoService : IJogoService
{
    private const string JOGOS_INDEX_NAME = "jogos";
    
    private readonly IJogoRepository _jogoRepository;
    private readonly IUsuarioJogosRepository _usuarioJogosRepository;
    private readonly IServiceBusPublisher _publisher;
    private readonly IJogoElasticClient _elasticClient;

    public JogoService(
        IJogoRepository jogoRepository,
        IUsuarioJogosRepository usuarioJogosRepository,
        IServiceBusPublisher publisher,
        IJogoElasticClient elasticClient)
    {
        _jogoRepository = jogoRepository;
        _usuarioJogosRepository = usuarioJogosRepository;
        _publisher = publisher;
        _elasticClient = elasticClient;
    }

    public async Task<BaseResponseDto> CadastrarAsync(CadastrarJogoDto jogoDto)
    {
        if (await _jogoRepository.ExisteTituloAsync(jogoDto.Titulo))
            return new BaseResponseDto(false, "Jogo já cadastrado");

        var jogo = new Jogo(jogoDto.Titulo, jogoDto.Preco, jogoDto.Descricao, jogoDto.Categoria);
        await _jogoRepository.AdicionarAsync(jogo);
        
        var jogoIndex = new JogoIndexMapping()
        {
            Id = jogo.Id.ToString(),
            Titulo = jogo.Titulo,
            Preco = jogo.Preco,
            Descricao = jogo.Descricao,
            Categoria = jogo.Categoria,
            CreatedAt = jogo.CreatedAt,
            Popularidade = 0
        };
        
        await _elasticClient.CreateAsync(jogoIndex, JOGOS_INDEX_NAME);
        
        return new BaseResponseDto(true, "Jogo cadastrado com sucesso");
    }

    public async Task<BaseResponseDto> ObterPorIdAsync(Guid id)
    {
        var jogo = await _jogoRepository.ObterPorIdAsync(id);
        
        if (jogo == null)
            return new BaseResponseDto(false, "Jogo não encontrado");

        var jogoDto = new JogoDto
        {
            Id = jogo.Id,
            Titulo = jogo.Titulo,
            Preco = jogo.Preco,
            Descricao = jogo.Descricao,
            Categoria = jogo.Categoria
        };
        
        return new DataResponseDto<JogoDto>(true, string.Empty, jogoDto);
    }

    public async Task<ResultadoPaginadoDto<JogoDto>> ObterTodosAsync(
        string? titulo = null,
        decimal? precoMinimo = null,
        decimal? precoMaximo = null,
        int numeroPagina = 1,
        int tamanhoPagina = 10)
    {
        var (docs, total) = await _elasticClient.ObterTodosAsync(
            numeroPagina,
            tamanhoPagina,
            titulo,
            precoMinimo.HasValue ? Convert.ToDouble(precoMinimo) : null,
            precoMaximo.HasValue ? Convert.ToDouble(precoMaximo) : null);
        
        var result = new ResultadoPaginadoDto<JogoDto>()
        {
            Itens = docs.Select(jogo => new JogoDto
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
        
        return result;
    }

    public async Task<BaseResponseDto> AtualizarAsync(AtualizarJogoDto jogoDto)
    {
        var jogoExistente = await _jogoRepository.ObterPorIdAsync(jogoDto.Id);
    
        if (jogoExistente == null)
            return new BaseResponseDto(false, "Jogo não encontrado");
    
        var jogosComMesmoTitulo = await _jogoRepository.ObterTodosAsync(
            j => j.Titulo.ToLower() == jogoDto.Titulo.ToLower() && j.Id != jogoDto.Id);
    
        if (jogosComMesmoTitulo.Any())
            return new BaseResponseDto(false, "Já existe outro jogo com este título");

        jogoExistente.Atualizar(jogoDto.Titulo, jogoDto.Preco, jogoDto.Descricao, jogoDto.Categoria);
    
        await _jogoRepository.AtualizarAsync(jogoExistente);
        
        // TODO atualizar jogo no ElasticSearch
    
        return new BaseResponseDto(true, "Jogo atualizado com sucesso");
    }

    public async Task<BaseResponseDto> RemoverAsync(Guid id)
    {
        var jogoExistente = await _jogoRepository.ObterPorIdAsync(id);
    
        if (jogoExistente == null)
            return new BaseResponseDto(false, "Jogo não encontrado");
        
        await _jogoRepository.RemoverAsync(jogoExistente);
        
        // TODO remover jogo do ElasticSearch
        
        return new BaseResponseDto(true, "Jogo removido com sucesso");
    }

    public async Task<BaseResponseDto> ComprarJogoAsync(Guid usuarioId, ComprarJogoDto comprarJogoDto)
    {
        var jogo = await _jogoRepository.ObterPorIdAsync(comprarJogoDto.JogoId);
        if (jogo == null)
            return new BaseResponseDto(false, "Jogo não encontrado");
        
        var usuarioJogo = await _usuarioJogosRepository.ExisteUsuarioJogoAsync(usuarioId, comprarJogoDto.JogoId);
        if (usuarioJogo)
            return new BaseResponseDto(false, "Usuário já possui este jogo");
        
        var message = new GamePurchaseRequestedMessage
        {
            UsuarioId = usuarioId,
            JogoId = comprarJogoDto.JogoId,
            Preco = jogo.Preco,
            SolicitadoEm = DateTime.UtcNow
        };

        await _publisher.PublishAsync(
            queueName: "game-purchase-requested",
            message: message,
            messageId: Guid.NewGuid().ToString(),
            correlationId: usuarioId.ToString(),
            ct: CancellationToken.None
        );
        
        var responseDto = new ComprarJogoResponseDto("Compra iniciada. Aguarde confirmação.", "Pendente", comprarJogoDto.JogoId);
        return new DataResponseDto<ComprarJogoResponseDto>(true, string.Empty, responseDto);
    }

    public async Task<BaseResponseDto> ObterJogosDoUsuarioAsync(Guid usuarioId)
    {
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
        return new DataResponseDto<List<MeuJogoDto>>(true, mensagem, jogosDto);
    }

    public async Task AdicionaJogoUsuarioAsync(PaymentSuccessMessage message)
    {
        var usuarioJogo = new UsuarioJogo(message.UsuarioId, message.JogoId);
        await _usuarioJogosRepository.AdicionarAsync(usuarioJogo);
        
        // TODO incrementar campo "popularidade" do jogo no ElasticSearch
    }
}