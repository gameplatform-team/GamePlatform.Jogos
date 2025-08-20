namespace GamePlatform.Jogos.Application.DTOs;

public class DataResponseDto<T> : BaseResponseDto
{
    public DataResponseDto(bool sucesso, string mensagem, T data) : base(sucesso, mensagem)
    {
        Data = data;
    }
    
    public T Data { get; private set; }
}