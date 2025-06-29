namespace Lacuna.Space.Infrastructure.Validation;

public interface IResponseValidator
{
    void ValidateResponse<T>(T response) where T : IApiResponse;
}