using System.ComponentModel.DataAnnotations;
using GamePlatform.Jogos.Application.DTOs;
using GamePlatform.Jogos.Application.DTOs.Jogo;
using GamePlatform.Jogos.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GamePlatform.Jogos.Api.Controllers;

[ApiController]
[Route("api/jogos")]
[ProducesResponseType(500)]
public class JogoController : ControllerBase
{
    private readonly IJogoService _jogoService;
    private readonly IUsuarioContextService _usuarioContext;
    private readonly ILogger<JogoController> _logger;

    public JogoController(
        IJogoService jogoService,
        IUsuarioContextService usuarioContext,
        ILogger<JogoController> logger)
    {
        _jogoService = jogoService;
        _usuarioContext = usuarioContext;
        _logger = logger;
    }

    /// <summary>
    /// Cadastra um novo jogo na plataforma
    /// </summary>
    /// <param name="jogo"></param>
    /// <response code="201">Jogo cadastrado com sucesso</response>
    /// <response code="400">Ocorreu um erro ao cadastrar o jogo</response>
    [ProducesResponseType(typeof(BaseResponseDto), 201)]
    [ProducesResponseType(typeof(BaseResponseDto), 400)]
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> PostAsync(CadastrarJogoDto jogo)
    {
        _logger.LogInformation("Iniciando cadastro de jogo. Titulo={Titulo}", jogo.Titulo);

        var resultado = await _jogoService.CadastrarAsync(jogo);

        if (!resultado.Sucesso)
        {
            _logger.LogWarning(
                "Falha ao cadastrar jogo. Titulo={Titulo} Erros={Erros}",
                jogo.Titulo,
                resultado.Mensagem);

            return BadRequest(resultado);
        }

        _logger.LogInformation(
            "Jogo cadastrado com sucesso. Titulo={Titulo}",
             jogo.Titulo);

        return StatusCode(201, resultado);
    }

    /// <summary>
    /// Obtém lista de jogos cadastrados na plataforma
    /// </summary>
    /// <param name="titulo">Filtrar jogos que o título contenha o texto informado</param>
    /// <param name="precoMinimo">Filtrar jogos por valor mínimo</param>
    /// <param name="precoMaximo">Filtrar jogos por valor máximo</param>
    /// <param name="numeroPagina">Número da página solicitada</param>
    /// <param name="tamanhoPagina">Quantidade de itens por página</param>
    /// <response code="200">Lista de jogos cadastrados</response>
    /// <response code="204">Nenhum jogo encontrado</response>
    [ProducesResponseType(typeof(ResultadoPaginadoDto<JogoDto>), 200)]
    [ProducesResponseType(204)]
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAllAsync(
        [FromQuery] string? titulo = null,
        [FromQuery] decimal? precoMinimo = null,
        [FromQuery] decimal? precoMaximo = null,
        [FromQuery] int numeroPagina = 1,
        [FromQuery] int tamanhoPagina = 10)
    {
        _logger.LogInformation(
           "Consultando jogos. Titulo={Titulo} PrecoMin={PrecoMin} PrecoMax={PrecoMax} Pagina={Pagina} Tamanho={Tamanho}",
           titulo, precoMinimo, precoMaximo, numeroPagina, tamanhoPagina);

        var resultado = await _jogoService.ObterTodosAsync(titulo, precoMinimo, precoMaximo, numeroPagina, tamanhoPagina);

        if (!resultado.Itens.Any())
        {
            _logger.LogInformation("Nenhum jogo encontrado para os filtros informados");
            return NoContent();
        }

        _logger.LogInformation(
            "Consulta de jogos realizada com sucesso. TotalItens={Total}",
            resultado.TotalDeItens);

        return Ok(resultado);
    }

