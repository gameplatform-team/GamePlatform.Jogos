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

    public JogoController(
        IJogoService jogoService,
        IUsuarioContextService usuarioContext)
    {
        _jogoService = jogoService;
        _usuarioContext = usuarioContext;
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
        var resultado = await _jogoService.CadastrarAsync(jogo);
        
        return !resultado.Sucesso ? BadRequest(resultado) : StatusCode(201, resultado);
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
        var resultado = await _jogoService.ObterTodosAsync(titulo, precoMinimo, precoMaximo, numeroPagina, tamanhoPagina);

        if (!resultado.Itens.Any())
            return NoContent();
        
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
        var resultado = await _jogoService.ObterPorIdAsync(id);
        
        return !resultado.Sucesso ? NotFound(resultado) : Ok(resultado);
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
        var resultado = await _jogoService.AtualizarAsync(jogoDto);
        
        return !resultado.Sucesso ? BadRequest(resultado) : Ok(resultado);
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
        var resultado = await _jogoService.RemoverAsync(id);
        
        return !resultado.Sucesso ? NotFound(resultado) : Ok(resultado);
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
        var resultado = await _jogoService.ComprarJogoAsync(usuarioId, comprarJogoDto);
        return !resultado.Sucesso ? BadRequest(resultado) : Accepted(resultado);
    }
    
    /// <summary>
    /// Obtém lista de jogos do usuário logado
    /// </summary>
    /// <response code="200">Lista de jogos do usuário</response>
    [ProducesResponseType(typeof(DataResponseDto<List<JogoDto>>), 200)]
    [HttpGet("meus-jogos")]
    [Authorize]
    public async Task<IActionResult> GetUserGamesAsync()
    {
        var usuarioId = _usuarioContext.GetUsuarioId();
        var resultado = await _jogoService.ObterJogosDoUsuarioAsync(usuarioId);
        return !resultado.Sucesso ? BadRequest(resultado) : Ok(resultado);
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
        var resultado = await _jogoService.ObterJogosPorPopularidadeAsync(numeroPagina, tamanhoPagina);
        
        if (!resultado.Itens.Any())
            return NoContent();
        
        return Ok(resultado);
    }
    
    // TODO criar endpoint GET /recomendados (para o usuário logado)
}