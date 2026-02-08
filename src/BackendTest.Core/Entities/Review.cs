namespace BackendTest.Core.Entities;

/// <summary>
/// Entidad que representa una reseña de una película
/// </summary>
public class Review : BaseEntity
{
    /// <summary>
    /// Identificador de la película asociada
    /// </summary>
    public Guid MovieId { get; set; }

    /// <summary>
    /// Nombre del revisor
    /// </summary>
    public string ReviewerName { get; set; } = string.Empty;

    /// <summary>
    /// Comentario o reseña escrita
    /// </summary>
    public string Comment { get; set; } = string.Empty;

    /// <summary>
    /// Calificación otorgada (entre 1 y 5)
    /// </summary>
    public int Rating { get; set; }

    /// <summary>
    /// Película asociada a esta reseña
    /// </summary>
    public Movie Movie { get; set; } = null!;

    /// <summary>
    /// Establece la calificación validando que esté en el rango permitido
    /// </summary>
    /// <param name="rating">Calificación a establecer</param>
    /// <exception cref="ArgumentException">Cuando la calificación no está entre 1 y 5</exception>
    public void SetRating(int rating)
    {
        if (rating < 1 || rating > 5)
        {
            throw new ArgumentException("La calificación debe estar entre 1 y 5.", nameof(rating));
        }
        Rating = rating;
    }
}
