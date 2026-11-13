using Microsoft.AspNetCore.Identity;
using SignLanguageApp.Application.Abstractions;
using SignLanguageApp.Domain.Entities;

namespace SignLanguageApp.Infrastructure.Security;

/// <summary>PBKDF2 con sal (el mismo algoritmo de ASP.NET Core Identity).</summary>
public sealed class PasswordHasherAdapter : IPasswordHasher
{
    private readonly PasswordHasher<User> _hasher = new();

    public string Hash(User user, string password) => _hasher.HashPassword(user, password);

    public bool Verify(User user, string password) =>
        _hasher.VerifyHashedPassword(user, user.PasswordHash, password) != PasswordVerificationResult.Failed;
}
