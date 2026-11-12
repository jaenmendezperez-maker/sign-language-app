using Microsoft.AspNetCore.Mvc;
using SignLanguageApp.API.DTOs;
using SignLanguageApp.Application.UseCases;

namespace SignLanguageApp.API.Controllers;

[ApiController]
[Route("api/translation")]
public sealed class TranslationController(
    TranslateSignLanguageUseCase translate,
    GetTranslationHistoryUseCase getHistory) : ControllerBase
{
    /// <summary>Traducción puntual (sin tiempo real). Útil para probar desde Swagger o el .http.</summary>
    [HttpPost]
    public async Task<ActionResult<TranslationResponseDto>> Translate(TranslationRequestDto request, CancellationToken ct)
    {
        var output = await translate.ExecuteAsync(request.ToCommand(), ct);
        return Ok(output.ToDto());
    }

    /// <summary>Últimas traducciones guardadas.</summary>
    [HttpGet("history")]
    public async Task<ActionResult<IEnumerable<HistoryItemDto>>> History([FromQuery] int take = 50, CancellationToken ct = default)
    {
        var items = await getHistory.ExecuteAsync(take, ct);
        return Ok(items.Select(i => i.ToDto()));
    }
}
