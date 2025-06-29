using Lacuna.Space.Domain.Abstractions;
using Lacuna.Space.Domain.Enums;
using Lacuna.Space.Domain.Jobs;
using Lacuna.Space.Domain.Probes;
using Lacuna.Space.Infrastructure.Authentication;
using Lacuna.Space.Infrastructure.Clients;
using Lacuna.Space.Infrastructure.Validation;
using Microsoft.Extensions.Logging;

namespace Lacuna.Space.Infrastructure.Services;

public class LumaApiService : ILumaApiService
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<LumaApiService> _logger;
    private readonly TokenManager _tokenManager;

    public LumaApiService(
        IApiClient apiClient,
        ILogger<LumaApiService> logger,
        TokenManager tokenManager)
    {
        _apiClient = apiClient;
        _logger = logger;
        _tokenManager = tokenManager;
    }

    public async Task<string> StartSessionAsync(string username, string email)
    {
        _logger.LogInformation("Iniciando sessão para usuário {Username}", username);

        var request = new { username, email };
        var result = await _apiClient.PostAsync<StartResponse>("/api/start", request);

        if (result.Code != "Success")
            throw new InvalidOperationException($"API retornou código de falha: {result.Message}");

        if (string.IsNullOrEmpty(result.AccessToken))
            throw new InvalidOperationException("Access token não foi retornado");

        _tokenManager.SetToken(result.AccessToken);
        _logger.LogInformation("Sessão iniciada com sucesso");
        return result.AccessToken;
    }

    public async Task<IEnumerable<Probe>> GetProbesAsync(string accessToken)
    {
        return await ExecuteWithTokenAsync(async (token) =>
        {
            _logger.LogInformation("Recuperando sondas");
            _apiClient.SetAuthorizationHeader(token);

            var result = await _apiClient.GetAsync<ProbesResponse>("/api/probe");

            if (result.Code != "Success")
                throw new InvalidOperationException($"API retornou código de falha: {result.Message}");

            if (result.Probes == null)
                throw new InvalidOperationException("Nenhuma sonda foi retornada");

            var probes = result.Probes.Select(p => new Probe(
                p.Id,
                p.Name,
                Enum.Parse<TimestampEncoding>(p.Encoding, true)
            ));

            _logger.LogInformation("Recuperado {ProbeCount} sondas", probes.Count());
            return probes;
        });
    }

    public async Task<(string t1, string t2)> SyncProbeAsync(string accessToken, string probeId)
    {
        return await ExecuteWithTokenAsync(async (token) =>
        {
            _logger.LogDebug("Sincronizando com a sonda {ProbeId}", probeId);
            _apiClient.SetAuthorizationHeader(token);

            var result = await _apiClient.PostAsync<SyncResponse>($"/api/probe/{probeId}/sync");

            if (result.Code != "Success")
                throw new InvalidOperationException($"API retornou código de falha: {result.Message}");

            return (result.T1, result.T2);
        });
    }

    public async Task<Job?> TakeJobAsync(string accessToken)
    {
        return await ExecuteWithTokenAsync(async (token) =>
        {
            _logger.LogDebug("Tomando o próximo trabalho");
            _apiClient.SetAuthorizationHeader(token);

            var result = await _apiClient.PostAsync<JobResponse>("/api/job/take");

            if (result.Code != "Success" && result.Job == null)
            {
                _logger.LogInformation("Não há mais empregos disponíveis");
                return null;
            }

            if (result.Code == "Unauthorized")
                throw new UnauthorizedAccessException("Token expirado ou inválido");

            if (result.Job == null) return null;

            _logger.LogInformation("Consegui emprego {JobId} para sonda {ProbeName}", result.Job.Id,
                result.Job.ProbeName);
            return new Job(result.Job.Id, result.Job.ProbeName);
        });
    }

    public async Task<string> CheckJobAsync(string accessToken, string jobId, string probeNow, long roundTrip)
    {
        return await ExecuteWithTokenAsync(async (token) =>
        {
            _logger.LogDebug("Verificando trabalho {JobId}", jobId);
            _apiClient.SetAuthorizationHeader(token);

            var request = new { probeNow, roundTrip };

            var result = await _apiClient.PostAsync<CheckJobResponse>($"/api/job/{jobId}/check", request);

            _logger.LogInformation("Trabalho {JobId} verificado com código: {Code} - Mensagem: {Message}", 
                jobId, result.Code, result.Message);

            return result.Code;
        });
    }

    private async Task<T> ExecuteWithTokenAsync<T>(Func<string, Task<T>> operation)
    {
        var token = _tokenManager.GetValidToken();
        if (token == null)
            throw new UnauthorizedAccessException("Nenhum token válido disponível");

        try
        {
            return await operation(token);
        }
        catch (UnauthorizedAccessException)
        {
            _logger.LogWarning("O token expirou durante a operação, invalidando o token");
            _tokenManager.InvalidateToken();
            throw;
        }
    }
}