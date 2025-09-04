using System.Linq.Expressions;
using GamePlatform.Jogos.Application.DTOs;
using GamePlatform.Jogos.Application.DTOs.Elastic;
using GamePlatform.Jogos.Application.DTOs.Jogo;
using GamePlatform.Jogos.Application.Services;
using GamePlatform.Jogos.Domain.Entities;
using GamePlatform.Jogos.Domain.Interfaces;
using GamePlatform.Jogos.Domain.Interfaces.Elastic;
using GamePlatform.Jogos.Domain.Interfaces.Messaging;
using Moq;

namespace GamePlatform.Jogos.Tests.Application.Services;

public class JogoServiceTests
{
    private readonly Mock<IJogoRepository> _jogoRepoMock;
    private readonly Mock<IUsuarioJogosRepository> _usuarioJogosRepoMock;
    private readonly Mock<IServiceBusPublisher> _serviceBusPubclisherMock;
    private readonly Mock<IElasticClient<JogoIndexMapping>> _elasticClientMock;
    private readonly JogoService _jogoService;

    public JogoServiceTests()
    {
        _jogoRepoMock = new Mock<IJogoRepository>();
        _usuarioJogosRepoMock = new Mock<IUsuarioJogosRepository>();
        _serviceBusPubclisherMock = new Mock<IServiceBusPublisher>();
        _elasticClientMock = new Mock<IElasticClient<JogoIndexMapping>>();
        _jogoService = new JogoService(
            _jogoRepoMock.Object,
            _usuarioJogosRepoMock.Object,
            _serviceBusPubclisherMock.Object,
            _elasticClientMock.Object);
    }

    [Fact]
    public async Task CadastrarAsync_DeveRetornarSucesso_QuandoJogoNaoExiste()
    {
        // Arrange
        var jogoDto = new CadastrarJogoDto { Titulo = "Novo Jogo", Preco = 99.99m };

        _jogoRepoMock.Setup(x => x.ExisteTituloAsync(jogoDto.Titulo)).ReturnsAsync(false);
        _jogoRepoMock.Setup(x => x.AdicionarAsync(It.IsAny<Jogo>())).Returns(Task.CompletedTask);

        // Act
        var resultado = await _jogoService.CadastrarAsync(jogoDto);

        // Assert
        _jogoRepoMock.Verify(x => x.ExisteTituloAsync(jogoDto.Titulo), Times.Once);
        _jogoRepoMock.Verify(x => x.AdicionarAsync(It.IsAny<Jogo>()), Times.Once);
        Assert.True(resultado.Sucesso);
        Assert.Equal("Jogo cadastrado com sucesso", resultado.Mensagem);
    }

    [Fact]
    public async Task CadastrarAsync_DeveRetornarErro_QuandoJogoJaExiste()
    {
        // Arrange
        var jogoDto = new CadastrarJogoDto { Titulo = "Jogo Existente", Preco = 99.99m };

        _jogoRepoMock.Setup(x => x.ExisteTituloAsync(jogoDto.Titulo)).ReturnsAsync(true);

        // Act
        var resultado = await _jogoService.CadastrarAsync(jogoDto);

        // Assert
        _jogoRepoMock.Verify(x => x.ExisteTituloAsync(jogoDto.Titulo), Times.Once);
        _jogoRepoMock.Verify(x => x.AdicionarAsync(It.IsAny<Jogo>()), Times.Never);
        Assert.False(resultado.Sucesso);
        Assert.Equal("Jogo já cadastrado", resultado.Mensagem);
    }
    
    [Fact]
    public async Task ObterPorIdAsync_DeveRetornarJogo_QuandoExistir()
    {
        // Arrange
        var jogo = new Jogo("Jogo Existente", 99.99m, "Descricao do jogo.", "Categoria");
        var jogoDto = new JogoDto
        {
            Id = jogo.Id,
            Titulo = jogo.Titulo,
            Preco = jogo.Preco,
            Descricao = jogo.Descricao,
            Categoria = jogo.Categoria
        };
        
        _jogoRepoMock.Setup(x => x.ObterPorIdAsync(It.IsAny<Guid>())).ReturnsAsync(jogo);
        
        // Act
        var resultado = await _jogoService.ObterPorIdAsync(Guid.NewGuid()) as DataResponseDto<JogoDto>;
        
        // Assert
        _jogoRepoMock.Verify(x => x.ObterPorIdAsync(It.IsAny<Guid>()), Times.Once);
        Assert.Equivalent(jogoDto, resultado!.Data, true);
        Assert.True(resultado.Sucesso);
    }
    
