namespace BackendTest.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IMovieRepository Movies { get; }
    IReviewRepository Reviews { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
