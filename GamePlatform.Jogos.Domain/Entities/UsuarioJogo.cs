namespace GamePlatform.Jogos.Domain.Entities;

public class UsuarioJogo
{
    public UsuarioJogo(Guid usuarioId, Guid jogoId)
    {
        Id = Guid.NewGuid();
        CompradoEm = DateTime.UtcNow;
        
        UsuarioId = usuarioId;
        JogoId = jogoId;
    }
    
    public Guid Id { get; internal set; }
    public Guid UsuarioId { get; internal set; }
    public Guid JogoId { get; internal set; }
    public DateTime CompradoEm { get; internal set; }
}