using Microsoft.Extensions.Logging;

namespace Lacuna.Space.Infrastructure.Authentication;

public class TokenManager
{
    private readonly ILogger<TokenManager> _logger;
    private string? _accessToken;
    private DateTime _tokenExpiration;
    private readonly object _lockObject = new();

    public TokenManager(ILogger<TokenManager> logger) => _logger = logger;

    public void SetToken(string accessToken)
    {
        lock (_lockObject)
        {
            _accessToken = accessToken;
            // Token expira em 2 minutos, definimos 1m50s para margem de segurança
            _tokenExpiration = DateTime.UtcNow.AddSeconds(110);
            _logger.LogInformation("Token set, expires at {ExpirationTime}", _tokenExpiration);
        }
    }

    public string? GetValidToken()
    {
        lock (_lockObject)
        {
            if (_accessToken == null)
            {
                _logger.LogWarning("Nenhum token disponível");
                return null;
            }

            if (DateTime.UtcNow >= _tokenExpiration)
            {
                _logger.LogWarning("Token expirado em {ExpirationTime}, hora atual: {CurrentTime}",
                    _tokenExpiration, DateTime.UtcNow);
                _accessToken = null;
                return null;
            }

            return _accessToken;
        }
    }

    public bool IsTokenValid()
    {
        lock (_lockObject)
        {
            return _accessToken != null && DateTime.UtcNow < _tokenExpiration;
        }
    }

    public void InvalidateToken()
    {
        lock (_lockObject)
        {
            _logger.LogInformation("Token invalidado");
            _accessToken = null;
            _tokenExpiration = DateTime.MinValue;
        }
    }
}