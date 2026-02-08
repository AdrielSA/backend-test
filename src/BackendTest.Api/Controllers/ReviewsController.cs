using BackendTest.Application.DTOs;
using BackendTest.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BackendTest.Api.Controllers;

[ApiController]
[Route("api/movies/{movieId}/reviews")]
public class ReviewsController(
    IReviewService reviewService,
    ILogger<ReviewsController> logger) : ControllerBase
{
    private readonly IReviewService _reviewService = reviewService;
    private readonly ILogger<ReviewsController> _logger = logger;

    /// <summary>
    /// Crea una nueva reseña para una película específica
    /// </summary>
    /// <param name="movieId">Identificador de la película a reseñar</param>
    /// <param name="createReviewDto">Datos de la reseña a crear</param>
    /// <returns>La reseña creada con su identificador único</returns>
    /// <response code="201">Reseña creada exitosamente</response>
    /// <response code="400">Datos inválidos (calificación fuera de rango 1-5)</response>
    /// <response code="404">Película no encontrada o inactiva</response>
    [HttpPost]
    [ProducesResponseType(typeof(ReviewDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReviewDto>> CreateReview([FromRoute] Guid movieId, [FromBody] CreateReviewDto createReviewDto)
    {
        var review = await _reviewService.CreateReviewAsync(movieId, createReviewDto);
        return CreatedAtAction(nameof(GetReviewsByMovieId), new { movieId }, review);
    }

    /// <summary>
    /// Obtiene todas las reseñas de una película ordenadas por fecha más reciente
    /// </summary>
    /// <param name="movieId">Identificador de la película</param>
    /// <returns>Lista de reseñas ordenadas por fecha de creación descendente</returns>
    /// <response code="200">Reseñas obtenidas exitosamente</response>
    /// <response code="404">Película no encontrada</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ReviewDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<ReviewDto>>> GetReviewsByMovieId([FromRoute] Guid movieId)
    {
        var reviews = await _reviewService.GetReviewsByMovieIdAsync(movieId);
        return Ok(reviews);
    }
}