    [Fact]
    public async Task ObterPorIdAsync_DeveRetornarErro_QuandoNaoExistir()
    {
        // Arrange
        _jogoRepoMock.Setup(x => x.ObterPorIdAsync(It.IsAny<Guid>())).ReturnsAsync((Jogo?)null);
        
        // Act
        var resultado = await _jogoService.ObterPorIdAsync(Guid.NewGuid());
        
        // Assert
        _jogoRepoMock.Verify(x => x.ObterPorIdAsync(It.IsAny<Guid>()), Times.Once);
        Assert.False(resultado.Sucesso);
        Assert.Equal("Jogo não encontrado", resultado.Mensagem);
    }
    
    [Fact]
    public async Task ObterTodosAsync_DeveRetornarLista_QuandoExistemJogos()
    {
        // Arrange
        var jogos = new List<Jogo>
        {
            new("Jogo 1", 99.99m, "Descricao do Jogo 1.", "Categoria Jogo 1"),
            new("Jogo 2", 149.99m, "Descricao do Jogo 2.", "Categoria Jogo 2")
        };

        var jogosDtos = jogos.Select(j =>
            new JogoDto
            {
                Id = j.Id,
                Titulo = j.Titulo,
                Preco = j.Preco,
                Descricao = j.Descricao,
                Categoria = j.Categoria
            });
        
        _jogoRepoMock.Setup(x => x.ObterTodosPaginadoAsync(
            It.IsAny<Expression<Func<Jogo, bool>>>(),
            It.IsAny<int>(),
            It.IsAny<int>())).ReturnsAsync((jogos, jogos.Count));
        
        // Act
        var resultado = await _jogoService.ObterTodosAsync();
        
        // Assert
        _jogoRepoMock.Verify(x => x.ObterTodosPaginadoAsync(
            It.IsAny<Expression<Func<Jogo, bool>>>(),
            It.IsAny<int>(),
            It.IsAny<int>()), Times.Once);
        Assert.Equal(jogos.Count, resultado.Itens.Count());
        Assert.Equivalent(jogosDtos, resultado.Itens, true);
    }
    
    [Fact]
    public async Task ObterTodosAsync_DeveRetornarListaVazia_QuandoNaoExistemJogos()
    {
        // Arrange
        _jogoRepoMock.Setup(x => x.ObterTodosPaginadoAsync(
            It.IsAny<Expression<Func<Jogo, bool>>>(),
            It.IsAny<int>(),
            It.IsAny<int>())).ReturnsAsync(([], 0));
        
        // Act
        var resultado = await _jogoService.ObterTodosAsync();
        
        // Assert
        _jogoRepoMock.Verify(x => x.ObterTodosPaginadoAsync(
            It.IsAny<Expression<Func<Jogo, bool>>>(),
            It.IsAny<int>(),
            It.IsAny<int>()), Times.Once);
        Assert.Empty(resultado.Itens);
    }
    
    [Fact]
    public async Task AtualizarAsync_DeveRetornarErro_QuandoJogoNaoEncontrado()
    {
        // Arrange
        var jogoDto = new AtualizarJogoDto
        {
            Id = Guid.NewGuid(),
            Titulo = "Novo Nome Do Jogo",
            Preco = 129.99m,
            Descricao = "Uma nova descricao",
            Categoria = "Categoria"
        };

        _jogoRepoMock
            .Setup(x => x.ObterPorIdAsync(It.Is<Guid>(g => g == jogoDto.Id)))
            .ReturnsAsync((Jogo?)null);

        // Act
        var resultado = await _jogoService.AtualizarAsync(jogoDto);

        // Assert
        _jogoRepoMock.Verify(x => x.ObterPorIdAsync(It.Is<Guid>(g => g == jogoDto.Id)), Times.Once);
        _jogoRepoMock.Verify(x => x.ObterTodosAsync(It.IsAny<Expression<Func<Jogo, bool>>>()), Times.Never);
        _jogoRepoMock.Verify(x => x.AtualizarAsync(It.IsAny<Jogo>()), Times.Never);
        Assert.False(resultado.Sucesso);
        Assert.Equal("Jogo não encontrado", resultado.Mensagem);
    }
    
