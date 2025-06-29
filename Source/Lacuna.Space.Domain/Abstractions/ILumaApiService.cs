using Lacuna.Space.Domain.Jobs;
using Lacuna.Space.Domain.Probes;

namespace Lacuna.Space.Domain.Abstractions;

public interface ILumaApiService
{
    Task<string> StartSessionAsync(string username, string email);
    Task<IEnumerable<Probe>> GetProbesAsync(string accessToken);
    Task<(string t1, string t2)> SyncProbeAsync(string accessToken, string probeId);
    Task<Job?> TakeJobAsync(string accessToken);
    Task<string> CheckJobAsync(string accessToken, string jobId, string probeNow, long roundTrip);
}