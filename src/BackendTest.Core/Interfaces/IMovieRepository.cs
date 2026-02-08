using BackendTest.Core.Entities;
using BackendTest.Core.ValueObjects;

namespace BackendTest.Core.Interfaces;

public interface IMovieRepository : IRepository<Movie>
{
    Task<Movie?> GetByIdWithReviewsAsync(Guid id);
    Task<(IEnumerable<Movie> Items, int TotalCount)> GetFilteredAndPagedAsync(MovieFilterCriteria criteria);
    Task DisableAsync(Guid id);
    Task<bool> ExistsByTitleAsync(string title, Guid? excludeId = null);
}
