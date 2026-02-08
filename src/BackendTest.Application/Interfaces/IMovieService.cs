using BackendTest.Application.Common;
using BackendTest.Application.DTOs;

namespace BackendTest.Application.Interfaces;

public interface IMovieService
{
    Task<MovieDto> CreateMovieAsync(CreateMovieDto createMovieDto);
    Task<MovieDto> GetMovieByIdAsync(Guid id);
    Task<MovieDetailDto> GetMovieWithReviewsAsync(Guid id);
    Task<PagedResult<MovieDto>> GetMoviesAsync(GetMoviesQueryDto query);
    Task DisableMovieAsync(Guid id);
}
