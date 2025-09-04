using GamePlatform.Jogos.Domain.Entities;

namespace GamePlatform.Jogos.Tests.Domain.Entities;

public class JogoTests
{
    [Fact]
    public void Deve_Criar_Jogo_Com_Dados_Corretos()
    {
        // Arrange
        var titulo = "Super Mario";
        var preco = 59.99m;
        var descricao = "Jogo de plataforma";
        var categoria = "Plataforma";

        // Act
        var jogo = new Jogo(titulo, preco, descricao, categoria);

        // Assert
        Assert.Equal(titulo, jogo.Titulo);
        Assert.Equal(preco, jogo.Preco);
        Assert.Equal(descricao, jogo.Descricao);
        
        Assert.NotEqual(Guid.Empty, jogo.Id);
        Assert.NotEqual(default, jogo.CreatedAt);
        Assert.True(jogo.CreatedAt <= DateTime.UtcNow);
        Assert.Null(jogo.UpdatedAt);
    }
    
    [Fact]
    public void Atualizar_DeveAtualizarDadosDoJogo()
    {
        // Arrange
        var titulo = "Super Mario";
        var preco = 59.99m;
        var descricao = "Jogo de plataforma";
        var categoria = "Plataforma";
        
        var jogo = new Jogo(titulo, preco, descricao, categoria);
        
        // Act
        jogo.Atualizar("Super Mario Bros", 89.99m, "Nova descricao", "Nova categoria");
        
        // Assert
        Assert.Equal("Super Mario Bros", jogo.Titulo);
        Assert.Equal(89.99m, jogo.Preco);
        Assert.Equal("Nova descricao", jogo.Descricao);
        Assert.Equal("Nova categoria", jogo.Categoria);
        Assert.NotNull(jogo.UpdatedAt);
        Assert.True(jogo.UpdatedAt <= DateTime.UtcNow);
    }
}