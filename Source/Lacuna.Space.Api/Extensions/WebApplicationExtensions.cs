using Lacuna.Space.Api.Models;
using Lacuna.Space.Application.Orchestrations;

namespace Lacuna.Space.Api.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication MapSyncEndpoints(this WebApplication app)
    {
        app.MapPost("/sync",
                async (SyncRequest request, LumaOrchestrationService orchestrationService, ILogger<Program> logger) =>
                {
                    if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Email))
                    {
                        return Results.BadRequest(new SyncResponse
                        {
                            Success = false,
                            Message = "Nome de usuário e e-mail são obrigatórios",
                            Timestamp = DateTime.UtcNow
                        });
                    }

                    logger.LogInformation("Iniciando sincronização para usuário {Username} ({Email})",
                        request.Username, request.Email);

                    try
                    {
                        var startTime = DateTime.UtcNow;
                        var (result, probes) = await orchestrationService.ExecuteFullProcessAsync(request.Username, request.Email);
                        var duration = DateTime.UtcNow - startTime;

                        var response = new SyncResponse
                        {
                            Success = result,
                            Message = result ? "Sincronização concluída com sucesso" : "Falha na sincronização",
                            Duration = duration.TotalMilliseconds,
                            Timestamp = DateTime.UtcNow,
                            Probes = probes.Select(pb => new ProbeInfo
                            {
                                Id = pb.Id,
                                Name = pb.Name,
                                Encoding = pb.Encoding,
                                IsSynchronized = pb.IsSynchronized
                            }).ToList()
                        };

                        if (result)
                        {
                            logger.LogInformation("Sincronização concluída com sucesso para o usuário {Username} em {Duration}ms",
                                request.Username, duration.TotalMilliseconds);
                            return Results.Ok(response);
                        }

                        logger.LogWarning("Falha na sincronização do usuário {Username} após {Duration}ms",
                            request.Username, duration.TotalMilliseconds);
                        return Results.BadRequest(response);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Erro durante a sincronização do usuário {Username}", request.Username);
                        return Results.Problem("Ocorreu um erro durante a sincronização", statusCode: 500);
                    }
                })
            .WithName("SyncClocks")
            .WithTags("Sincronização")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Sincronizar relógios com sondas espaciais",
                Description = "Inicia o processo completo de sincronização de relógios entre o sistema e todas as sondas espaciais disponíveis para o usuário especificado. O processo inclui autenticação, descoberta de sondas, sincronização de relógios e processamento de jobs."
            })
            .Produces<SyncResponse>(StatusCodes.Status200OK)
            .Produces<SyncResponse>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        return app;
    }
}