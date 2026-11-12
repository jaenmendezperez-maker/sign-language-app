using Microsoft.AspNetCore.SignalR;
using SignLanguageApp.API.DTOs;
using SignLanguageApp.Application.UseCases;
using SignLanguageApp.Application.Validators;
using SignLanguageApp.Domain.Exceptions;

namespace SignLanguageApp.API.Hubs;

/// <summary>
/// Canal en tiempo real. El cliente invoca "SendFrames" y recibe el evento "TranslationReceived".
/// Ruta: /hubs/translation
/// </summary>
public sealed class TranslationHub(
    TranslateSignLanguageUseCase translate,
    ILogger<TranslationHub> logger) : Hub
{
    public const string TranslationReceived = "TranslationReceived";

    public async Task SendFrames(TranslationRequestDto request)
    {
        try
        {
            var output = await translate.ExecuteAsync(request.ToCommand(), Context.ConnectionAborted);
            await Clients.Caller.SendAsync(TranslationReceived, output.ToDto(), Context.ConnectionAborted);
        }
        // El middleware de excepciones no aplica a SignalR: los errores se devuelven como HubException,
        // que el cliente recibe como un error en la promesa de invoke().
        catch (Exception ex) when (ex is RequestValidationException or UnsupportedLanguagePairException)
        {
            throw new HubException(ex.Message);
        }
        catch (Exception ex) when (ex is HttpRequestException
                                   || (ex is TaskCanceledException && !Context.ConnectionAborted.IsCancellationRequested))
        {
            logger.LogError(ex, "No se pudo contactar a ml-service");
            throw new HubException("El servicio de modelos no está disponible.");
        }
    }
}
