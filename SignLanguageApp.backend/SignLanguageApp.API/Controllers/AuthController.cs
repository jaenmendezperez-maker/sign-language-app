using Microsoft.AspNetCore.Mvc;
using SignLanguageApp.Application.DTOs;
using SignLanguageApp.Application.UseCases.Auth;

namespace SignLanguageApp.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(LoginUseCase login) : ControllerBase
{
    /// <summary>CU-01 Iniciar sesión. Devuelve el token JWT con el rol del usuario.</summary>
    [HttpPost("login")]
    public async Task<ActionResult<LoginResult>> Login(LoginCommand command, CancellationToken ct) =>
        Ok(await login.ExecuteAsync(command, ct));
}
