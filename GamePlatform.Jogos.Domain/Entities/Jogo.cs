namespace GamePlatform.Jogos.Domain.Entities;

public class Jogo : BaseEntity
{
    public string Titulo { get; private set; }
    public decimal Preco { get; private set; }
    public string Descricao { get; private set; }
    public string Categoria { get; private set; }

    public Jogo(string titulo, decimal preco, string descricao, string categoria)
    {
        Titulo = titulo;
        Preco = preco;
        Descricao = descricao;
        Categoria = categoria;
    }

    public void Atualizar(string titulo, decimal preco, string descricao, string categoria)
    {
        Titulo = titulo;
        Preco = preco;
        Descricao = descricao;
        Categoria = categoria;
        UpdatedAt = DateTime.UtcNow;
    }
}