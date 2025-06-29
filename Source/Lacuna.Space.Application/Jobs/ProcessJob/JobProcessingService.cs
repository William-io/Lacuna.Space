using Lacuna.Space.Domain.Abstractions;
using Lacuna.Space.Domain.Probes;
using Microsoft.Extensions.Logging;

namespace Lacuna.Space.Application.Jobs.ProcessJob;

public class JobProcessingService
{
    private readonly ILumaApiService _lumaApiService;
    private readonly ILogger<JobProcessingService> _logger;

    public JobProcessingService(ILumaApiService lumaApiService, ILogger<JobProcessingService> logger)
    {
        _lumaApiService = lumaApiService;
        _logger = logger;
    }

    public async Task<bool> ProcessAllJobsAsync(string accessToken, IEnumerable<Probe> synchronizedProbes)
    {
        _logger.LogInformation("Iniciando processamento de trabalhos");

        var probesDictionary = synchronizedProbes.ToDictionary(p => p.Name, p => p);
        var completedJobs = 0;

        while (true)
        {
            try
            {
                var job = await _lumaApiService.TakeJobAsync(accessToken);

                if (job == null)
                {
                    _logger.LogInformation("Não há mais trabalhos disponíveis. Total de trabalhos concluídos: {CompletedJobs}",
                        completedJobs);
                    return true;
                }

                _logger.LogInformation("Processando job {JobId} para sonda {ProbeName}", job.Id, job.ProbeName);

                if (!probesDictionary.TryGetValue(job.ProbeName, out var probe))
                {
                    _logger.LogError("Sonda {ProbeName} não encontrada para trabalho {JobId}", job.ProbeName, job.Id);
                    continue;
                }

                // Obter timestamp sincronizado da sonda
                var synchronizedTime = probe.GetSynchronizedTime();
                var encodedTimestamp = probe.GetEncodedTimestamp(synchronizedTime);

                // Verificar job
                var result = await _lumaApiService.CheckJobAsync(accessToken, job.Id, encodedTimestamp, probe.RoundTrip);
                
                if (result == "Done")
                {
                    _logger.LogInformation("Todos os trabalhos foram concluídos com sucesso! Total de trabalhos: {CompletedJobs}",
                        completedJobs + 1);
                    return true;
                }
                else if (result == "Success")
                {
                    completedJobs++;
                    job.MarkAsCompleted();
                    _logger.LogInformation("Trabalho {JobId} concluído com sucesso", job.Id);
                }
                else if (result == "Fail")
                {
                    _logger.LogError("Trabalho {JobId} falhou, necessário reiniciar desde o início", job.Id);
                    return false;
                }
                else
                {
                    _logger.LogWarning("Resultado inesperado para trabalho {JobId}: {Result}", job.Id, result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar trabalho");
                await Task.Delay(2000);
            }
        }
    }
}