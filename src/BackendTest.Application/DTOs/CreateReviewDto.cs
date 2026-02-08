namespace BackendTest.Application.DTOs;

/// <summary>
/// DTO para crear una nueva reseña
/// </summary>
public record CreateReviewDto
{
    /// <summary>
    /// Nombre del revisor (requerido)
    /// </summary>
    public required string ReviewerName { get; init; }

    /// <summary>
    /// Comentario o reseña escrita (requerido, máximo 1000 caracteres)
    /// </summary>
    public required string Comment { get; init; }

    /// <summary>
    /// Calificación de la película (requerido, entre 1 y 5)
    /// </summary>
    public required int Rating { get; init; }
}
