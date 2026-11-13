using SignLanguageApp.Domain.Entities;

namespace SignLanguageApp.Application.Abstractions;

public interface IPasswordHasher
{
    string Hash(User user, string password);
    bool Verify(User user, string password);
}

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) CreateToken(User user);
}
