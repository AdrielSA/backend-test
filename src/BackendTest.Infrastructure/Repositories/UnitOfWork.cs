using BackendTest.Core.Interfaces;
using BackendTest.Infrastructure.Data;

namespace BackendTest.Infrastructure.Repositories;

public class UnitOfWork(ApplicationDbContext context) : IUnitOfWork
{
    private readonly ApplicationDbContext _context = context;
    private IMovieRepository? _movies;
    private IReviewRepository? _reviews;

    /// <summary>
    /// Repositorio de películas con lazy loading
    /// </summary>
    public IMovieRepository Movies => _movies ??= new MovieRepository(_context);

    /// <summary>
    /// Repositorio de reseñas con lazy loading
    /// </summary>
    public IReviewRepository Reviews => _reviews ??= new ReviewRepository(_context);

    /// <summary>
    /// Guarda todos los cambios pendientes en la base de datos dentro de una transacción
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Número de entidades afectadas</returns>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
