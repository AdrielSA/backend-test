using BackendTest.Application.DTOs;

namespace BackendTest.Application.Interfaces;

public interface IReviewService
{
    Task<ReviewDto> CreateReviewAsync(Guid movieId, CreateReviewDto createReviewDto);
    Task<IEnumerable<ReviewDto>> GetReviewsByMovieIdAsync(Guid movieId);
}
