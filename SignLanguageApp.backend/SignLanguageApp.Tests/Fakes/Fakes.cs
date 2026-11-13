using Microsoft.Extensions.Options;
using SignLanguageApp.Application.Abstractions;
using SignLanguageApp.Domain.Entities;
using SignLanguageApp.Domain.ValueObjects;

namespace SignLanguageApp.Tests.Fakes;

/// <summary>Repositorios en memoria para probar los casos de uso sin base de datos.</summary>
public sealed class InMemoryStore : IUserRepository, ICatalogRepository, IConsultationRepository, IUnitOfWork
{
    public List<User> Users { get; } = [];
    public List<Category> Categories { get; } = [];
    public List<Sign> Signs { get; } = [];
    public List<Consultation> Consultations { get; } = [];
    public int SaveCount { get; private set; }

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        Task.FromResult(Users.FirstOrDefault(u => u.Email == email));
    public Task AddAsync(User user, CancellationToken ct = default) { Users.Add(user); return Task.CompletedTask; }

    public Task<IReadOnlyList<Category>> GetCategoriesAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<Category>>(Categories);
    public Task<Category?> GetCategoryAsync(int id, CancellationToken ct = default) =>
        Task.FromResult(Categories.FirstOrDefault(c => c.Id == id));
    public Task AddCategoryAsync(Category category, CancellationToken ct = default) { Categories.Add(category); return Task.CompletedTask; }
    public Task<IReadOnlyList<Sign>> GetActiveSignsAsync(int? categoryId, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<Sign>>(Signs.Where(s => s.IsActive && (categoryId is null || s.CategoryId == categoryId)).ToList());
    public Task<Sign?> GetSignAsync(Guid id, CancellationToken ct = default) => Task.FromResult(Signs.FirstOrDefault(s => s.Id == id));
    public Task<Sign?> GetSignByWordAsync(string word, CancellationToken ct = default) => Task.FromResult(Signs.FirstOrDefault(s => s.Word == word));
    public Task AddSignAsync(Sign sign, CancellationToken ct = default) { Signs.Add(sign); return Task.CompletedTask; }

    public Task<Consultation?> GetAsync(Guid id, CancellationToken ct = default) => Task.FromResult(Consultations.FirstOrDefault(c => c.Id == id));
    public Task<IReadOnlyList<Consultation>> ListByDoctorAsync(Guid doctorId, int take, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<Consultation>>(Consultations.Where(c => c.DoctorId == doctorId).OrderByDescending(c => c.StartedAt).Take(take).ToList());
    public Task AddAsync(Consultation consultation, CancellationToken ct = default) { Consultations.Add(consultation); return Task.CompletedTask; }

    public Task SaveChangesAsync(CancellationToken ct = default) { SaveCount++; return Task.CompletedTask; }
}

public sealed class FakeHasher : IPasswordHasher
{
    public string Hash(User user, string password) => "hash:" + password;
    public bool Verify(User user, string password) => user.PasswordHash == "hash:" + password;
}

public sealed class FakeTokens : ITokenService
{
    public (string Token, DateTime ExpiresAt) CreateToken(User user) => ($"token-{user.Id}", DateTime.UtcNow.AddHours(1));
}

public sealed class FixedClock(DateTime utcNow) : TimeProvider
{
    public DateTime Now { get; set; } = utcNow;
    public override DateTimeOffset GetUtcNow() => new(Now, TimeSpan.Zero);
}

public static class TestData
{
    public static readonly DateTime Now = new(2026, 11, 12, 18, 0, 0, DateTimeKind.Utc);

    public static IOptions<CertaintyOptions> Certainty() => Options.Create(new CertaintyOptions());

    public static double[] Vector(int length, double value = 0.5) => Enumerable.Repeat(value, length).ToArray();

    /// <summary>Seña de una mano con una referencia válida.</summary>
    public static Sign SignWithReference(string word = "dolor", int categoryId = 1)
    {
        var sign = Sign.Create(word, categoryId, 1, null, Guid.NewGuid(), Now);
        sign.AddReference(
            Vector(HandGeometry.DistancesPerHand), Vector(HandGeometry.AnglesPerHand, 90),
            Vector(HandGeometry.CoordinatesPerHand), 5, Guid.NewGuid(), Now);
        return sign;
    }
}
