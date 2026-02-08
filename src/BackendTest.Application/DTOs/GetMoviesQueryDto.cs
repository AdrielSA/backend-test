using BackendTest.Core.Enums;

namespace BackendTest.Application.DTOs;

/// <summary>
/// Query DTO para obtener películas con filtros, ordenamiento y paginación
/// </summary>
public record GetMoviesQueryDto
{
    /// <summary>
    /// Búsqueda genérica en título, director, género y descripción
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
    /// Número de página (comienza en 1)
    /// </summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>
    /// Tamaño de página (máximo 100)
    /// </summary>
    public int PageSize { get; init; } = 10;
}
