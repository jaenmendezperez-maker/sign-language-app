using System.Diagnostics;

namespace SignLanguageApp.API.Middlewares;

/// <summary>Registra cada petición HTTP con su código de respuesta y duración.</summary>
public sealed class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        await next(context);
        stopwatch.Stop();

        logger.LogInformation("{Method} {Path} → {StatusCode} en {Elapsed} ms",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            stopwatch.ElapsedMilliseconds);
    }
}