    /// <summary>
    /// Obtém um jogo pelo ID
    /// </summary>
    /// <param name="id">ID do jogo</param>
    /// <response code="200">Jogo encontrado com sucesso</response>
    /// <response code="404">Jogo não encontrado</response>
    [ProducesResponseType(typeof(DataResponseDto<JogoDto>), 200)]
    [ProducesResponseType(typeof(BaseResponseDto), 404)]
    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> GetByIdAsync([FromRoute] Guid id)
    {
        _logger.LogInformation("Buscando jogo por ID. JogoId={JogoId}", id);

        var resultado = await _jogoService.ObterPorIdAsync(id);

        if (!resultado.Sucesso)
        {
            _logger.LogWarning("Jogo não encontrado. JogoId={JogoId}", id);
            return NotFound(resultado);
        }

        return Ok(resultado);
    }
    
    /// <summary>
    /// Atualiza um jogo na plataforma
    /// </summary>
    /// <param name="jogoDto">Dados do jogo</param>
    /// <response code="200">Jogo atualizado com sucesso</response>
    /// <response code="400">Ocorreu um erro ao atualizar o jogo</response>
    [ProducesResponseType(typeof(BaseResponseDto), 200)]
    [ProducesResponseType(typeof(BaseResponseDto), 400)]
    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> PutAsync(AtualizarJogoDto jogoDto)
    {
        _logger.LogInformation("Atualizando jogo. JogoId={JogoId}", jogoDto.Id);

        var resultado = await _jogoService.AtualizarAsync(jogoDto);

        if (!resultado.Sucesso)
        {
            _logger.LogWarning(
                "Falha ao atualizar jogo. JogoId={JogoId} Erros={Erros}",
                jogoDto.Id,
                resultado.Mensagem);

            return BadRequest(resultado);
        }

        _logger.LogInformation("Jogo atualizado com sucesso. JogoId={JogoId}", jogoDto.Id);

        return Ok(resultado);
    }

    /// <summary>
    /// Remove um jogo da plataforma
    /// </summary>
    /// <param name="id">ID do jogo</param>
    /// <response code="200">Jogo removido com sucesso</response>
    /// <response code="404">Jogo não encontrado</response>
    [ProducesResponseType(typeof(BaseResponseDto), 200)]
    [ProducesResponseType(typeof(BaseResponseDto), 404)]
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteAsync([FromRoute] Guid id)
    {
        _logger.LogInformation("Removendo jogo. JogoId={JogoId}", id);

        var resultado = await _jogoService.RemoverAsync(id);

        if (!resultado.Sucesso)
        {
            _logger.LogWarning("Falha ao remover jogo. JogoId={JogoId}", id);
            return NotFound(resultado);
        }

        _logger.LogInformation("Jogo removido com sucesso. JogoId={JogoId}", id);

        return Ok(resultado);
    }
    
    /// <summary>
    /// Inicia a compra de um jogo pelo usuário logado
    /// </summary>
    /// <param name="comprarJogoDto"></param>
    /// <response code="202">Compra iniciada com sucesso</response>
    /// <response code="400">Ocorreu um erro ao realizar a compra do jogo</response>
    [ProducesResponseType(typeof(DataResponseDto<ComprarJogoResponseDto>), 202)]
    [ProducesResponseType(typeof(BaseResponseDto), 400)]
    [HttpPost("comprar")]
    [Authorize]
    public async Task<IActionResult> ComprarAsync([FromBody, Required] ComprarJogoDto comprarJogoDto)
    {
        var usuarioId = _usuarioContext.GetUsuarioId();

        _logger.LogInformation(
           "Usuário iniciando compra. UsuarioId={UsuarioId} JogoId={JogoId}",
           usuarioId,
           comprarJogoDto.JogoId);

        var resultado = await _jogoService.ComprarJogoAsync(usuarioId, comprarJogoDto);

        if (!resultado.Sucesso)
        {
            _logger.LogWarning(
                "Falha na compra do jogo. UsuarioId={UsuarioId} JogoId={JogoId}",
                usuarioId,
                comprarJogoDto.JogoId);

            return BadRequest(resultado);
        }

        _logger.LogInformation(
            "Compra iniciada com sucesso. UsuarioId={UsuarioId} JogoId={JogoId}",
            usuarioId,
            comprarJogoDto.JogoId);

        return Accepted(resultado);
    }
    
