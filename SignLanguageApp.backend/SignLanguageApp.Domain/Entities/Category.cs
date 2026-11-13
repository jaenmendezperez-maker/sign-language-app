namespace SignLanguageApp.Domain.Entities;

/// <summary>CATEGORIA: síntomas, urgencias, respuestas, partes del cuerpo, lugares…</summary>
public class Category
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    private Category() { } // EF Core

    public static Category Create(string name, string? description = null) => new()
    {
        Name = name.Trim(),
        Description = description,
    };
}
