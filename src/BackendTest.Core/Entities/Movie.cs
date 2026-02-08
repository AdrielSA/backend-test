namespace BackendTest.Core.Entities;

/// <summary>
/// Entidad que representa una película en el sistema
/// </summary>
public class Movie : BaseEntity
{
    /// <summary>
    /// Título de la película
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Descripción o sinopsis de la película
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Año de lanzamiento de la película
    /// </summary>
    public int ReleaseYear { get; set; }

    /// <summary>
    /// Género cinematográfico
    /// </summary>
    public string Genre { get; set; } = string.Empty;

    /// <summary>
    /// Director de la película
    /// </summary>
    public string Director { get; set; } = string.Empty;

    /// <summary>
    /// Duración en minutos
    /// </summary>
    public int? DurationMinutes { get; set; }

    /// <summary>
    /// Indica si la película está activa (no eliminada mediante soft delete)
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Colección de reseñas asociadas a esta película
    /// </summary>
    public ICollection<Review> Reviews { get; set; } = [];

    /// <summary>
    /// Desactiva la película mediante soft delete
    /// </summary>
    public void Disable()
    {
        IsActive = false;
    }

    /// <summary>
    /// Agrega una reseña a la película si está activa
    /// </summary>
    /// <param name="review">Reseña a agregar</param>
    /// <exception cref="InvalidOperationException">Cuando la película está inactiva</exception>
    public void AddReview(Review review)
    {
        if (!IsActive)
        {
            throw new InvalidOperationException("No se pueden agregar reseñas a una película inactiva.");
        }

        Reviews.Add(review);
    }

    /// <summary>
    /// Calcula la calificación promedio de todas las reseñas de la película
    /// </summary>
    /// <returns>Promedio de calificaciones o 0 si no hay reseñas</returns>
    public double CalculateAverageRating()
    {
        if (Reviews.Count == 0)
        {
            return 0;
        }

        return Reviews.Average(r => r.Rating);
    }
}
