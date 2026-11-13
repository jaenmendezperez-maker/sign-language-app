using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;
using SignLanguageApp.Domain.Enums;

namespace SignLanguageApp.API.Auth;

public static class Roles
{
    public const string Administrator = nameof(UserRole.Administrator);
    public const string MedicalStaff = nameof(UserRole.MedicalStaff);
}

public static class ClaimsPrincipalExtensions
{
    /// <summary>Id del usuario autenticado (claim "sub" del token).</summary>
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id) ? id : throw new UnauthorizedAccessException();
    }
}
