namespace GamePlatform.Jogos.Application.DTOs.Jogo;

public class JogoDto
{
    public Guid Id { get; set; }
    public string Titulo { get; set; }
    public decimal Preco { get; set; }
    public string Descricao { get; set; }
}