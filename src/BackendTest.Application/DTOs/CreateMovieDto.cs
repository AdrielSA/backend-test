namespace BackendTest.Application.DTOs;

/// <summary>
/// DTO para crear una nueva película
/// </summary>
public record CreateMovieDto
{
    /// <summary>
    /// Título de la película (requerido, máximo 200 caracteres)
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Descripción o sinopsis de la película (opcional, máximo 2000 caracteres)
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Año de lanzamiento de la película (entre 1888 y año actual)
    /// </summary>
    public required int ReleaseYear { get; init; }

    /// <summary>
    /// Género cinematográfico (requerido)
    /// </summary>
    public required string Genre { get; init; }

    /// <summary>
    /// Director de la película (requerido)
    /// </summary>
    public required string Director { get; init; }

    /// <summary>
    /// Duración en minutos (opcional, debe ser mayor a 0)
    /// </summary>
    public int? DurationMinutes { get; init; }
}
