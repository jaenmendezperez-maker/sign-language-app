using SignLanguageApp.Application.DTOs;
using SignLanguageApp.Application.Interfaces;
using SignLanguageApp.Application.UseCases;
using SignLanguageApp.Application.Validators;
using SignLanguageApp.Domain.Entities;
using SignLanguageApp.Domain.Enums;
using SignLanguageApp.Domain.Exceptions;
using SignLanguageApp.Domain.ValueObjects;
using SignLanguageApp.Infrastructure.ModelRegistry;

namespace SignLanguageApp.Tests.Application;

public class TranslateSignLanguageUseCaseTests
{
    private readonly InMemoryRepository _repository = new();
    private readonly TranslateSignLanguageUseCase _useCase;

    public TranslateSignLanguageUseCaseTests()
    {
        var factory = new TranslationModelFactory([new FakeModel()]);
        _useCase = new TranslateSignLanguageUseCase(factory, _repository);
    }

    [Fact]
    public async Task Translates_and_saves_the_result()
    {
        var output = await _useCase.ExecuteAsync(Command(sessionId: null));

        Assert.Equal("hola", output.Text);
        Assert.Equal(new[] { "HOLA" }, output.Glosses);
        Assert.Single(_repository.Sessions);
        Assert.Equal(1, _repository.SaveCount);
    }

    [Fact]
    public async Task Reuses_the_session_when_sessionId_is_sent()
    {
        var first = await _useCase.ExecuteAsync(Command(sessionId: null));
        var second = await _useCase.ExecuteAsync(Command(sessionId: first.SessionId));

        Assert.Equal(first.SessionId, second.SessionId);
        Assert.Single(_repository.Sessions);
        Assert.Equal(2, _repository.Sessions[0].Requests.Count);
    }

    [Fact]
    public async Task Rejects_frames_with_wrong_shape()
    {
        var bad = new TranslateCommand(null, Language.LSM, Language.SpanishText, [[[0f, 0f, 0f]]]);

        await Assert.ThrowsAsync<RequestValidationException>(() => _useCase.ExecuteAsync(bad));
    }

    [Fact]
    public async Task Rejects_pairs_without_a_model()
    {
        var cmd = Command(sessionId: null) with { Target = Language.LSU };

        await Assert.ThrowsAsync<UnsupportedLanguagePairException>(() => _useCase.ExecuteAsync(cmd));
    }

    // ---------- helpers ----------

    private static TranslateCommand Command(Guid? sessionId)
    {
        var frames = Enumerable.Range(0, 30)
            .Select(_ => Enumerable.Range(0, TranslateCommandValidator.PointsPerFrame)
                .Select(_ => new[] { 0.5f, 0.5f, 0f })
                .ToArray())
            .ToArray();

        return new TranslateCommand(sessionId, Language.LSM, Language.SpanishText, frames);
    }

    private sealed class FakeModel : ITranslationModel
    {
        public LanguagePair SupportedPair { get; } = new(Language.LSM, Language.SpanishText);

        public Task<TranslationModelResult> TranslateAsync(TranslationModelRequest request, CancellationToken ct = default) =>
            Task.FromResult(new TranslationModelResult("hola", ["HOLA"], 0.9));
    }

    private sealed class InMemoryRepository : ITranslationRepository
    {
        public List<TranslationSession> Sessions { get; } = [];
        public int SaveCount { get; private set; }

        public Task<TranslationSession?> GetSessionAsync(Guid sessionId, CancellationToken ct = default) =>
            Task.FromResult(Sessions.FirstOrDefault(s => s.Id == sessionId));

        public Task AddSessionAsync(TranslationSession session, CancellationToken ct = default)
        {
            Sessions.Add(session);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<TranslationHistoryItem>> GetHistoryAsync(int take, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<TranslationHistoryItem>>([]);

        public Task SaveChangesAsync(CancellationToken ct = default)
        {
            SaveCount++;
            return Task.CompletedTask;
        }
    }
}
