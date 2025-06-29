using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lacuna.Space.Api.Configuration;

internal sealed class ConfigureSwaggerOptions : IConfigureNamedOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;

    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
    {
        _provider = provider;
    }

    public void Configure(SwaggerGenOptions options)
    {
        foreach (ApiVersionDescription description in _provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, CreateVersionInfo(description));
        }

        // Configurações adicionais do Swagger
        options.IncludeXmlComments(GetXmlCommentsFilePath());

        // Configurações de schema
        options.CustomSchemaIds(type => type.FullName);
        options.DescribeAllParametersInCamelCase();
    }

    public void Configure(string? name, SwaggerGenOptions options)
    {
        Configure(options);
    }

    private static OpenApiInfo CreateVersionInfo(ApiVersionDescription apiVersionDescription)
    {
        var openApiInfo = new OpenApiInfo
        {
            Title = $"Lacuna Space API v{apiVersionDescription.ApiVersion}",
            Version = apiVersionDescription.ApiVersion.ToString(),
            Description = "API para sincronização de relógios com sondas espaciais da Lacuna Space. " +
                         "Esta API fornece endpoints para gerenciar jobs de processamento, " +
                         "sincronização de relógios e orquestração de serviços espaciais.",
            Contact = new OpenApiContact
            {
                Name = "Lacuna Space Team",
                Email = "admissions@lacunasoftware.com",
                Url = new Uri("https://luma.lacuna.cc")
            },
            License = new OpenApiLicense
            {
                Name = "MIT License - PRIVATE PROJECT",
                Url = new Uri("https://github.com/William-io/Lacuna.Space")
            }
        };

        if (apiVersionDescription.IsDeprecated)
            openApiInfo.Description += " Esta versão da API foi descontinuada.";    

        return openApiInfo;
    }

    private static string GetXmlCommentsFilePath()
    {
        var basePath = AppContext.BaseDirectory;
        var fileName = typeof(Program).Assembly.GetName().Name + ".xml";
        return Path.Combine(basePath, fileName);
    }
}
