namespace GamePlatform.Jogos.Application.DTOs.Elastic;

public class JogoIndexMapping
{
    public string Id { get; init; }
    public string Titulo { get; init; }
    public decimal Preco { get; init; }
    public string Descricao { get; init; }
    public string Categoria { get; init; }
    public DateTime CreatedAt { get; init; }
    public long Popularidade { get; init; }
}