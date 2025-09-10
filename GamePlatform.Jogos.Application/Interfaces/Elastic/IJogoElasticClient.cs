using GamePlatform.Jogos.Application.DTOs.Elastic;
using GamePlatform.Jogos.Domain.Entities;
using GamePlatform.Jogos.Domain.Interfaces.Elastic;

namespace GamePlatform.Jogos.Application.Interfaces.Elastic;

public interface IJogoElasticClient : IElasticClient<JogoIndexMapping>
{
    Task AdicionarAsync(Jogo jogo);
    Task<(IReadOnlyCollection<JogoIndexMapping> Documents, long Total)> ObterTodosAsync(
        int numeroPagina,
        int tamanhoPagina,
        string? titulo = null,
        double? precoMinimo = null,
        double? precoMaximo = null);

    Task AtualizarAsync(Jogo jogo);
    Task RemoverAsync(Guid jogoId);
    Task IncrementarPopularidadeAsync(Guid jogoId);
    Task<(IReadOnlyCollection<JogoIndexMapping> jogos, long total)> ObterTodosPorPopularidadeAsync(
        int numeroPagina,
        int tamanhoPagina);

    Task<IReadOnlyCollection<JogoIndexMapping>> ObterJogosRecomendadosAsync(IEnumerable<string> categorias);
}