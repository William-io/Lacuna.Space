namespace Lacuna.Space.Infrastructure.Clients;

public interface IApiClient
{
    Task<T> PostAsync<T>(string endpoint, object? request = null);
    Task<T> GetAsync<T>(string endpoint);
    void SetAuthorizationHeader(string token);
}