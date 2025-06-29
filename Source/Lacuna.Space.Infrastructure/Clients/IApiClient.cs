namespace Lacuna.Space.Infrastructure.Services;

public interface IApiClient
{
    Task<T> PostAsync<T>(string endpoint, object? request = null);
    Task<T> GetAsync<T>(string endpoint);
    void SetAuthorizationHeader(string token);
}