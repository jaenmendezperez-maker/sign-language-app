using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SignLanguageApp.API.Auth;
using SignLanguageApp.Application.DTOs;
using SignLanguageApp.Application.UseCases.Consultations;

namespace SignLanguageApp.API.Controllers;

/// <summary>Consultas del médico autenticado. Una consulta de otro médico responde 404.</summary>
[ApiController]
[Route("api/consultations")]
[Authorize(Roles = Roles.MedicalStaff)]
public sealed class ConsultationsController(
    StartConsultationUseCase start,
    PauseResumeConsultationUseCase pauseResume,
    FinishConsultationUseCase finish,
    RecordTranslationUseCase record,
    ReviewTranslationUseCase review,
    ConsultationHistoryUseCase history,
    ExportSummaryUseCase export,
    IConfiguration configuration) : ControllerBase
{
    private Guid DoctorId => User.GetUserId();

    /// <summary>CU-02 Iniciar consulta.</summary>
    [HttpPost]
    public async Task<ActionResult<ConsultationDto>> Start(CancellationToken ct)
    {
        var consultation = await start.ExecuteAsync(DoctorId, ct);
        return CreatedAtAction(nameof(Get), new { id = consultation.Id }, consultation);
    }

    /// <summary>CU-10 Mis consultas (más recientes primero).</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ConsultationSummaryDto>>> List([FromQuery] int take = 50, CancellationToken ct = default) =>
        Ok(await history.ListAsync(DoctorId, take, ct));

    /// <summary>CU-10 Consulta con su historial cronológico.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ConsultationDto>> Get(Guid id, CancellationToken ct) =>
        Ok(await history.GetAsync(id, DoctorId, ct));

    /// <summary>CU-03 Pausar.</summary>
    [HttpPost("{id:guid}/pause")]
    public async Task<ActionResult<ConsultationDto>> Pause(Guid id, CancellationToken ct) =>
        Ok(await pauseResume.PauseAsync(id, DoctorId, ct));

    /// <summary>CU-03 Reanudar.</summary>
    [HttpPost("{id:guid}/resume")]
    public async Task<ActionResult<ConsultationDto>> Resume(Guid id, CancellationToken ct) =>
        Ok(await pauseResume.ResumeAsync(id, DoctorId, ct));

    /// <summary>CU-04 Finalizar.</summary>
    [HttpPost("{id:guid}/finish")]
    public async Task<ActionResult<ConsultationDto>> Finish(Guid id, CancellationToken ct) =>
        Ok(await finish.ExecuteAsync(id, DoctorId, ct));

    /// <summary>CU-05/CU-06 Registrar el resultado de una seña completada.</summary>
    [HttpPost("{id:guid}/records")]
    public async Task<ActionResult<RecordDto>> Record(Guid id, RecordTranslationCommand command, CancellationToken ct) =>
        Ok(await record.ExecuteAsync(id, DoctorId, command, ct));

    /// <summary>CU-07 Confirmar palabra con certeza media.</summary>
    [HttpPost("{id:guid}/records/{recordId:guid}/confirm")]
    public async Task<ActionResult<RecordDto>> Confirm(Guid id, Guid recordId, CancellationToken ct) =>
        Ok(await review.ConfirmAsync(id, recordId, DoctorId, ct));

    /// <summary>CU-07 Descartar palabra con certeza media.</summary>
    [HttpPost("{id:guid}/records/{recordId:guid}/discard")]
    public async Task<ActionResult<RecordDto>> Discard(Guid id, Guid recordId, CancellationToken ct) =>
        Ok(await review.DiscardAsync(id, recordId, DoctorId, ct));

    /// <summary>CU-07 Pasó el tiempo de confirmación sin decisión.</summary>
    [HttpPost("{id:guid}/records/{recordId:guid}/not-confirmed")]
    public async Task<ActionResult<RecordDto>> NotConfirmed(Guid id, Guid recordId, CancellationToken ct) =>
        Ok(await review.MarkNotConfirmedAsync(id, recordId, DoctorId, ct));

    /// <summary>CU-10 Limpiar historial.</summary>
    [HttpDelete("{id:guid}/records")]
    public async Task<ActionResult<ConsultationDto>> Clear(Guid id, CancellationToken ct) =>
        Ok(await history.ClearAsync(id, DoctorId, ct));

    /// <summary>CU-11 Exportar resumen como .txt.</summary>
    [HttpGet("{id:guid}/summary")]
    public async Task<IActionResult> Summary(Guid id, CancellationToken ct)
    {
        var (fileName, content) = await export.ExecuteAsync(id, DoctorId, TimeZone(), ct);
        return File(Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(content)).ToArray(), "text/plain; charset=utf-8", fileName);
    }

    /// <summary>Zona horaria para las horas del resumen ("TimeZone" en appsettings).</summary>
    private TimeZoneInfo TimeZone() =>
        TimeZoneInfo.TryFindSystemTimeZoneById(configuration["TimeZone"] ?? "America/Mexico_City", out var tz) ? tz : TimeZoneInfo.Utc;
}
