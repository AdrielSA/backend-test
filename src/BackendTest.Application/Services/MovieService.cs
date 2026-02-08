using BackendTest.Application.Common;
using BackendTest.Application.DTOs;
using BackendTest.Application.Interfaces;
using BackendTest.Core.Entities;
using BackendTest.Core.Exceptions;
using BackendTest.Core.Interfaces;
using BackendTest.Core.ValueObjects;
using FluentValidation;
using Mapster;
using Microsoft.Extensions.Logging;

namespace BackendTest.Application.Services;

public class MovieService(
    IUnitOfWork unitOfWork,
    ICacheService cacheService,
    ILogger<MovieService> logger,
    IValidator<CreateMovieDto> createMovieValidator,
    IValidator<GetMoviesQueryDto> getMoviesQueryValidator) : IMovieService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICacheService _cacheService = cacheService;
    private readonly ILogger<MovieService> _logger = logger;
    private readonly IValidator<CreateMovieDto> _createMovieValidator = createMovieValidator;
    private readonly IValidator<GetMoviesQueryDto> _getMoviesQueryValidator = getMoviesQueryValidator;
    private const string MovieCacheKeyPrefix = "movie:";
    private const string MoviesCacheKeyPrefix = "movies:";

    /// <summary>
    /// Crea una nueva película validando que no exista duplicado por título
    /// </summary>
    /// <param name="createMovieDto">Datos de la película a crear</param>
    /// <returns>La película creada con su información completa</returns>
    /// <exception cref="ValidationException">Cuando los datos no cumplen las validaciones</exception>
    /// <exception cref="BusinessRuleException">Cuando ya existe una película con el mismo título</exception>
    public async Task<MovieDto> CreateMovieAsync(CreateMovieDto createMovieDto)
    {
        await _createMovieValidator.ValidateAndThrowAsync(createMovieDto);

        var exists = await _unitOfWork.Movies.ExistsByTitleAsync(createMovieDto.Title);
        if (exists)
        {
            throw new BusinessRuleException($"Ya existe una película con el título '{createMovieDto.Title}'.");
        }

        var movie = createMovieDto.Adapt<Movie>();
        movie.Id = Guid.NewGuid();
        movie.CreatedAt = DateTime.UtcNow;
        movie.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Movies.AddAsync(movie);
        await _unitOfWork.SaveChangesAsync();

        await _cacheService.RemoveByPatternAsync($"{MoviesCacheKeyPrefix}*");

        return movie.Adapt<MovieDto>();
    }

    /// <summary>
    /// Obtiene una película por su identificador con caché de 5 minutos
    /// </summary>
    /// <param name="id">Identificador único de la película</param>
    /// <returns>Información de la película con su calificación promedio</returns>
    /// <exception cref="NotFoundException">Cuando la película no existe</exception>
    public async Task<MovieDto> GetMovieByIdAsync(Guid id)
    {
        var cacheKey = $"{MovieCacheKeyPrefix}{id}";
        var cachedMovie = await _cacheService.GetAsync<MovieDto>(cacheKey);
        if (cachedMovie != null)
        {
            _logger.LogDebug("Película {MovieId} obtenida desde caché", id);
            return cachedMovie;
        }

        var movie = await _unitOfWork.Movies.GetByIdWithReviewsAsync(id) ?? throw new NotFoundException(nameof(Movie), id);
        var movieDto = movie.Adapt<MovieDto>();

        _logger.LogDebug("Película {MovieId} obtenida desde base de datos", id);

        await _cacheService.SetAsync(cacheKey, movieDto, TimeSpan.FromMinutes(5));

        return movieDto;
    }

    /// <summary>
    /// Obtiene una película con todas sus reseñas asociadas
    /// </summary>
    /// <param name="id">Identificador único de la película</param>
    /// <returns>Información detallada de la película incluyendo todas sus reseñas</returns>
    /// <exception cref="NotFoundException">Cuando la película no existe</exception>
    public async Task<MovieDetailDto> GetMovieWithReviewsAsync(Guid id)
    {
        var movie = await _unitOfWork.Movies.GetByIdWithReviewsAsync(id) ?? throw new NotFoundException(nameof(Movie), id);
        return movie.Adapt<MovieDetailDto>();
    }

    /// <summary>
    /// Obtiene un listado paginado de películas con filtros y ordenamiento, con caché de 2 minutos
    /// </summary>
    /// <param name="query">Criterios de búsqueda, ordenamiento y paginación</param>
    /// <returns>Resultado paginado con las películas que cumplen los criterios</returns>
    /// <exception cref="ValidationException">Cuando los parámetros de la consulta son inválidos</exception>
    public async Task<PagedResult<MovieDto>> GetMoviesAsync(GetMoviesQueryDto query)
    {
        await _getMoviesQueryValidator.ValidateAndThrowAsync(query);

        var criteria = query.Adapt<MovieFilterCriteria>();

        var cacheKey = $"{MoviesCacheKeyPrefix}{GenerateCacheKey(query)}";
        var cachedResult = await _cacheService.GetAsync<PagedResult<MovieDto>>(cacheKey);
        if (cachedResult != null)
        {
            _logger.LogDebug("Listado de películas obtenido desde caché: {ResultCount} resultados", cachedResult.Items.Count());
            return cachedResult;
        }

        var (movies, totalCount) = await _unitOfWork.Movies.GetFilteredAndPagedAsync(criteria);
        var movieDtos = movies.Adapt<List<MovieDto>>();

        var result = new PagedResult<MovieDto>(
            movieDtos,
            totalCount,
            query.PageNumber,
            query.PageSize
        );

        _logger.LogDebug("Listado de películas obtenido desde BD: {ResultCount} de {TotalCount} resultados", result.Items.Count(), result.TotalCount);

        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(2));

        return result;
    }

    /// <summary>
    /// Desactiva una película mediante soft delete para que no aparezca en listados
    /// </summary>
    /// <param name="id">Identificador único de la película a desactivar</param>
    /// <exception cref="NotFoundException">Cuando la película no existe</exception>
    public async Task DisableMovieAsync(Guid id)
    {
        var movie = await _unitOfWork.Movies.GetByIdAsync(id) ?? throw new NotFoundException(nameof(Movie), id);

        movie.Disable();
        movie.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Movies.UpdateAsync(movie);
        await _unitOfWork.SaveChangesAsync();

        await _cacheService.RemoveAsync($"{MovieCacheKeyPrefix}{id}");
        await _cacheService.RemoveByPatternAsync($"{MoviesCacheKeyPrefix}*");
    }

    private static string GenerateCacheKey(GetMoviesQueryDto query)
    {
        return $"{query.Search}_{query.SortBy}_{query.IsDescending}_" +
               $"{query.PageNumber}_{query.PageSize}";
    }
}
