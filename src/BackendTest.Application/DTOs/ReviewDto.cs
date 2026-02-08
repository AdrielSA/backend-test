namespace BackendTest.Application.DTOs;

/// <summary>
/// DTO de respuesta que representa una reseña
/// </summary>
public record ReviewDto
{
    /// <summary>
    /// Identificador de la reseña
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Identificador de la película asociada
    /// </summary>
    public Guid MovieId { get; init; }

    /// <summary>
    /// Nombre del revisor
    /// </summary>
    public string ReviewerName { get; init; } = string.Empty;

    /// <summary>
    /// Comentario o reseña escrita
    /// </summary>
    public string Comment { get; init; } = string.Empty;

    /// <summary>
    /// Calificación otorgada (entre 1 y 5)
    /// </summary>
    public int Rating { get; init; }

    /// <summary>
    /// Fecha y hora de creación de la reseña
    /// </summary>
    public DateTime CreatedAt { get; init; }
}
