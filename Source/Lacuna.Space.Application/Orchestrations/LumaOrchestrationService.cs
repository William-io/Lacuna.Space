using Lacuna.Space.Application.Jobs.ProcessJob;
using Lacuna.Space.Application.Syncs;
using Lacuna.Space.Domain.Abstractions;
using Lacuna.Space.Domain.Probes;
using Microsoft.Extensions.Logging;

namespace Lacuna.Space.Application.Orchestrations;

public class LumaOrchestrationService
{
    private readonly ILumaApiService _lumaApiService;
    private readonly ClockSynchronizationService _clockSyncService;
    private readonly JobProcessingService _jobProcessingService;
    private readonly ILogger<LumaOrchestrationService> _logger;

    public LumaOrchestrationService(
        ILumaApiService lumaApiService,
        ClockSynchronizationService clockSyncService,
        JobProcessingService jobProcessingService,
        ILogger<LumaOrchestrationService> logger)
    {
        _lumaApiService = lumaApiService;
        _clockSyncService = clockSyncService;
        _jobProcessingService = jobProcessingService;
        _logger = logger;
    }

    public async Task<(bool Success, IEnumerable<Probe> Probes)> ExecuteFullProcessAsync(string username, string email)
    {
        try
        {
            _logger.LogInformation("Iniciando processo de sincronização Luma para usuário {Username}", username);

            // 1. Inicializar sessão
            var accessToken = await _lumaApiService.StartSessionAsync(username, email);
            _logger.LogInformation("Sessão iniciada com sucesso");

            // 2. Obter lista de sondas
            var probes = await _lumaApiService.GetProbesAsync(accessToken);
            _logger.LogInformation("Recuperadas {ProbeCount} sondas", probes.Count());

            // 3. Sincronizar cada sonda
            var synchronizedProbes = new List<Probe>();
            foreach (var probe in probes)
            {
                var success = await _clockSyncService.SynchronizeProbeAsync(probe, accessToken);
                if (success)
                {
                    synchronizedProbes.Add(probe);
                }
                else
                {
                    _logger.LogError("Falha ao sincronizar sonda {ProbeName}", probe.Name);
                    return (false, probes);
                }
            }

            _logger.LogInformation("Todas as {ProbeCount} sondas sincronizadas com sucesso", synchronizedProbes.Count);

            // 4. Processar jobs
            var jobResult = await _jobProcessingService.ProcessAllJobsAsync(accessToken, synchronizedProbes);
            return (true, probes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante processo de sincronização Luma");
            return (false, Enumerable.Empty<Probe>());
        }
    }
}