namespace SignLanguageApp.Infrastructure.Security;

/// <summary>Sección "Jwt" de appsettings. La clave solo está en appsettings.Development.json; en otros entornos va por variable de entorno (Jwt__Key).</summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "SignLink";
    public string Audience { get; set; } = "SignLink";
    public string Key { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 480; // un turno
}
