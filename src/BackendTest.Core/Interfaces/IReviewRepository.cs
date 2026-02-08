using BackendTest.Core.Entities;

namespace BackendTest.Core.Interfaces;

public interface IReviewRepository : IRepository<Review>
{
    Task<IEnumerable<Review>> GetByMovieIdAsync(Guid movieId);
    Task<double> GetAverageRatingByMovieIdAsync(Guid movieId);
}
