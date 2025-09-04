using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using GamePlatform.Jogos.Domain.Interfaces.Elastic;
using Microsoft.Extensions.Options;

namespace GamePlatform.Jogos.Infrastructure.Elastic;

public class ElasticClient<T> : IElasticClient<T>
{
    private readonly ElasticsearchClient _client;

    public ElasticClient(IOptions<ElasticSettings> options)
    {
        _client = new ElasticsearchClient(options.Value.CloudId, new ApiKey(options.Value.ApiKey));
    }

    public async Task<IReadOnlyCollection<T>> SearchAsync(int pagina, int quantidade, IndexName index)
    {
        var response = await _client.SearchAsync<T>(s => s
            .Indices(index)
            .From(pagina)
            .Size(quantidade));
        
        return response.Documents;
    }

    public async Task<bool> CreateAsync(T entity, IndexName index)
    {
        var response = await _client.IndexAsync<T>(entity, s => s.Index(index));
        return response.IsValidResponse;
    }
}