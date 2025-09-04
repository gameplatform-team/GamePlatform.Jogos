using Elastic.Clients.Elasticsearch;

namespace GamePlatform.Jogos.Domain.Interfaces.Elastic;

public interface IElasticClient<T>
{
    Task<IReadOnlyCollection<T>> SearchAsync(int pagina, int quantidade, IndexName index);
    Task<bool> CreateAsync(T entity, IndexName index);
}