namespace SignLanguageApp.Application.Errors;

/// <summary>La petición no cumple el formato esperado. La API responde 400.</summary>
public sealed class RequestValidationException(string message) : Exception(message);

/// <summary>El recurso no existe o no pertenece al usuario. La API responde 404.</summary>
public sealed class NotFoundException(string message) : Exception(message);

/// <summary>Credenciales incorrectas o usuario inactivo. La API responde 401.</summary>
public sealed class InvalidCredentialsException() : Exception("Correo o contraseña incorrectos.");
