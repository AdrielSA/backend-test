using BackendTest.Core.Entities;
using BackendTest.Core.Enums;
using BackendTest.Core.ValueObjects;
using BackendTest.Infrastructure.Data;
using BackendTest.Infrastructure.Repositories;
using BackendTest.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BackendTest.Tests.Unit.Repositories;

public class MovieRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly MovieRepository _sut;

    public MovieRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _sut = new MovieRepository(_context);
    }

    [Fact]
    public async Task GetByIdWithReviewsAsync_WithExistingId_ReturnsMovieWithReviews()
    {
        // Arrange
        var movie = TestDataBuilder.CreateMovie();
        await _context.Movies.AddAsync(movie);
        await _context.SaveChangesAsync();

        var review1 = TestDataBuilder.CreateReview(movie.Id, 5);
        var review2 = TestDataBuilder.CreateReview(movie.Id, 4);
        await _context.Reviews.AddRangeAsync(review1, review2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByIdWithReviewsAsync(movie.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(movie.Id);
        result.Reviews.Should().HaveCount(2);
        result.Reviews.Should().Contain(r => r.Id == review1.Id);
        result.Reviews.Should().Contain(r => r.Id == review2.Id);
    }

    [Fact]
    public async Task GetByIdWithReviewsAsync_WithNonExistentId_ReturnsNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _sut.GetByIdWithReviewsAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetFilteredAndPagedAsync_WithSearchFilter_ReturnsFilteredMovies()
    {
        // Arrange
        var movie1 = TestDataBuilder.CreateMovie("El Padrino", 1972, "Drama");
        var movie2 = TestDataBuilder.CreateMovie("Titanic", 1997, "Romance");
        var movie3 = TestDataBuilder.CreateMovie("El Padrino II", 1974, "Drama");

        await _context.Movies.AddRangeAsync(movie1, movie2, movie3);
        await _context.SaveChangesAsync();

        var criteria = new MovieFilterCriteria
        {
            Search = "Padrino",
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var (items, totalCount) = await _sut.GetFilteredAndPagedAsync(criteria);

        // Assert
        totalCount.Should().Be(2);
        items.Should().HaveCount(2);
        items.Should().Contain(m => m.Title == "El Padrino");
        items.Should().Contain(m => m.Title == "El Padrino II");
    }

    [Fact]
    public async Task GetFilteredAndPagedAsync_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        for (int i = 1; i <= 5; i++)
        {
            var movie = TestDataBuilder.CreateMovie($"Película {i}", 2020 + i, "Acción");
            await _context.Movies.AddAsync(movie);
        }
        await _context.SaveChangesAsync();

        var criteria = new MovieFilterCriteria
        {
            PageNumber = 2,
            PageSize = 2
        };

        // Act
        var (items, totalCount) = await _sut.GetFilteredAndPagedAsync(criteria);

        // Assert
        totalCount.Should().Be(5);
        items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetFilteredAndPagedAsync_SortByTitle_ReturnsSortedMovies()
    {
        // Arrange
        var movie1 = TestDataBuilder.CreateMovie("Zorro", 2020, "Acción");
        var movie2 = TestDataBuilder.CreateMovie("Avatar", 2021, "Ciencia Ficción");
        var movie3 = TestDataBuilder.CreateMovie("Matrix", 2022, "Acción");

        await _context.Movies.AddRangeAsync(movie1, movie2, movie3);
        await _context.SaveChangesAsync();

        var criteria = new MovieFilterCriteria
        {
            SortBy = MovieSortBy.Title,
            IsDescending = false,
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var (items, totalCount) = await _sut.GetFilteredAndPagedAsync(criteria);

        // Assert
        var movieList = items.ToList();
        movieList[0].Title.Should().Be("Avatar");
        movieList[1].Title.Should().Be("Matrix");
        movieList[2].Title.Should().Be("Zorro");
    }

    [Fact]
    public async Task GetFilteredAndPagedAsync_SortByRating_ReturnsSortedMovies()
    {
        // Arrange
        var movie1 = TestDataBuilder.CreateMovie("Película A", 2020, "Acción");
        var movie2 = TestDataBuilder.CreateMovie("Película B", 2021, "Drama");
        var movie3 = TestDataBuilder.CreateMovie("Película C", 2022, "Comedia");

        await _context.Movies.AddRangeAsync(movie1, movie2, movie3);
        await _context.SaveChangesAsync();

        await _context.Reviews.AddAsync(TestDataBuilder.CreateReview(movie1.Id, 3));
        await _context.Reviews.AddAsync(TestDataBuilder.CreateReview(movie2.Id, 5));
        await _context.Reviews.AddAsync(TestDataBuilder.CreateReview(movie2.Id, 5));
        await _context.Reviews.AddAsync(TestDataBuilder.CreateReview(movie3.Id, 4));
        await _context.SaveChangesAsync();

        var criteria = new MovieFilterCriteria
        {
            SortBy = MovieSortBy.Rating,
            IsDescending = true,
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var (items, totalCount) = await _sut.GetFilteredAndPagedAsync(criteria);

        // Assert
        var movieList = items.ToList();
        movieList[0].Title.Should().Be("Película B");
    }

    [Fact]
    public async Task DisableAsync_WithExistingId_DisablesMovie()
    {
        // Arrange
        var movie = TestDataBuilder.CreateMovie();
        await _context.Movies.AddAsync(movie);
        await _context.SaveChangesAsync();

        // Act
        await _sut.DisableAsync(movie.Id);
        await _context.SaveChangesAsync();

        // Assert
        _context.ChangeTracker.Clear();
        var disabledMovie = await _context.Movies.IgnoreQueryFilters().FirstOrDefaultAsync(m => m.Id == movie.Id);
        disabledMovie.Should().NotBeNull();
        disabledMovie!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task DisableAsync_WithNonExistentId_DoesNotThrow()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        Func<Task> act = async () => await _sut.DisableAsync(nonExistentId);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ExistsByTitleAsync_WithExistingTitle_ReturnsTrue()
    {
        // Arrange
        var movie = TestDataBuilder.CreateMovie("Título Único");
        await _context.Movies.AddAsync(movie);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.ExistsByTitleAsync("Título Único");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByTitleAsync_WithNonExistentTitle_ReturnsFalse()
    {
        // Arrange & Act
        var result = await _sut.ExistsByTitleAsync("Título No Existente");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsByTitleAsync_WithExcludeId_ExcludesSpecifiedMovie()
    {
        // Arrange
        var movie1 = TestDataBuilder.CreateMovie("Título Compartido");
        var movie2 = TestDataBuilder.CreateMovie("Otra Película");
        
        await _context.Movies.AddRangeAsync(movie1, movie2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.ExistsByTitleAsync("Título Compartido", movie1.Id);

        // Assert
        result.Should().BeFalse();
    }


    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
