using GamePlatform.Jogos.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GamePlatform.Jogos.Infrastructure.Data;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options) { }

    public DbSet<Jogo> Jogos { get; set; }
    public DbSet<UsuarioJogo> UsuarioJogos { get; set; }
    // public DbSet<CompraPendente> ComprasPendentes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DataContext).Assembly);
    }
}