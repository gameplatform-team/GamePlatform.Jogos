namespace GamePlatform.Jogos.Application.DTOs.Messaging;

public class PaymentSuccessMessage
{
    public Guid UsuarioId { get; init; }
    public Guid JogoId { get; init; }
}