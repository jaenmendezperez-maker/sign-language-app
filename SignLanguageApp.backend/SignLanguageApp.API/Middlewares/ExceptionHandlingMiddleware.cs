using Microsoft.AspNetCore.Mvc;
using SignLanguageApp.Application.Validators;
using SignLanguageApp.Domain.Exceptions;

namespace SignLanguageApp.API.Middlewares;

/// <summary>Convierte excepciones conocidas en respuestas HTTP con formato ProblemDetails.</summary>
public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // El cliente cerró la conexión; no hay a quién responder.
        }
        catch (Exception ex)
        {
            var (status, title) = ex switch
            {
                RequestValidationException => (StatusCodes.Status400BadRequest, "Petición inválida"),
                UnsupportedLanguagePairException => (StatusCodes.Status400BadRequest, "Par de idiomas no soportado"),
                HttpRequestException => (StatusCodes.Status503ServiceUnavailable, "Servicio de modelos no disponible"),
                TaskCanceledException => (StatusCodes.Status503ServiceUnavailable, "El servicio de modelos tardó demasiado"),
                _ => (StatusCodes.Status500InternalServerError, "Error interno"),
            };

            if (status >= 500)
                logger.LogError(ex, "Error procesando {Method} {Path}", context.Request.Method, context.Request.Path);

            context.Response.StatusCode = status;
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = status == StatusCodes.Status500InternalServerError ? null : ex.Message,
            });
        }
    }
}
