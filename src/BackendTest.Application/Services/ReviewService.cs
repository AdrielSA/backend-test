using BackendTest.Application.DTOs;
using BackendTest.Application.Interfaces;
using BackendTest.Core.Entities;
using BackendTest.Core.Exceptions;
using BackendTest.Core.Interfaces;
using FluentValidation;
using Mapster;
using Microsoft.Extensions.Logging;

namespace BackendTest.Application.Services;

public class ReviewService(
    IUnitOfWork unitOfWork,
    ICacheService cacheService,
    ILogger<ReviewService> logger,
    IValidator<CreateReviewDto> createReviewValidator) : IReviewService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICacheService _cacheService = cacheService;
    private readonly ILogger<ReviewService> _logger = logger;
    private readonly IValidator<CreateReviewDto> _createReviewValidator = createReviewValidator;
    private const string ReviewsCacheKeyPrefix = "reviews:movie:";
    private const string MovieCacheKeyPrefix = "movie:";

    /// <summary>
    /// Crea una nueva reseña para una película activa
    /// </summary>
    /// <param name="movieId">Identificador de la película a reseñar</param>
    /// <param name="createReviewDto">Datos de la reseña a crear</param>
    /// <returns>La reseña creada con su información completa</returns>
    /// <exception cref="ValidationException">Cuando los datos no cumplen las validaciones</exception>
    /// <exception cref="NotFoundException">Cuando la película no existe</exception>
    /// <exception cref="BusinessRuleException">Cuando la película está inactiva</exception>
    public async Task<ReviewDto> CreateReviewAsync(Guid movieId, CreateReviewDto createReviewDto)
    {
        await _createReviewValidator.ValidateAndThrowAsync(createReviewDto);

        var movie = await _unitOfWork.Movies.GetByIdAsync(movieId) ?? throw new NotFoundException(nameof(Movie), movieId);
        if (!movie.IsActive)
        {
            throw new BusinessRuleException("No se pueden agregar reseñas a una película inactiva.");
        }

        var review = createReviewDto.Adapt<Review>();
        review.Id = Guid.NewGuid();
        review.MovieId = movieId;
        review.CreatedAt = DateTime.UtcNow;

        review.SetRating(createReviewDto.Rating);

        await _unitOfWork.Reviews.AddAsync(review);
        await _unitOfWork.SaveChangesAsync();

        await _cacheService.RemoveAsync($"{ReviewsCacheKeyPrefix}{movieId}");
        await _cacheService.RemoveAsync($"{MovieCacheKeyPrefix}{movieId}");

        return review.Adapt<ReviewDto>();
    }

    /// <summary>
    /// Obtiene todas las reseñas de una película con caché de 5 minutos
    /// </summary>
    /// <param name="movieId">Identificador de la película</param>
    /// <returns>Colección de reseñas asociadas a la película</returns>
    /// <exception cref="NotFoundException">Cuando la película no existe</exception>
    public async Task<IEnumerable<ReviewDto>> GetReviewsByMovieIdAsync(Guid movieId)
    {
        var cacheKey = $"{ReviewsCacheKeyPrefix}{movieId}";
        var cachedReviews = await _cacheService.GetAsync<IEnumerable<ReviewDto>>(cacheKey);
        if (cachedReviews != null)
        {
            _logger.LogDebug("Reseñas para película {MovieId} obtenidas desde caché: {ReviewCount} reseñas", movieId, cachedReviews.Count());
            return cachedReviews;
        }

        var movie = await _unitOfWork.Movies.GetByIdAsync(movieId) ?? throw new NotFoundException(nameof(Movie), movieId);

        var reviews = await _unitOfWork.Reviews.GetByMovieIdAsync(movieId);
        var reviewDtos = reviews.Adapt<List<ReviewDto>>();

        _logger.LogDebug("Reseñas para película {MovieId} obtenidas desde BD: {ReviewCount} reseñas", movieId, reviewDtos.Count);

        await _cacheService.SetAsync(cacheKey, reviewDtos, TimeSpan.FromMinutes(5));

        return reviewDtos;
    }
}
