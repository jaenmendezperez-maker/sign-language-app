namespace SignLanguageApp.Domain.Exceptions;

/// <summary>
/// Se intentó una operación que viola una regla de negocio en el estado actual
/// (por ejemplo, pausar una consulta finalizada). La API responde 409.
/// </summary>
public class DomainException(string message) : Exception(message);
