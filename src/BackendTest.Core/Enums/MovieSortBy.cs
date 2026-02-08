namespace BackendTest.Core.Enums;

/// <summary>
/// Campos disponibles para ordenar películas
/// </summary>
public enum MovieSortBy
{
    /// <summary>
    /// Ordenar por título alfabéticamente
    /// </summary>
    Title,

    /// <summary>
    /// Ordenar por año de lanzamiento
    /// </summary>
    Year,

    /// <summary>
    /// Ordenar por calificación promedio
    /// </summary>
    Rating,

    /// <summary>
    /// Ordenar por fecha de creación
    /// </summary>
    CreatedAt
}
