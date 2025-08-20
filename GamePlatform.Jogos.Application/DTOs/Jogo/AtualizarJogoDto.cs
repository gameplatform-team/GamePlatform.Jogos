using System.ComponentModel.DataAnnotations;

namespace GamePlatform.Jogos.Application.DTOs.Jogo;

public class AtualizarJogoDto
{
    [Required]
    public Guid Id { get; init; }
    
    [Required]
    [MinLength(1)]
    public string Titulo { get; init; }
    
    [Required]
    [Range(0.01, 100000.00)]
    public decimal Preco { get; init; }
    
    [Required]
    [MinLength(1)]
    public string Descricao { get; init; }
}