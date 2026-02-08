using BackendTest.Core.Enums;

namespace BackendTest.Core.ValueObjects;

/// <summary>
/// Value Object que encapsula los criterios de filtrado, ordenamiento y paginación para películas
/// </summary>
public record MovieFilterCriteria
{
    /// <summary>
    /// Término de búsqueda genérica aplicado a título, director, género y descripción
    /// </summary>
    public string? Search { get; init; }

    /// <summary>
    /// Campo por el cual ordenar los resultados
    /// </summary>
    public MovieSortBy SortBy { get; init; } = MovieSortBy.CreatedAt;

    /// <summary>
    /// Indica si el ordenamiento es descendente
    /// </summary>
    public bool IsDescending { get; init; }

    /// <summary>
    /// Número de página
    /// </summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>
    /// Cantidad de elementos por página
    /// </summary>
    public int PageSize { get; init; } = 10;
}
