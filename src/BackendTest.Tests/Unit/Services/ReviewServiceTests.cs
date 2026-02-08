using BackendTest.Application.DTOs;
using BackendTest.Application.Interfaces;
using BackendTest.Application.Services;
using BackendTest.Core.Entities;
using BackendTest.Core.Exceptions;
using BackendTest.Core.Interfaces;
using BackendTest.Tests.Helpers;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;

namespace BackendTest.Tests.Unit.Services;

public class ReviewServiceTests : IDisposable
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<ILogger<ReviewService>> _loggerMock;
    private readonly Mock<IValidator<CreateReviewDto>> _validatorMock;
    private readonly ReviewService _sut;

    public ReviewServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _cacheServiceMock = new Mock<ICacheService>();
        _loggerMock = new Mock<ILogger<ReviewService>>();
        _validatorMock = new Mock<IValidator<CreateReviewDto>>();

        // Configurar validator para retornar validación exitosa por defecto
        _validatorMock.Setup(x => x.ValidateAsync(It.IsAny<CreateReviewDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _sut = new ReviewService(_unitOfWorkMock.Object, _cacheServiceMock.Object, _loggerMock.Object, _validatorMock.Object);
    }

    [Fact]
    public async Task CreateReviewAsync_ForActiveMovie_ReturnsCreatedReview()
    {
        // Arrange
        var movie = TestDataBuilder.CreateMovie(isActive: true);
        var createDto = new CreateReviewDto
        {
            ReviewerName = "Juan Pérez",
            Comment = "¡Excelente película, muy recomendada!",
            Rating = 5
        };

        _unitOfWorkMock.Setup(x => x.Movies.GetByIdAsync(movie.Id))
            .ReturnsAsync(movie);
        _unitOfWorkMock.Setup(x => x.Reviews.AddAsync(It.IsAny<Review>()))
            .ReturnsAsync((Review r) => r);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(default))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.CreateReviewAsync(movie.Id, createDto);

        // Assert
        result.Should().NotBeNull();
        result.ReviewerName.Should().Be(createDto.ReviewerName);
        result.Rating.Should().Be(createDto.Rating);
        result.MovieId.Should().Be(movie.Id);
        _unitOfWorkMock.Verify(x => x.Reviews.AddAsync(It.IsAny<Review>()), Times.Once);
    }

    [Fact]
    public async Task CreateReviewAsync_ForInactiveMovie_ThrowsBusinessRuleException()
    {
        // Arrange
        var movie = TestDataBuilder.CreateMovie(isActive: false);
        var createDto = new CreateReviewDto
        {
            ReviewerName = "Juan Pérez",
            Comment = "Comentario de prueba",
            Rating = 5
        };

        _unitOfWorkMock.Setup(x => x.Movies.GetByIdAsync(movie.Id))
            .ReturnsAsync(movie);

        // Act
        Func<Task> act = async () => await _sut.CreateReviewAsync(movie.Id, createDto);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("No se pueden agregar reseñas a una película inactiva.");
    }

    [Fact]
    public async Task CreateReviewAsync_ForNonExistentMovie_ThrowsNotFoundException()
    {
        // Arrange
        var movieId = Guid.NewGuid();
        var createDto = new CreateReviewDto
        {
            ReviewerName = "Juan Pérez",
            Comment = "Comentario de prueba",
            Rating = 5
        };

        _unitOfWorkMock.Setup(x => x.Movies.GetByIdAsync(movieId))
            .ReturnsAsync((Movie?)null);

        // Act
        Func<Task> act = async () => await _sut.CreateReviewAsync(movieId, createDto);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetReviewsByMovieIdAsync_ReturnsReviews()
    {
        // Arrange
        var movie = TestDataBuilder.CreateMovie();
        var reviews = new List<Review>
        {
            TestDataBuilder.CreateReview(movie.Id, 5),
            TestDataBuilder.CreateReview(movie.Id, 4)
        };

        _cacheServiceMock.Setup(x => x.GetAsync<IEnumerable<ReviewDto>>(It.IsAny<string>()))
            .ReturnsAsync((IEnumerable<ReviewDto>?)null);
        _unitOfWorkMock.Setup(x => x.Movies.GetByIdAsync(movie.Id))
            .ReturnsAsync(movie);
        _unitOfWorkMock.Setup(x => x.Reviews.GetByMovieIdAsync(movie.Id))
            .ReturnsAsync(reviews);

        // Act
        var result = await _sut.GetReviewsByMovieIdAsync(movie.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }

    public void Dispose() 
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
