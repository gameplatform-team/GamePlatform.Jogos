using Elastic.Clients.Elasticsearch;
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
        
        var response = await CreateAsync(jogoIndex, Index);
        
        if (!response.IsValidResponse)
        {
            throw new Exception($"Erro ao adicionar jogo no Elasticsearch. ID: {jogo.Id}: {response.DebugInformation}");
        }
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
        var response = await Client.UpdateAsync<JogoIndexMapping, object>(
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
        
        if (!response.IsValidResponse)
        {
            throw new Exception($"Erro ao atualizar jogo no Elasticsearch. ID: {jogo.Id}: {response.DebugInformation}");
        }
    }

    public async Task RemoverAsync(Guid jogoId)
    {
        var response = await DeleteAsync(jogoId, Index);
        
        if (!response.IsValidResponse)
        {
            throw new Exception($"Erro ao remover jogo no Elasticsearch. ID: {jogoId}: {response.DebugInformation}");
        }
    }

    public async Task IncrementarPopularidadeAsync(Guid jogoId)
    {
        var response = await Client.UpdateAsync<JogoIndexMapping, object>(
            Index,
            jogoId,
            u => u
                .Script(s => s.Source("ctx._source.popularidade += params.count").Params(p => p.Add("count", 1)))
                .RetryOnConflict(3)
        );

        if (!response.IsValidResponse)
        {
            throw new Exception($"Erro ao incrementar popularidade do jogo no Elasticsearch. ID {jogoId}: {response.DebugInformation}");
        }
    }

    public async Task<(IReadOnlyCollection<JogoIndexMapping> jogos, long total)> ObterTodosPorPopularidadeAsync(int numeroPagina, int tamanhoPagina)
    {
        var offset = (numeroPagina - 1) * tamanhoPagina;

        var response = await Client.SearchAsync<JogoIndexMapping>(s => s
            .Indices(Index)
            .From(offset)
            .Size(tamanhoPagina)
            .Sort(sr => sr.Field(f => f
                .Field(j => j.Popularidade)
                .Order(SortOrder.Desc))));

        var total = response.HitsMetadata.Total?.Value1?.Value ?? 0;
        return (response.Documents, total);

    }

    public async Task<IReadOnlyCollection<JogoIndexMapping>> ObterJogosRecomendadosAsync(IEnumerable<string> categorias)
    {
        var response = await Client.SearchAsync<JogoIndexMapping>(s => s
            .Indices(Index)
            .Size(100)
            .Query(q => q
                .Terms(t => t
                    .Field(j => j.Categoria)
                    .Terms(new TermsQueryField([.. categorias]))))
            .Sort(sr => sr.Field(f => f
                .Field(j => j.Popularidade)
                .Order(SortOrder.Desc))));

        if (!response.IsValidResponse)
        {
            throw new Exception($"Erro na consulta ES dos jogos recomendados: {response.DebugInformation}");
        }

        return response.Documents;
    }
}