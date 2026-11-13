using SignLanguageApp.Domain.Enums;

namespace SignLanguageApp.Domain.Entities;

/// <summary>USUARIO: personal médico o administrador.</summary>
public class User
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private User() { } // EF Core

    public static User Create(string name, string email, string passwordHash, UserRole role, DateTime now) => new()
    {
        Id = Guid.NewGuid(),
        Name = name.Trim(),
        Email = NormalizeEmail(email),
        PasswordHash = passwordHash,
        Role = role,
        IsActive = true,
        CreatedAt = now,
    };

    public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    public void ChangePasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("El hash no puede estar vacío.");
        PasswordHash = passwordHash;
    }

    public void Deactivate() => IsActive = false;
}
