namespace SignLanguageApp.Domain.Enums;

/// <summary>Estado de una traducción registrada (RN-02, RN-03).</summary>
public enum RecordStatus
{
    Accepted,      // certeza alta: entra directo a la frase
    Pending,       // certeza media: espera que el médico confirme o descarte (RF-08)
    Confirmed,
    NotConfirmed,  // pasaron 10 s sin acción o se finalizó la consulta
    Discarded,
    NotRecognized, // certeza baja: se pide repetir la seña (RF-07)
}
