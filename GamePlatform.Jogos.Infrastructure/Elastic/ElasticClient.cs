using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using GamePlatform.Jogos.Domain.Interfaces.Elastic;
using Microsoft.Extensions.Options;

namespace GamePlatform.Jogos.Infrastructure.Elastic;

public class ElasticClient<T> : IElasticClient<T>
{
    protected readonly ElasticsearchClient Client;

    public ElasticClient(IOptions<ElasticSettings> options)
    {
        Client = new ElasticsearchClient(options.Value.CloudId, new ApiKey(options.Value.ApiKey));
    }

    public async Task<IReadOnlyCollection<T>> SearchAsync(int pagina, int quantidade, IndexName index)
    {
        var response = await Client.SearchAsync<T>(s => s
            .Indices(index)
            .From(pagina)
            .Size(quantidade));
        
        return response.Documents;
    }

    public async Task<bool> CreateAsync(T entity, IndexName index)
    {
        var response = await Client.IndexAsync<T>(entity, s => s.Index(index));
        return response.IsValidResponse;
    }

    public async Task<bool> DeleteAsync(Guid id, IndexName index)
    {
        var response = await Client.DeleteAsync(index, id);
        return response.IsValidResponse;
    }
}