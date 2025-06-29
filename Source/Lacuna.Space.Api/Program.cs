using Lacuna.Space.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Usar extensions para organizar configurações
builder.Services.AddLumaServices(builder.Configuration);
builder.Services.AddSwaggerDocumentation();

// Configurar logging
builder.Logging.AddConsole().AddSimpleConsole(options =>
{
    options.ColorBehavior = Microsoft.Extensions.Logging.Console.LoggerColorBehavior.Enabled;
    options.IncludeScopes = true;
    options.SingleLine = true;
});

var app = builder.Build();

// Logging de inicialização
var lumaBaseUrl = builder.Configuration["LumaApi:BaseUrl"] ?? "https://luma.lacuna.cc/";
app.Logger.LogInformation("API de sincronização do relógio espacial Lacuna iniciando...");
app.Logger.LogInformation("URL base da API: {BaseUrl}", lumaBaseUrl);

// Configurar Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Lacuna Space API v1");
        c.RoutePrefix = "swagger";
        c.DocumentTitle = "Lacuna Space API";
    });
}

// Mapear endpoints usando extension
app.MapSyncEndpoints();

app.Run();