namespace Lacuna.Space.Infrastructure.Validation;

public class ResponseValidator : IResponseValidator
{
    public void ValidateResponse<T>(T response) where T : IApiResponse
    {
        if (response.Code == "Fail")
            throw new InvalidOperationException($"API retornou código de falha: {response.Message}");

        if (response.Code == "Unauthorized")
            throw new UnauthorizedAccessException("Token expirado ou inválido");

        if (response.Code != "Success")
            throw new InvalidOperationException($"Operação de API falhou: {response.Message}");
    }
}