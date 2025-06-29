namespace Lacuna.Space.Infrastructure.Setting;

public class LumaApiOptions
{
    public const string SectionName = "LumaApi";
    public string BaseUrl { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;
}