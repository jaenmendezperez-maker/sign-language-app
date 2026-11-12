namespace SignLanguageApp.Application.Validators;

/// <summary>La petición no cumple el formato esperado (se responde 400).</summary>
public sealed class RequestValidationException(string message) : Exception(message);
