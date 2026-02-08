using BackendTest.Application.Common;
using BackendTest.Application.DTOs;
using BackendTest.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BackendTest.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MoviesController(IMovieService movieService) : ControllerBase
{
    private readonly IMovieService _movieService = movieService;

    /// <summary>
    /// Crea una nueva película en el sistema
    /// </summary>
    /// <param name="createMovieDto">Datos de la película a crear</param>
    /// <returns>La película creada con su identificador único</returns>
    /// <response code="201">Película creada exitosamente</response>
    /// <response code="400">Datos inválidos o película duplicada</response>
    [HttpPost]
    [ProducesResponseType(typeof(MovieDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MovieDto>> CreateMovie([FromBody] CreateMovieDto createMovieDto)
    {
        var movie = await _movieService.CreateMovieAsync(createMovieDto);
        return CreatedAtAction(nameof(GetMovieById), new { id = movie.Id }, movie);
    }

    /// <summary>
    /// Obtiene una película específica por su identificador
    /// </summary>
    /// <param name="id">Identificador único de la película</param>
    /// <returns>Información de la película con su calificación promedio</returns>
    /// <response code="200">Película encontrada</response>
    /// <response code="404">Película no encontrada</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(MovieDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MovieDto>> GetMovieById([FromRoute] Guid id)
    {
        var movie = await _movieService.GetMovieByIdAsync(id);
        return Ok(movie);
    }

    /// <summary>
    /// Obtiene una película con todas sus reseñas asociadas
    /// </summary>
    /// <param name="id">Identificador único de la película</param>
    /// <returns>Información detallada de la película incluyendo todas sus reseñas</returns>
    /// <response code="200">Película con reseñas encontrada</response>
    /// <response code="404">Película no encontrada</response>
    [HttpGet("{id}/details")]
    [ProducesResponseType(typeof(MovieDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MovieDetailDto>> GetMovieWithReviews([FromRoute] Guid id)
    {
        var movie = await _movieService.GetMovieWithReviewsAsync(id);
        return Ok(movie);
    }

    /// <summary>
    /// Obtiene un listado paginado de películas con búsqueda, filtrado y ordenamiento
    /// </summary>
    /// <param name="query">Criterios de búsqueda, ordenamiento y paginación</param>
    /// <returns>Listado paginado de películas que coinciden con los criterios especificados</returns>
    /// <response code="200">Listado obtenido exitosamente</response>
    /// <response code="400">Parámetros de consulta inválidos</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<MovieDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<MovieDto>>> GetMovies([FromQuery] GetMoviesQueryDto query)
    {
        var result = await _movieService.GetMoviesAsync(query);
        return Ok(result);
    }

    /// <summary>
    /// Desactiva una película mediante soft delete para que no aparezca en listados
    /// </summary>
    /// <param name="id">Identificador único de la película a desactivar</param>
    /// <response code="204">Película desactivada exitosamente</response>
    /// <response code="404">Película no encontrada</response>
    [HttpPatch("{id}/disable")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DisableMovie([FromRoute] Guid id)
    {
        await _movieService.DisableMovieAsync(id);
        return NoContent();
    }
}
