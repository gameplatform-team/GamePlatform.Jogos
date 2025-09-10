using Elastic.Clients.Elasticsearch;
using Elastic.Transport.Products.Elasticsearch;

namespace GamePlatform.Jogos.Domain.Interfaces.Elastic;

public interface IElasticClient<T>
{
    Task<IReadOnlyCollection<T>> SearchAsync(int pagina, int quantidade, IndexName index);
    Task<ElasticsearchResponse> CreateAsync(T entity, IndexName index);
    Task<ElasticsearchResponse> DeleteAsync(Guid id, IndexName index);
}