using Microsoft.EntityFrameworkCore;
using SignLanguageApp.Application.Abstractions;
using SignLanguageApp.Domain.Entities;

namespace SignLanguageApp.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(AppDbContext db) : IUserRepository
{
    public Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken ct = default) =>
        db.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail, ct);

    public async Task AddAsync(User user, CancellationToken ct = default) => await db.Users.AddAsync(user, ct);
}

public sealed class CatalogRepository(AppDbContext db) : ICatalogRepository
{
    public async Task<IReadOnlyList<Category>> GetCategoriesAsync(CancellationToken ct = default) =>
        await db.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync(ct);

    public Task<Category?> GetCategoryAsync(int id, CancellationToken ct = default) =>
        db.Categories.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task AddCategoryAsync(Category category, CancellationToken ct = default) =>
        await db.Categories.AddAsync(category, ct);

    public async Task<IReadOnlyList<Sign>> GetActiveSignsAsync(int? categoryId, CancellationToken ct = default)
    {
        var query = db.Signs.AsNoTracking().Include(s => s.Category).Include(s => s.References).Where(s => s.IsActive);
        if (categoryId is { } id) query = query.Where(s => s.CategoryId == id);
        return await query.OrderBy(s => s.Word).AsSplitQuery().ToListAsync(ct);
    }

    public Task<Sign?> GetSignAsync(Guid id, CancellationToken ct = default) =>
        db.Signs.Include(s => s.Category).Include(s => s.References).FirstOrDefaultAsync(s => s.Id == id, ct);

    public Task<Sign?> GetSignByWordAsync(string normalizedWord, CancellationToken ct = default) =>
        db.Signs.Include(s => s.Category).Include(s => s.References).FirstOrDefaultAsync(s => s.Word == normalizedWord, ct);

    public async Task AddSignAsync(Sign sign, CancellationToken ct = default) => await db.Signs.AddAsync(sign, ct);
}

public sealed class ConsultationRepository(AppDbContext db) : IConsultationRepository
{
    public Task<Consultation?> GetAsync(Guid id, CancellationToken ct = default) =>
        db.Consultations.Include(c => c.Records).FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyList<Consultation>> ListByDoctorAsync(Guid doctorId, int take, CancellationToken ct = default) =>
        await db.Consultations.AsNoTracking().Include(c => c.Records)
            .Where(c => c.DoctorId == doctorId)
            .OrderByDescending(c => c.StartedAt)
            .Take(take)
            .AsSplitQuery()
            .ToListAsync(ct);

    public async Task AddAsync(Consultation consultation, CancellationToken ct = default) =>
        await db.Consultations.AddAsync(consultation, ct);
}
