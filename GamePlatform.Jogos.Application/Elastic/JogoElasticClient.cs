using Elastic.Clients.Elasticsearch.QueryDsl;
using GamePlatform.Jogos.Application.DTOs.Elastic;
using GamePlatform.Jogos.Application.Interfaces.Elastic;
using GamePlatform.Jogos.Domain.Entities;
using GamePlatform.Jogos.Infrastructure.Elastic;
using Microsoft.Extensions.Options;

namespace GamePlatform.Jogos.Application.Elastic;

public class JogoElasticClient : ElasticClient<JogoIndexMapping>, IJogoElasticClient
{
    private const string Index = "jogos";
    
    public JogoElasticClient(IOptions<ElasticSettings> options) : base(options)
    {
    }
    
    public async Task AdicionarAsync(Jogo jogo)
    {
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
        
        await CreateAsync(jogoIndex, Index);
    }

    public async Task<(IReadOnlyCollection<JogoIndexMapping> Documents, long Total)> ObterTodosAsync(
        int numeroPagina,
        int tamanhoPagina,
        string? titulo = null,
        double? precoMinimo = null,
        double? precoMaximo = null)
    {
        var offset = (numeroPagina - 1) * tamanhoPagina;

        var mustQueries = new List<Query>();

        if (!string.IsNullOrWhiteSpace(titulo))
        {
            mustQueries.Add(new WildcardQuery("titulo")
            {
                Wildcard = $"*{titulo.Trim().ToLowerInvariant()}*",
                CaseInsensitive = true
            });
        }

        if (precoMinimo.HasValue || precoMaximo.HasValue)
        {
            var range = new NumberRangeQuery(field: "preco");
            if (precoMinimo.HasValue) range.Gte = precoMinimo.Value;
            if (precoMaximo.HasValue) range.Lte = precoMaximo.Value;
            mustQueries.Add(range);
        }

        var response = await Client.SearchAsync<JogoIndexMapping>(s =>
        {
            s = s.Indices(Index).From(offset).Size(tamanhoPagina);

            if (mustQueries.Count == 0)
            {
                s.Query(q => q.MatchAll(new MatchAllQuery()));
                return;
            }

            s.Query(q => q.Bool(b => b.Must(mustQueries)));
        });

        var total = response.HitsMetadata.Total?.Value1?.Value ?? 0;
        return (response.Documents, total);
    }

    public async Task AtualizarAsync(Jogo jogo)
    {
        await Client.UpdateAsync<JogoIndexMapping, object>(
            Index,
            jogo.Id.ToString(),
            u => u.Doc(new
            {
                titulo = jogo.Titulo,
                preco = Convert.ToDouble(jogo.Preco),
                descricao = jogo.Descricao,
                categoria = jogo.Categoria
            })
        );
    }

    public async Task RemoverAsync(Guid jogoId)
    {
        await DeleteAsync(jogoId, Index);
    }
}