using SignLanguageApp.Domain.Entities;

namespace SignLanguageApp.Application.Abstractions;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
}

public interface ICatalogRepository
{
    Task<IReadOnlyList<Category>> GetCategoriesAsync(CancellationToken ct = default);
    Task<Category?> GetCategoryAsync(int id, CancellationToken ct = default);
    Task AddCategoryAsync(Category category, CancellationToken ct = default);

    /// <summary>Señas activas con su categoría y referencias, opcionalmente filtradas.</summary>
    Task<IReadOnlyList<Sign>> GetActiveSignsAsync(int? categoryId, CancellationToken ct = default);
    Task<Sign?> GetSignAsync(Guid id, CancellationToken ct = default);
    Task<Sign?> GetSignByWordAsync(string normalizedWord, CancellationToken ct = default);
    Task AddSignAsync(Sign sign, CancellationToken ct = default);
}

public interface IConsultationRepository
{
    /// <summary>Consulta con sus traducciones.</summary>
    Task<Consultation?> GetAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Consultation>> ListByDoctorAsync(Guid doctorId, int take, CancellationToken ct = default);
    Task AddAsync(Consultation consultation, CancellationToken ct = default);
}

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken ct = default);
}
