using SignLanguageApp.Application.Abstractions;
using SignLanguageApp.Application.DTOs;

namespace SignLanguageApp.Application.UseCases.Catalog;

/// <summary>
/// CU-12 Consultar vocabulario. El navegador usa este mismo catálogo para el reconocimiento
/// (RF-10: el catálogo sale de las mismas referencias que usa el algoritmo).
/// </summary>
public sealed class GetCatalogUseCase(ICatalogRepository catalog)
{
    public async Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken ct = default) =>
        (await catalog.GetCategoriesAsync(ct)).Select(c => c.ToDto()).ToList();

    public async Task<IReadOnlyList<SignDto>> GetSignsAsync(int? categoryId, CancellationToken ct = default) =>
        (await catalog.GetActiveSignsAsync(categoryId, ct)).Select(s => s.ToDto()).ToList();
}
