using GamePlatform.Jogos.Application.DTOs.Elastic;
using GamePlatform.Jogos.Domain.Interfaces.Elastic;

namespace GamePlatform.Jogos.Application.Interfaces.Elastic;

public interface IJogoElasticClient : IElasticClient<JogoIndexMapping>
{
    Task<(IReadOnlyCollection<JogoIndexMapping> Documents, long Total)> ObterTodosAsync(
        int numeroPagina,
        int tamanhoPagina,
        string? titulo = null,
        double? precoMinimo = null,
        double? precoMaximo = null);
}