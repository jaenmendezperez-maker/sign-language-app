using SignLanguageApp.Application.DTOs;
using SignLanguageApp.Application.Interfaces;
using SignLanguageApp.Application.Mappings;
using SignLanguageApp.Application.Validators;
using SignLanguageApp.Domain.Entities;
using SignLanguageApp.Domain.Enums;
using SignLanguageApp.Domain.ValueObjects;

namespace SignLanguageApp.Application.UseCases;

public sealed class TranslateSignLanguageUseCase(
    ITranslationModelFactory modelFactory,
    ITranslationRepository repository)
{
    public async Task<TranslationOutput> ExecuteAsync(TranslateCommand command, CancellationToken ct = default)
    {
        TranslateCommandValidator.Validate(command);

        var pair = LanguagePair.Create(command.Source, command.Target);
        var model = modelFactory.Resolve(pair);

        var session = await GetOrStartSessionAsync(command.SessionId, pair, ct);
        var request = session.AddRequest(InputType.Landmarks, command.Frames.Length);

        var modelResult = await model.TranslateAsync(new TranslationModelRequest(command.Frames), ct);

        var result = request.Complete(modelResult.Text, modelResult.Glosses, modelResult.Confidence);
        await repository.SaveChangesAsync(ct);

        return result.ToOutput(session.Id);
    }

    /// <summary>
    /// Reutiliza la sesión si existe y es del mismo par; si el usuario cambió de idioma
    /// o no mandó sessionId, empieza una nueva.
    /// </summary>
    private async Task<TranslationSession> GetOrStartSessionAsync(Guid? sessionId, LanguagePair pair, CancellationToken ct)
    {
        if (sessionId is { } id)
        {
            var existing = await repository.GetSessionAsync(id, ct);
            if (existing is not null && existing.Pair == pair)
                return existing;
        }

        var session = TranslationSession.Start(pair);
        await repository.AddSessionAsync(session, ct);
        return session;
    }
}
