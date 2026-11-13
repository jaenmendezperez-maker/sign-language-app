using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SignLanguageApp.Application.Abstractions;

namespace SignLanguageApp.API.Controllers;

[ApiController]
[Route("api/config")]
public sealed class ConfigController(IOptions<CertaintyOptions> certainty) : ControllerBase
{
    /// <summary>
    /// Umbrales del semáforo (RF-06) desde appsettings.json. El navegador los usa para
    /// mostrar el color al instante; el servidor los vuelve a aplicar al guardar.
    /// </summary>
    [HttpGet("certainty")]
    public ActionResult<CertaintyOptions> GetCertainty() => Ok(certainty.Value);
}
