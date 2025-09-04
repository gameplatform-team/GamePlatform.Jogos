namespace GamePlatform.Jogos.Application.DTOs.Jogo;

public class MeuJogoDto
{
    public Guid Id { get; set; }
    public string Titulo { get; set; }
    public string Descricao { get; set; }
    public string Categoria { get; set; }
    public DateTime CompradoEm { get; set; }
}