    /// <summary>
    /// Obtém lista de jogos do usuário logado
    /// </summary>
    /// <response code="200">Lista de jogos do usuário</response>
    [ProducesResponseType(typeof(DataResponseDto<List<MeuJogoDto>>), 200)]
    [HttpGet("meus-jogos")]
    [Authorize]
    public async Task<IActionResult> GetUserGamesAsync()
    {
        var usuarioId = _usuarioContext.GetUsuarioId();

        _logger.LogInformation("Consultando jogos do usuário. UsuarioId={UsuarioId}", usuarioId);

        var resultado = await _jogoService.ObterJogosDoUsuarioAsync(usuarioId);

        if (!resultado.Sucesso)
        {
            _logger.LogWarning("Falha ao consultar jogos do usuário. UsuarioId={UsuarioId} Erros={Erros}", usuarioId, resultado.Mensagem);
            return BadRequest(resultado);
        }

        _logger.LogInformation(
            "Consulta de jogos do usuário finalizada com sucesso. UsuarioId={UsuarioId}",
            usuarioId);

        return Ok(resultado);
    }

    /// <summary>
    /// Obtém lista de jogos por ordem de popularidade
    /// </summary>
    /// <param name="numeroPagina">Número da página solicitada</param>
    /// <param name="tamanhoPagina">Quantidade de itens por página</param>
    /// <response code="200">Lista de jogos por ordem de popularidade</response>
    /// <response code="204">Nenhum jogo encontrado</response>
    [ProducesResponseType(typeof(ResultadoPaginadoDto<JogoDto>), 200)]
    [ProducesResponseType(204)]
    [HttpGet("populares")]
    [Authorize]
    public async Task<IActionResult> GetPopularesAsync(
        [FromQuery] int numeroPagina = 1,
        [FromQuery] int tamanhoPagina = 10)
    {
        _logger.LogInformation(
           "Consultando jogos populares. Pagina={Pagina} Tamanho={Tamanho}",
           numeroPagina,
           tamanhoPagina);

        var resultado = await _jogoService.ObterJogosPorPopularidadeAsync(numeroPagina, tamanhoPagina);

        if (!resultado.Itens.Any())
        {
            _logger.LogInformation(
                "Consulta de jogos populares sem resultados. Pagina={Pagina} Tamanho={Tamanho}",
                numeroPagina,
                tamanhoPagina);

            return NoContent();
        }

        _logger.LogInformation(
            "Consulta de jogos populares finalizada com sucesso. Pagina={Pagina} Tamanho={Tamanho} QuantidadeItens={Quantidade}",
            numeroPagina,
            tamanhoPagina,
            resultado.TotalDeItens);

        return Ok(resultado);
    }
    
    /// <summary>
    /// Obtém lista de jogos recomendados para o usuário logado
    /// </summary>
    /// <response code="200">Lista de jogos recomendados para o usuário, por ordem de popularidade</response>
    [ProducesResponseType(typeof(DataResponseDto<List<JogoDto>>), 200)]
    [HttpGet("recomendados")]
    [Authorize]
    public async Task<IActionResult> GetRecomendadosAsync()
    {
        var usuarioId = _usuarioContext.GetUsuarioId();

        _logger.LogInformation(
          "Consultando jogos recomendados. UsuarioId={UsuarioId}",
          usuarioId);

        var resultado = await _jogoService.ObterJogosRecomendadosAsync(usuarioId);

        if (!resultado.Sucesso)
        {
            _logger.LogWarning(
                "Falha ao obter jogos recomendados. UsuarioId={UsuarioId} Erros={Erros}",
                usuarioId,
                resultado.Mensagem);

            return BadRequest(resultado);
        }

        _logger.LogInformation(
            "Consulta de jogos recomendados finalizada com sucesso. UsuarioId={UsuarioId}",
            usuarioId);

        return Ok(resultado);
    }
}