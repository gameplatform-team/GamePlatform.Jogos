using System.Linq.Expressions;
using GamePlatform.Jogos.Domain.Entities;

namespace GamePlatform.Jogos.Domain.Interfaces;

public interface IJogoRepository
{
    public Task<bool> ExisteTituloAsync(string titulo);
    public Task AdicionarAsync(Jogo jogo);
    public Task<Jogo?> ObterPorIdAsync(Guid id);
    public Task<IEnumerable<Jogo>> ObterTodosAsync(Expression<Func<Jogo, bool>>? filtro = null);
    public Task<(IEnumerable<Jogo> Jogos, int TotalDeItens)> ObterTodosPaginadoAsync(
        Expression<Func<Jogo, bool>>? filtro = null,
        int numeroPagina = 1,
        int tamanhoPagina = 10);
    public Task AtualizarAsync(Jogo jogo);
    public Task RemoverAsync(Jogo jogo);
}