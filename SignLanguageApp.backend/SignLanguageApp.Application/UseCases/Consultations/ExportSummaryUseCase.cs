using System.Globalization;
using System.Text;
using SignLanguageApp.Application.Abstractions;
using SignLanguageApp.Domain.Entities;
using SignLanguageApp.Domain.Enums;

namespace SignLanguageApp.Application.UseCases.Consultations;

/// <summary>
/// CU-11 Exportar resumen (RF-13): fecha, hora de inicio y fin, y la lista de palabras con su
/// nivel de certeza. Solo texto: nunca incluye video ni imágenes del paciente.
/// </summary>
public sealed class ExportSummaryUseCase(IConsultationRepository consultations)
{
    public async Task<(string FileName, string Content)> ExecuteAsync(
        Guid id, Guid doctorId, TimeZoneInfo timeZone, CancellationToken ct = default)
    {
        var consultation = await consultations.GetOwnedAsync(id, doctorId, ct);
        var content = Format(consultation, timeZone);
        var fileName = $"signlink-consulta-{ToLocal(consultation.StartedAt, timeZone):yyyyMMdd-HHmm}.txt";
        return (fileName, content);
    }

    public static string Format(Consultation c, TimeZoneInfo tz)
    {
        var es = CultureInfo.GetCultureInfo("es-MX");
        var sb = new StringBuilder();
        sb.AppendLine("SignLink — Resumen de la consulta");
        sb.AppendLine(new string('=', 34));
        sb.AppendLine($"Fecha:          {ToLocal(c.StartedAt, tz).ToString("dddd d 'de' MMMM 'de' yyyy", es)}");
        sb.AppendLine($"Hora de inicio: {ToLocal(c.StartedAt, tz):HH:mm:ss}");
        sb.AppendLine($"Hora de fin:    {(c.EndedAt is { } end ? ToLocal(end, tz).ToString("HH:mm:ss") : "en curso")}");
        sb.AppendLine();

        var words = c.Records.Where(r => r.IsInPhrase).OrderBy(r => r.RecognizedAt).ToList();
        sb.AppendLine("Frase traducida:");
        sb.AppendLine(words.Count == 0 ? "  Sin palabras registradas" : "  " + string.Join(" ", words.Select(w => w.Word)));
        sb.AppendLine();

        sb.AppendLine("Palabras:");
        if (words.Count == 0) sb.AppendLine("  Sin palabras registradas");
        foreach (var w in words)
            sb.AppendLine($"  {ToLocal(w.RecognizedAt, tz):HH:mm:ss}  {w.Word,-20} {LevelLabel(w.CertaintyLevel),-6} {StatusLabel(w.Status)}");

        var discarded = c.Records.Count(r => r.Status == RecordStatus.Discarded);
        var notRecognized = c.Records.Count(r => r.Status == RecordStatus.NotRecognized);
        sb.AppendLine();
        sb.AppendLine($"Señas no reconocidas: {notRecognized} · Palabras descartadas: {discarded}");
        return sb.ToString();
    }

    private static DateTime ToLocal(DateTime utc, TimeZoneInfo tz) =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), tz);

    public static string LevelLabel(CertaintyLevel level) => level switch
    {
        CertaintyLevel.High => "Alta",
        CertaintyLevel.Medium => "Media",
        _ => "Baja",
    };

    private static string StatusLabel(RecordStatus status) => status switch
    {
        RecordStatus.Confirmed => "(confirmada)",
        RecordStatus.NotConfirmed => "(no confirmada)",
        _ => string.Empty,
    };
}
