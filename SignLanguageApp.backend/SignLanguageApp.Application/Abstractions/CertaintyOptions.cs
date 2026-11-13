using SignLanguageApp.Domain.ValueObjects;

namespace SignLanguageApp.Application.Abstractions;

/// <summary>Sección "Certainty" de appsettings.json: umbrales del semáforo (RF-06).</summary>
public sealed class CertaintyOptions
{
    public const string SectionName = "Certainty";

    public double High { get; set; } = 80;
    public double Medium { get; set; } = 60;
    /// <summary>Segundos para confirmar una palabra amarilla antes de marcarla "no confirmada" (RF-08).</summary>
    public int ConfirmationSeconds { get; set; } = 10;

    public CertaintyThresholds ToThresholds() => new(High, Medium);
}
