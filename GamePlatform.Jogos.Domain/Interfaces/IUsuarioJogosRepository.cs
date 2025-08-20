using GamePlatform.Jogos.Domain.Entities;

namespace GamePlatform.Jogos.Domain.Interfaces;

public interface IUsuarioJogosRepository
{
    public Task AdicionarAsync(UsuarioJogo usuarioJogo);
    public Task<bool> ExisteUsuarioJogoAsync(Guid usuarioId, Guid jogoId);
}