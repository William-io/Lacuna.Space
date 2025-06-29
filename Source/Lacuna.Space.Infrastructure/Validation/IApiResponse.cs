namespace Lacuna.Space.Infrastructure.Validation;

public interface IApiResponse
{
    string Code { get; }
    string? Message { get; }
}

public record StartResponse(string? AccessToken, string Code, string? Message) : IApiResponse;
public record ProbesResponse(ProbeDto[]? Probes, string Code, string? Message) : IApiResponse;
public record ProbeDto(string Id, string Name, string Encoding);
public record SyncResponse(string T1, string T2, string Code, string? Message) : IApiResponse;
public record JobResponse(JobDto? Job, string Code, string? Message) : IApiResponse;
public record JobDto(string Id, string ProbeName);
public record CheckJobResponse(string Code, string? Message) : IApiResponse;