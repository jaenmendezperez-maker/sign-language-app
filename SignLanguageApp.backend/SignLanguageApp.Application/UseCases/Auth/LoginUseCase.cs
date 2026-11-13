using SignLanguageApp.Application.Abstractions;
using SignLanguageApp.Application.DTOs;
using SignLanguageApp.Application.Errors;
using SignLanguageApp.Domain.Entities;

namespace SignLanguageApp.Application.UseCases.Auth;

/// <summary>CU-01 Iniciar sesión.</summary>
public sealed class LoginUseCase(IUserRepository users, IPasswordHasher hasher, ITokenService tokens)
{
    public async Task<LoginResult> ExecuteAsync(LoginCommand command, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(command.Email) || string.IsNullOrEmpty(command.Password))
            throw new RequestValidationException("Escribe tu correo y contraseña.");

        var user = await users.GetByEmailAsync(User.NormalizeEmail(command.Email), ct);

        // Mismo error para "no existe", "contraseña incorrecta" e "inactivo": no se revela cuál falló.
        if (user is null || !user.IsActive || !hasher.Verify(user, command.Password))
            throw new InvalidCredentialsException();

        var (token, expiresAt) = tokens.CreateToken(user);
        return new LoginResult(token, expiresAt, user.Name, user.Role);
    }
}
