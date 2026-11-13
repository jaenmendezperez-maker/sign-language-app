using SignLanguageApp.Domain.Enums;

namespace SignLanguageApp.Domain.ValueObjects;

/// <summary>
/// Umbrales del semáforo de certeza (RF-06). Se leen del archivo de configuración,
/// no de la base de datos. Por defecto: Alta ≥ 80, Media ≥ 60, Baja &lt; 60.
/// </summary>
public sealed record CertaintyThresholds
{
    public double High { get; }
    public double Medium { get; }

    public CertaintyThresholds(double high, double medium)
    {
        if (medium <= 0 || high <= medium || high > 100)
            throw new ArgumentException("Los umbrales deben cumplir 0 < Medio < Alto ≤ 100.");
        High = high;
        Medium = medium;
    }

    public static CertaintyThresholds Default { get; } = new(80, 60);

    /// <summary>RN-01: nivel a partir del puntaje de similitud (0–100).</summary>
    public CertaintyLevel LevelFor(double score) =>
        score >= High ? CertaintyLevel.High
        : score >= Medium ? CertaintyLevel.Medium
        : CertaintyLevel.Low;

    /// <summary>RN-02: estado inicial de la traducción según su nivel.</summary>
    public static RecordStatus InitialStatusFor(CertaintyLevel level) => level switch
    {
        CertaintyLevel.High => RecordStatus.Accepted,
        CertaintyLevel.Medium => RecordStatus.Pending,
        _ => RecordStatus.NotRecognized,
    };
}
