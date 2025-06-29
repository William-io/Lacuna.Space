using Asp.Versioning;
using Lacuna.Space.Api.Configuration;
using Lacuna.Space.Application.Jobs.ProcessJob;
using Lacuna.Space.Application.Orchestrations;
using Lacuna.Space.Application.Syncs;
using Lacuna.Space.Domain.Abstractions;
using Lacuna.Space.Infrastructure.Authentication;
using Lacuna.Space.Infrastructure.Clients;
using Lacuna.Space.Infrastructure.Services;
using Lacuna.Space.Infrastructure.Validation;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

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

        return services;
    }

    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
        services.AddSwaggerGen();

        return services;
    }

    public static IServiceCollection AddLumaApiVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),
                new QueryStringApiVersionReader("version"),
                new HeaderApiVersionReader("X-Version"),
                new MediaTypeApiVersionReader("ver")
            );
        }).AddApiExplorer(setup =>
        {
            setup.GroupNameFormat = "'v'VVV";
            setup.SubstituteApiVersionInUrl = true;
        });

        return services;
    }
}