    [Fact]
    public async Task AtualizarAsync_DeveRetornarErro_QuandoOutroJogoComMesmoNomeExiste()
    {
        // Arrange
        var jogoDto = new AtualizarJogoDto
        {
            Id = Guid.NewGuid(),
            Titulo = "Novo Nome Do Jogo",
            Preco = 129.99m,
            Descricao = "Uma nova descricao",
            Categoria = "Categoria"
        };
        var jogoExistente = new Jogo("Nome Do Jogo", 159.99m, "Descricao do jogo.", "Categoria");

        _jogoRepoMock
            .Setup(x => x.ObterPorIdAsync(It.Is<Guid>(
                g => g == jogoDto.Id)))
            .ReturnsAsync(jogoExistente);
        
        _jogoRepoMock
            .Setup(x => x.ObterTodosAsync(It.IsAny<Expression<Func<Jogo, bool>>>()))
            .ReturnsAsync([ new Jogo("Novo Nome Do Jogo", 99.99m, "Outra descricao.", "Categoria")]);

        // Act
        var resultado = await _jogoService.AtualizarAsync(jogoDto);

        // Assert
        _jogoRepoMock.Verify(x => x.ObterPorIdAsync(It.Is<Guid>(g => g == jogoDto.Id)), Times.Once);
        _jogoRepoMock.Verify(x => x.ObterTodosAsync(It.IsAny<Expression<Func<Jogo, bool>>>()), Times.Once);
        _jogoRepoMock.Verify(x => x.AtualizarAsync(It.IsAny<Jogo>()), Times.Never);
        Assert.False(resultado.Sucesso);
        Assert.Equal("Já existe outro jogo com este título", resultado.Mensagem);
    }
    
    [Fact]
    public async Task AtualizarAsync_DeveRetornarSucesso_QuandoOutroJogoComMesmoNomeNaoExiste()
    {
        // Arrange
        var jogoDto = new AtualizarJogoDto
        {
            Id = Guid.NewGuid(),
            Titulo = "Novo Nome Do Jogo",
            Preco = 129.99m,
            Descricao = "Uma nova descricao",
            Categoria = "Categoria"
        };
        var jogoExistente = new Jogo("Nome Do Jogo", 159.99m, "Descricao do jogo.", "Categoria");

        _jogoRepoMock
            .Setup(x => x.ObterPorIdAsync(It.Is<Guid>(
                g => g == jogoDto.Id)))
            .ReturnsAsync(jogoExistente);
        
        _jogoRepoMock
            .Setup(x => x.ObterTodosAsync(It.IsAny<Expression<Func<Jogo, bool>>>()))
            .ReturnsAsync([]);
        
        _jogoRepoMock
            .Setup(x => x.AtualizarAsync(It.Is<Jogo>(
                j => j.Id == jogoDto.Id && j.Titulo == jogoDto.Titulo && j.Preco == jogoDto.Preco)))
            .Returns(Task.CompletedTask);

        // Act
        var resultado = await _jogoService.AtualizarAsync(jogoDto);

        // Assert
        _jogoRepoMock.Verify(x => x.ObterPorIdAsync(It.Is<Guid>(g => g == jogoDto.Id)), Times.Once);
        _jogoRepoMock.Verify(x => x.ObterTodosAsync(It.IsAny<Expression<Func<Jogo, bool>>>()), Times.Once);
        _jogoRepoMock.Verify(x => x.AtualizarAsync(It.IsAny<Jogo>()), Times.Once);
        Assert.True(resultado.Sucesso);
        Assert.Equal("Jogo atualizado com sucesso", resultado.Mensagem);
    }
    
    [Fact]
    public async Task RemoverAsync_DeveRetornarErro_QuandoIdNaoEncontrado()
    {
        // Arrange
        var jogoId = Guid.NewGuid();
        
        _jogoRepoMock
            .Setup(x => x.ObterPorIdAsync(It.Is<Guid>(g => g == jogoId)))
            .ReturnsAsync((Jogo?)null);
        
        // Act
        var resultado = await _jogoService.RemoverAsync(jogoId);
        
        // Assert
        _jogoRepoMock.Verify(x => x.ObterPorIdAsync(It.Is<Guid>(g => g == jogoId)), Times.Once);
        _jogoRepoMock.Verify(x => x.RemoverAsync(It.IsAny<Jogo>()), Times.Never);
        Assert.False(resultado.Sucesso);
        Assert.Equal("Jogo não encontrado", resultado.Mensagem);
    }
    
    [Fact]
    public async Task RemoverAsync_DeveRetornarSucesso_QuandoJogoRemovido()
    {
        // Arrange
        var jogo = new Jogo("Jogo Existente", 99.99m, "Descricao do jogo.", "Categoria");
        
        _jogoRepoMock
            .Setup(x => x.ObterPorIdAsync(It.Is<Guid>(g => g == jogo.Id)))
            .ReturnsAsync(jogo);
        
        _jogoRepoMock
            .Setup(x => x.RemoverAsync(It.Is<Jogo>(j => j == jogo)))
            .Returns(Task.CompletedTask);
        
        // Act
        var resultado = await _jogoService.RemoverAsync(jogo.Id);
        
        // Assert
        _jogoRepoMock.Verify(x => x.ObterPorIdAsync(It.Is<Guid>(g => g == jogo.Id)), Times.Once);
        _jogoRepoMock.Verify(x => x.RemoverAsync(It.Is<Jogo>(j => j == jogo)), Times.Once);
        Assert.True(resultado.Sucesso);
        Assert.Equal("Jogo removido com sucesso", resultado.Mensagem);
    }
}