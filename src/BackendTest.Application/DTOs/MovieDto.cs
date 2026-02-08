namespace BackendTest.Application.DTOs;

/// <summary>
/// DTO de respuesta que representa una película
/// </summary>
public record MovieDto
{
    /// <summary>
    /// Identificador de la película
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Título de la película
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Descripción o sinopsis de la película
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Año de lanzamiento de la película
    /// </summary>
    public int ReleaseYear { get; init; }

    /// <summary>
    /// Género cinematográfico
    /// </summary>
    public string Genre { get; init; } = string.Empty;

    /// <summary>
    /// Director de la película
    /// </summary>
    public string Director { get; init; } = string.Empty;

    /// <summary>
    /// Duración en minutos
    /// </summary>
    public int? DurationMinutes { get; init; }

    /// <summary>
    /// Indica si la película está activa (no eliminada)
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// Calificación promedio calculada de todas las reseñas
    /// </summary>
    public double AverageRating { get; init; }

    /// <summary>
    /// Fecha y hora de creación del registro
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Fecha y hora de última actualización del registro
    /// </summary>
    public DateTime UpdatedAt { get; init; }
}
