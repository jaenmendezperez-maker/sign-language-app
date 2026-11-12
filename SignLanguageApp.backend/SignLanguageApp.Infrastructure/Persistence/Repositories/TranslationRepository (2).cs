using Microsoft.EntityFrameworkCore;
using SignLanguageApp.Application.DTOs;
using SignLanguageApp.Application.Interfaces;
using SignLanguageApp.Domain.Entities;

namespace SignLanguageApp.Infrastructure.Persistence.Repositories;

public sealed class TranslationRepository(AppDbContext db) : ITranslationRepository
{
    public Task<TranslationSession?> GetSessionAsync(Guid sessionId, CancellationToken ct = default) =>
        db.Sessions.FirstOrDefaultAsync(s => s.Id == sessionId, ct);

    public async Task AddSessionAsync(TranslationSession session, CancellationToken ct = default) =>
        await db.Sessions.AddAsync(session, ct);

    public async Task<IReadOnlyList<TranslationHistoryItem>> GetHistoryAsync(int take, CancellationToken ct = default)
    {
        var query =
            from result in db.Results.AsNoTracking()
            join request in db.Requests on result.RequestId equals request.Id
            join session in db.Sessions on request.SessionId equals session.Id
            orderby result.CreatedAt descending
            select new TranslationHistoryItem(
                session.Id,
                session.Source,
                session.Target,
                result.Text,
                result.Glosses,
                result.Confidence,
                result.CreatedAt);

        return await query.Take(take).ToListAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}
