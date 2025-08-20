using System.ComponentModel.DataAnnotations;

namespace GamePlatform.Jogos.Application.DTOs.Jogo;

public class ComprarJogoDto
{
    [Required]
    public Guid JogoId { get; init; }
}