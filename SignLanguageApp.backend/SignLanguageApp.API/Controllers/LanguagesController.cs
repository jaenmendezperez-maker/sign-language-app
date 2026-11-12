using Microsoft.AspNetCore.Mvc;
using SignLanguageApp.API.DTOs;
using SignLanguageApp.Application.UseCases;

namespace SignLanguageApp.API.Controllers;

[ApiController]
[Route("api/languages")]
public sealed class LanguagesController(GetAvailableLanguagesUseCase getAvailableLanguages) : ControllerBase
{
    /// <summary>Pares de idiomas para los que hay un modelo disponible.</summary>
    [HttpGet]
    public ActionResult<IEnumerable<LanguagePairDto>> Get() =>
        Ok(getAvailableLanguages.Execute().Select(p => p.ToDto()));
}
