using BackendTest.Core.Entities;
using BackendTest.Core.Enums;
using BackendTest.Core.Interfaces;
using BackendTest.Core.ValueObjects;
using BackendTest.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BackendTest.Infrastructure.Repositories;

public class MovieRepository(ApplicationDbContext context) : Repository<Movie>(context), IMovieRepository
{
    /// <summary>
    /// Obtiene una película por su identificador incluyendo todas sus reseñas
    /// </summary>
    /// <param name="id">Identificador de la película</param>
    /// <returns>La película con sus reseñas o null si no existe</returns>
    public async Task<Movie?> GetByIdWithReviewsAsync(Guid id)
    {
        return await _dbSet
            .Include(m => m.Reviews)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    /// <summary>
    /// Obtiene películas filtradas, ordenadas y paginadas
    /// </summary>
    /// <param name="criteria">Criterios de filtrado, ordenamiento y paginación</param>
    /// <returns>Tupla con las películas encontradas y el total de registros</returns>
    public async Task<(IEnumerable<Movie> Items, int TotalCount)> GetFilteredAndPagedAsync(MovieFilterCriteria criteria)
    {
        var query = _dbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(criteria.Search))
        {
            var search = criteria.Search.Trim();
            query = query.Where(m =>
                m.Title.Contains(search) ||
                m.Director.Contains(search) ||
                m.Genre.Contains(search) ||
                (m.Description != null && m.Description.Contains(search))
            );
        }

        if (criteria.SortBy == MovieSortBy.Rating)
        {
            query = query.Include(m => m.Reviews);
        }

        var totalCount = await query.CountAsync();

        query = ApplySorting(query, criteria.SortBy, criteria.IsDescending);

        var items = await query
            .Skip((criteria.PageNumber - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    /// <summary>
    /// Desactiva una película mediante soft delete
    /// </summary>
    /// <param name="id">Identificador de la película a desactivar</param>
    public async Task DisableAsync(Guid id)
    {
        var movie = await GetByIdAsync(id);
        if (movie != null)
        {
            movie.Disable();
            await UpdateAsync(movie);
        }
    }

    /// <summary>
    /// Verifica si existe una película con el título especificado
    /// </summary>
    /// <param name="title">Título a buscar (case-insensitive)</param>
    /// <param name="excludeId">Identificador opcional de película a excluir de la búsqueda</param>
    /// <returns>True si existe una película con ese título, false en caso contrario</returns>
    public async Task<bool> ExistsByTitleAsync(string title, Guid? excludeId = null)
    {
        var query = _dbSet.Where(m => m.Title == title);

        if (excludeId.HasValue)
        {
            query = query.Where(m => m.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    private static IQueryable<Movie> ApplySorting(IQueryable<Movie> query, MovieSortBy sortBy, bool isDescending)
    {
        if (isDescending)
        {
            return sortBy switch
            {
                MovieSortBy.Title => query.OrderByDescending(m => m.Title),
                MovieSortBy.Year => query.OrderByDescending(m => m.ReleaseYear),
                MovieSortBy.Rating => query.OrderByDescending(m => m.Reviews.Any() ? m.Reviews.Average(r => r.Rating) : 0),
                MovieSortBy.CreatedAt => query.OrderByDescending(m => m.CreatedAt),
                _ => query.OrderByDescending(m => m.CreatedAt)
            };
        }

        return sortBy switch
        {
            MovieSortBy.Title => query.OrderBy(m => m.Title),
            MovieSortBy.Year => query.OrderBy(m => m.ReleaseYear),
            MovieSortBy.Rating => query.OrderBy(m => m.Reviews.Any() ? m.Reviews.Average(r => r.Rating) : 0),
            MovieSortBy.CreatedAt => query.OrderBy(m => m.CreatedAt),
            _ => query.OrderBy(m => m.CreatedAt)
        };
    }
}
