namespace SignLanguageApp.Infrastructure.ModelClients;

/// <summary>Sección "MlService" de appsettings.json.</summary>
public sealed class MlServiceOptions
{
    public const string SectionName = "MlService";

    public string BaseUrl { get; set; } = "http://localhost:8000";
    public int TimeoutSeconds { get; set; } = 10;
}
