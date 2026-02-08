using BackendTest.Core.Entities;
using BackendTest.Core.Interfaces;
using BackendTest.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BackendTest.Infrastructure.Repositories;

public class ReviewRepository(ApplicationDbContext context) : Repository<Review>(context), IReviewRepository
{

    /// <summary>
    /// Obtiene todas las reseñas de una película ordenadas por fecha de creación descendente
    /// </summary>
    /// <param name="movieId">Identificador de la película</param>
    /// <returns>Colección de reseñas ordenadas por fecha más reciente primero</returns>
    public async Task<IEnumerable<Review>> GetByMovieIdAsync(Guid movieId)
    {
        return await _dbSet
            .Where(r => r.MovieId == movieId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Calcula la calificación promedio de todas las reseñas de una película
    /// </summary>
    /// <param name="movieId">Identificador de la película</param>
    /// <returns>Promedio de calificaciones o 0 si no hay reseñas</returns>
    public async Task<double> GetAverageRatingByMovieIdAsync(Guid movieId)
    {
        var reviews = await _dbSet.Where(r => r.MovieId == movieId).ToListAsync();

        if (reviews.Count == 0)
        {
            return 0;
        }

        return reviews.Average(r => r.Rating);
    }
}
