using GamePlatform.Jogos.Domain.Entities;
using GamePlatform.Jogos.Domain.Interfaces;
using GamePlatform.Jogos.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GamePlatform.Jogos.Infrastructure.Repositories;

public class UsuarioJogosRepository : IUsuarioJogosRepository
{
    private readonly DataContext _context;

    public UsuarioJogosRepository(DataContext context)
    {
        _context = context;
    }

    public async Task AdicionarAsync(UsuarioJogo usuarioJogo)
    {
        await _context.UsuarioJogos.AddAsync(usuarioJogo);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExisteUsuarioJogoAsync(Guid usuarioId, Guid jogoId)
    {
        var usuarioJogo = _context.UsuarioJogos
            .Where(x => x.JogoId == jogoId && x.UsuarioId == usuarioId);

        return await usuarioJogo.AnyAsync();
    }

    public async Task<List<UsuarioJogo>> ObterJogosDoUsuarioAsync(Guid usuarioId)
    {
        // return await _context.UsuarioJogos
        //     .Where(uj => uj.UsuarioId == usuarioId)
        //     .Select(uj => uj.Jogo)
        //     .ToListAsync();
        
        return await _context.UsuarioJogos
            .Include(uj => uj.Jogo)
            .Where(uj => uj.UsuarioId == usuarioId).ToListAsync();
    }
}