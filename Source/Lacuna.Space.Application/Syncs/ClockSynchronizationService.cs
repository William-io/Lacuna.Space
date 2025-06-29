using Lacuna.Space.Domain.Abstractions;
using Lacuna.Space.Domain.Probes;
using Microsoft.Extensions.Logging;

namespace Lacuna.Space.Application.Syncs;

public class ClockSynchronizationService
{
    private readonly ILumaApiService _lumaApiService;
    private readonly ILogger<ClockSynchronizationService> _logger;
    private const long SYNC_TOLERANCE_TICKS = 50000; // 5ms em ticks

    public ClockSynchronizationService(ILumaApiService lumaApiService, ILogger<ClockSynchronizationService> logger)
    {
        _lumaApiService = lumaApiService;
        _logger = logger;
    }

    public async Task<bool> SynchronizeProbeAsync(Probe probe, string accessToken)
    {
        _logger.LogInformation("Iniciando sincronização para sonda {ProbeName}", probe.Name);

        var attempts = 0;
        const int maxAttempts = 10;

        while (!probe.IsSynchronized && attempts < maxAttempts)
        {
            attempts++;
            _logger.LogInformation("Tentativa de sincronização {Attempt} para sonda {ProbeName}", attempts, probe.Name);

            try
            {
                var t0 = DateTimeOffset.UtcNow.Ticks;
                var (t1, t2) = await _lumaApiService.SyncProbeAsync(accessToken, probe.Id);
                var t3 = DateTimeOffset.UtcNow.Ticks;

                // Decodifica t1 e t2 usando o encoding da sonda
                var t1Decoded = Probe.DecodeTimestamp(t1, probe.Encoding);
                var t2Decoded = Probe.DecodeTimestamp(t2, probe.Encoding);

                // Calcula offset e round trip conforme especificação do edital
                var timeOffset = ((t1Decoded - t0) + (t2Decoded - t3)) / 2;
                var roundTrip = (t3 - t0) - (t2Decoded - t1Decoded);

                probe.UpdateSynchronization(timeOffset, roundTrip);
                
                if (probe.IsSynchronized)
                {
                    _logger.LogInformation("Sonda {ProbeName} sincronizada com sucesso após {Attempts} tentativas",
                        probe.Name, attempts);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro específico ao sincronizar sonda {ProbeName} na tentativa {Attempt}: {Message}", probe.Name, attempts, ex.Message);
                await Task.Delay(1000); // Wait 1 second before retry
            }
        }

        _logger.LogWarning("Falha ao sincronizar sonda {ProbeName} após {MaxAttempts} tentativas", probe.Name,
            maxAttempts);
        return false;
    }
}