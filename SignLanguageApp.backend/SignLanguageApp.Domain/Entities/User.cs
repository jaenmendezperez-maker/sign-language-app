namespace SignLanguageApp.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    private User() { } // EF Core

    public static User Create(string email, string passwordHash) => new()
    {
        Id = Guid.NewGuid(),
        Email = email.Trim().ToLowerInvariant(),
        PasswordHash = passwordHash,
        CreatedAt = DateTime.UtcNow,
    };
}
