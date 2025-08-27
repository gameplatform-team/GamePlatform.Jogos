namespace GamePlatform.Jogos.Domain.Entities;

public class UsuarioJogo
{
    private UsuarioJogo() { }
    
    public UsuarioJogo(Guid usuarioId, Guid jogoId)
    {
        Id = Guid.NewGuid();
        CompradoEm = DateTime.UtcNow;
        
        UsuarioId = usuarioId;
        JogoId = jogoId;
    }
    
    public Guid Id { get; init; }
    public Guid UsuarioId { get; init; }
    public Guid JogoId { get; init; }
    public DateTime CompradoEm { get; init; }
    
    public Jogo Jogo { get; private set; } = null!;
}