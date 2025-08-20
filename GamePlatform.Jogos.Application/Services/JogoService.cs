using System.Linq.Expressions;
using GamePlatform.Jogos.Application.DTOs;
using GamePlatform.Jogos.Application.DTOs.Jogo;
using GamePlatform.Jogos.Application.Interfaces.Services;
using GamePlatform.Jogos.Domain.Entities;
using GamePlatform.Jogos.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GamePlatform.Jogos.Application.Services;

public class JogoService : IJogoService
{
    private readonly IJogoRepository _jogoRepository;
    private readonly IUsuarioJogosRepository _usuarioJogosRepository;

    public JogoService(
        IJogoRepository jogoRepository,
        IUsuarioJogosRepository usuarioJogosRepository)
    {
        _jogoRepository = jogoRepository;
        _usuarioJogosRepository = usuarioJogosRepository;
    }

    public async Task<BaseResponseDto> CadastrarAsync(CadastrarJogoDto jogoDto)
    {
        if (await _jogoRepository.ExisteTituloAsync(jogoDto.Titulo))
            return new BaseResponseDto(false, "Jogo já cadastrado");

        var jogo = new Jogo(jogoDto.Titulo, jogoDto.Preco, jogoDto.Descricao);
        await _jogoRepository.AdicionarAsync(jogo);
        
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
            Descricao = jogo.Descricao
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
        Expression<Func<Jogo, bool>>? filtro = null;

        if (!string.IsNullOrWhiteSpace(titulo) || precoMinimo.HasValue || precoMaximo.HasValue)
        {
            filtro = jogo =>
                (string.IsNullOrWhiteSpace(titulo) || EF.Functions.Like(jogo.Titulo.ToLower(), $"%{titulo.ToLower()}%")) &&
                (!precoMinimo.HasValue || jogo.Preco >= precoMinimo.Value) &&
                (!precoMaximo.HasValue || jogo.Preco <= precoMaximo.Value);
        }
        
        var (jogos, totalDeItens) = await _jogoRepository.ObterTodosPaginadoAsync(filtro, numeroPagina, tamanhoPagina);
        
        var result = new ResultadoPaginadoDto<JogoDto>()
        {
            Itens = jogos.Select(jogo => new JogoDto
            {
                Id = jogo.Id,
                Titulo = jogo.Titulo,
                Preco = jogo.Preco,
                Descricao = jogo.Descricao
            }),
            NumeroPagina = numeroPagina,
            TamanhoPagina = tamanhoPagina,
            TotalDeItens = totalDeItens
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

        jogoExistente.Atualizar(jogoDto.Titulo, jogoDto.Preco, jogoDto.Descricao);
    
        await _jogoRepository.AtualizarAsync(jogoExistente);
    
        return new BaseResponseDto(true, "Jogo atualizado com sucesso");
    }

    public async Task<BaseResponseDto> RemoverAsync(Guid id)
    {
        var jogoExistente = await _jogoRepository.ObterPorIdAsync(id);
    
        if (jogoExistente == null)
            return new BaseResponseDto(false, "Jogo não encontrado");
        
        await _jogoRepository.RemoverAsync(jogoExistente);
        
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
        
        // TODO (opcional) gravar intenção de compra em uma nova tabela (ComprasPendentes)
        // var compraPendente = new CompraPendente(usuarioId, comprarJogoDto.JogoId, "Solicitado");
        // await _comprasPendentesRepository.AdicionarAsync(compraPendente);
        
        // TODO publica evento GamePurchaseRequested
        
        
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
            CompradoEm = uj.CompradoEm
        }).ToList();
        
        var mensagem = jogosDto.Count == 0 ? "Nenhum jogo comprado" : string.Empty;
        return new DataResponseDto<List<MeuJogoDto>>(true, mensagem, jogosDto);
    }
}