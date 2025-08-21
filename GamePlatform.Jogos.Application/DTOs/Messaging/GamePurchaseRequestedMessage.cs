namespace GamePlatform.Jogos.Application.DTOs.Messaging;

public class GamePurchaseRequestedMessage
{
    public static string TipoEvento => "GamePurchaseRequested";
    public Guid UsuarioId { get; set; }
    public Guid JogoId { get; set; }
    public decimal Preco { get; set; }
    public DateTime SolicitadoEm { get; set; }
}