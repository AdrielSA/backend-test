using BackendTest.Application.Common;
using BackendTest.Application.DTOs;
using BackendTest.Application.Interfaces;
using BackendTest.Application.Services;
using BackendTest.Core.Entities;
using BackendTest.Core.Exceptions;
using BackendTest.Core.Interfaces;
using BackendTest.Core.ValueObjects;
using BackendTest.Tests.Helpers;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;

namespace BackendTest.Tests.Unit.Services;

public class MovieServiceTests : IDisposable
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<ILogger<MovieService>> _loggerMock;
    private readonly Mock<IValidator<CreateMovieDto>> _createMovieValidatorMock;
    private readonly Mock<IValidator<GetMoviesQueryDto>> _getMoviesQueryValidatorMock;
    private readonly MovieService _sut;

    public MovieServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _cacheServiceMock = new Mock<ICacheService>();
        _loggerMock = new Mock<ILogger<MovieService>>();
        _createMovieValidatorMock = new Mock<IValidator<CreateMovieDto>>();
        _getMoviesQueryValidatorMock = new Mock<IValidator<GetMoviesQueryDto>>();

        _createMovieValidatorMock.Setup(x => x.ValidateAsync(It.IsAny<CreateMovieDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _getMoviesQueryValidatorMock.Setup(x => x.ValidateAsync(It.IsAny<GetMoviesQueryDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _sut = new MovieService(
            _unitOfWorkMock.Object,
            _cacheServiceMock.Object,
            _loggerMock.Object,
            _createMovieValidatorMock.Object,
            _getMoviesQueryValidatorMock.Object);
    }

    [Fact]
    public async Task CreateMovieAsync_WithValidData_ReturnsCreatedMovie()
    {
        // Arrange
        var createDto = new CreateMovieDto
        {
            Title = "Película Nueva",
            Description = "Descripción de la película nueva para pruebas unitarias",
            ReleaseYear = 2023,
            Genre = "Acción",
            Director = "Director de Prueba",
            DurationMinutes = 120
        };

        _unitOfWorkMock.Setup(x => x.Movies.ExistsByTitleAsync(createDto.Title, null))
            .ReturnsAsync(false);
        _unitOfWorkMock.Setup(x => x.Movies.AddAsync(It.IsAny<Movie>()))
            .ReturnsAsync((Movie m) => m);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(default))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.CreateMovieAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be(createDto.Title);
        result.IsActive.Should().BeTrue();
        _unitOfWorkMock.Verify(x => x.Movies.AddAsync(It.IsAny<Movie>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task CreateMovieAsync_WithDuplicateTitle_ThrowsBusinessRuleException()
    {
        // Arrange
        var createDto = new CreateMovieDto
        {
            Title = "Película Duplicada",
            Genre = "Acción",
            Director = "Director de Prueba",
            ReleaseYear = 2023
        };

        _unitOfWorkMock.Setup(x => x.Movies.ExistsByTitleAsync(createDto.Title, null))
            .ReturnsAsync(true);

        // Act
        Func<Task> act = async () => await _sut.CreateMovieAsync(createDto);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage($"Ya existe una película con el título '{createDto.Title}'.");
    }

    [Fact]
    public async Task GetMovieByIdAsync_WithValidId_ReturnsMovie()
    {
        // Arrange
        var movie = TestDataBuilder.CreateMovie();
        var movieId = movie.Id;

        _cacheServiceMock.Setup(x => x.GetAsync<MovieDto>(It.IsAny<string>()))
            .ReturnsAsync((MovieDto?)null);
        _unitOfWorkMock.Setup(x => x.Movies.GetByIdWithReviewsAsync(movieId))
            .ReturnsAsync(movie);

        // Act
        var result = await _sut.GetMovieByIdAsync(movieId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(movieId);
        result.Title.Should().Be(movie.Title);
    }

    [Fact]
    public async Task GetMovieByIdAsync_WithInvalidId_ThrowsNotFoundException()
    {
        // Arrange
        var movieId = Guid.NewGuid();
        _cacheServiceMock.Setup(x => x.GetAsync<MovieDto>(It.IsAny<string>()))
            .ReturnsAsync((MovieDto?)null);
        _unitOfWorkMock.Setup(x => x.Movies.GetByIdWithReviewsAsync(movieId))
            .ReturnsAsync((Movie?)null);

        // Act
        Func<Task> act = async () => await _sut.GetMovieByIdAsync(movieId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DisableMovieAsync_WithValidId_DisablesMovie()
    {
        // Arrange
        var movie = TestDataBuilder.CreateMovie();
        var movieId = movie.Id;

        _unitOfWorkMock.Setup(x => x.Movies.GetByIdAsync(movieId))
            .ReturnsAsync(movie);
        _unitOfWorkMock.Setup(x => x.Movies.UpdateAsync(It.IsAny<Movie>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(default))
            .ReturnsAsync(1);

        // Act
        await _sut.DisableMovieAsync(movieId);

        // Assert
        movie.IsActive.Should().BeFalse();
        _unitOfWorkMock.Verify(x => x.Movies.UpdateAsync(It.Is<Movie>(m => !m.IsActive)), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task DisableMovieAsync_WithInvalidId_ThrowsNotFoundException()
    {
        // Arrange
        var movieId = Guid.NewGuid();
        _unitOfWorkMock.Setup(x => x.Movies.GetByIdAsync(movieId))
            .ReturnsAsync((Movie?)null);

        // Act
        Func<Task> act = async () => await _sut.DisableMovieAsync(movieId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetMoviesAsync_WithSearch_ReturnsFilteredMovies()
    {
        // Arrange
        var movies = new List<Movie>
        {
            TestDataBuilder.CreateMovie("Película 1", 2020, "Acción"),
            TestDataBuilder.CreateMovie("Película 2", 2021, "Drama")
        };

        var query = new GetMoviesQueryDto
        {
            Search = "Acción",
            PageNumber = 1,
            PageSize = 10
        };

        var filteredMovies = movies.Where(m => m.Genre == "Acción").ToList();

        _cacheServiceMock.Setup(x => x.GetAsync<PagedResult<MovieDto>>(It.IsAny<string>()))
            .ReturnsAsync((PagedResult<MovieDto>?)null);
        _unitOfWorkMock.Setup(x => x.Movies.GetFilteredAndPagedAsync(It.IsAny<MovieFilterCriteria>()))
            .ReturnsAsync((filteredMovies, filteredMovies.Count));

        // Act
        var result = await _sut.GetMoviesAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(1);
        result.Items.Should().HaveCount(1);
        result.Items.First().Genre.Should().Be("Acción");
    }

    public void Dispose() 
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
