using Lacuna.Space.Domain.Enums;

namespace Lacuna.Space.Api.Models;

public record SyncRequest(string Username, string Email);

public class SyncResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public double Duration { get; set; }
    public DateTime Timestamp { get; set; }
    public List<ProbeInfo> Probes { get; set; } = new();
}

public class ProbeInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public TimestampEncoding Encoding { get; set; }
    public bool IsSynchronized { get; set; }
}