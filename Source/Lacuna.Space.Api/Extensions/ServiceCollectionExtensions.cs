using Lacuna.Space.Application.Jobs.ProcessJob;
using Lacuna.Space.Application.Orchestrations;
using Lacuna.Space.Application.Syncs;
using Lacuna.Space.Domain.Abstractions;
using Lacuna.Space.Infrastructure.Authentication;
using Lacuna.Space.Infrastructure.Clients;
using Lacuna.Space.Infrastructure.Services;
using Lacuna.Space.Infrastructure.Validation;

namespace Lacuna.Space.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLumaServices(this IServiceCollection services, IConfiguration configuration)
    {
        var lumaBaseUrl = configuration["LumaApi:BaseUrl"] 
            ?? throw new InvalidOperationException("LumaApi:BaseUrl é obrigatório");
        var lumaTimeoutSeconds = configuration.GetValue<int>("LumaApi:TimeoutSeconds", 30);

        services.AddSingleton<TokenManager>();
        
        services.AddHttpClient<IApiClient, ApiClient>(client =>
        {
            client.BaseAddress = new Uri(lumaBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(lumaTimeoutSeconds);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("User-Agent", "Lacuna-Space-Client/1.0");
        });

        services.AddScoped<ILumaApiService, LumaApiService>();
        services.AddScoped<ClockSynchronizationService>();
        services.AddScoped<JobProcessingService>();
        services.AddScoped<LumaOrchestrationService>();
        services.AddScoped<IResponseValidator, ResponseValidator>();

        return services;
    }

    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new()
            {
                Title = "Lacuna Space API",
                Version = "v1",
                Description = "API para sincronização de relógios com sondas espaciais",
                Contact = new() { Name = "Lacuna Space Team" }
            });
        });

        return services;
    }
}