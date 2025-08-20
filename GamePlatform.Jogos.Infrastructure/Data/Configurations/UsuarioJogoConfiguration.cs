using GamePlatform.Jogos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GamePlatform.Jogos.Infrastructure.Data.Configurations;

public class UsuarioJogoConfiguration : IEntityTypeConfiguration<UsuarioJogo>
{
    public void Configure(EntityTypeBuilder<UsuarioJogo> builder)
    {
        builder.ToTable("UsuarioJogos");

        builder.HasKey(j => j.Id);

        builder.Property(j => j.UsuarioId)
            .IsRequired();

        builder.Property(j => j.JogoId)
            .IsRequired();
    }
}