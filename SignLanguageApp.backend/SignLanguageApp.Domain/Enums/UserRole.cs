namespace SignLanguageApp.Domain.Enums;

/// <summary>Rol del usuario que inicia sesión. El paciente no es usuario del sistema (RNF-06).</summary>
public enum UserRole
{
    Administrator, // registra y mantiene las referencias geométricas (RF-11)
    MedicalStaff,  // inicia y controla las consultas
}
