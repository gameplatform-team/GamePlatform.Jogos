using System.Linq.Expressions;
using GamePlatform.Jogos.Domain.Entities;
using GamePlatform.Jogos.Domain.Interfaces;
using GamePlatform.Jogos.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GamePlatform.Jogos.Infrastructure.Repositories;

public class JogoRepository : IJogoRepository
{
    private readonly DataContext _context;
    
    public JogoRepository(DataContext context)
    {
        _context = context;
    }
    
    public async Task<bool> ExisteTituloAsync(string titulo)
    {
        return await _context.Jogos.AnyAsync(j => j.Titulo == titulo);
    }

    public async Task AdicionarAsync(Jogo jogo)
    {
        await _context.Jogos.AddAsync(jogo);
        await _context.SaveChangesAsync();
    }

    public async Task<Jogo?> ObterPorIdAsync(Guid id)
    {
        return await _context.Jogos.FindAsync(id);
    }
    
    public async Task<IEnumerable<Jogo>> ObterTodosAsync(Expression<Func<Jogo, bool>>? filtro = null)
    {
        var query = _context.Jogos.AsQueryable();

        if (filtro != null)
            query = query.Where(filtro);

        return await query.ToListAsync();
    }

    public async Task<(IEnumerable<Jogo> Jogos, int TotalDeItens)> ObterTodosPaginadoAsync(
        Expression<Func<Jogo, bool>>? filtro = null,
        int numeroPagina = 1,
        int tamanhoPagina = 10)
    {
        var query = _context.Jogos.AsQueryable();

        if (filtro != null)
            query = query.Where(filtro);
        
        var totalDeItens = await query.CountAsync();

        var jogos = await query
            .Skip((numeroPagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync();

        return (jogos, totalDeItens);
    }

    public async Task AtualizarAsync(Jogo jogo)
    {
        _context.Jogos.Update(jogo);
        await _context.SaveChangesAsync();
    }

    public async Task RemoverAsync(Jogo jogo)
    {
        _context.Jogos.Remove(jogo);
        await _context.SaveChangesAsync();
    }
}