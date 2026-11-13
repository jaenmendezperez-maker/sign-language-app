using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SignLanguageApp.API.Auth;
using SignLanguageApp.Application.DTOs;
using SignLanguageApp.Application.UseCases.Catalog;

namespace SignLanguageApp.API.Controllers;

[ApiController]
[Route("api")]
public sealed class CatalogController(GetCatalogUseCase catalog, RegisterSignReferenceUseCase register) : ControllerBase
{
    /// <summary>Categorías del vocabulario.</summary>
    [HttpGet("categories")]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> Categories(CancellationToken ct) =>
        Ok(await catalog.GetCategoriesAsync(ct));

    /// <summary>CU-12 Consultar vocabulario (sin login). Incluye las referencias que usa el reconocedor del navegador.</summary>
    [HttpGet("catalog")]
    public async Task<ActionResult<IEnumerable<SignDto>>> Catalog([FromQuery] int? categoryId, CancellationToken ct) =>
        Ok(await catalog.GetSignsAsync(categoryId, ct));

    /// <summary>CU-13 Registrar referencia geométrica (solo administrador).</summary>
    [HttpPost("catalog/references")]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<ActionResult<SignDto>> Register(RegisterReferenceCommand command, CancellationToken ct)
    {
        var sign = await register.ExecuteAsync(command, User.GetUserId(), ct);
        return Created($"/api/catalog?categoryId={sign.CategoryId}", sign);
    }
}
