using System.Text.Json.Serialization;

namespace GamePlatform.Jogos.Application.DTOs;

public class BaseResponseDto
{
    public BaseResponseDto() {}

    public BaseResponseDto(bool sucesso, string mensagem)
    {
        Sucesso = sucesso;
        Mensagem = mensagem;
    }
    
    [JsonPropertyOrder(-2)]
    public bool Sucesso { get; set; }
    
    [JsonPropertyOrder(-1)]
    public string Mensagem { get; set; }
}