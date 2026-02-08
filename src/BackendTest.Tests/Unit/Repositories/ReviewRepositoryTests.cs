using BackendTest.Core.Entities;
using BackendTest.Infrastructure.Data;
using BackendTest.Infrastructure.Repositories;
using BackendTest.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BackendTest.Tests.Unit.Repositories;

public class ReviewRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly ReviewRepository _sut;

    public ReviewRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _sut = new ReviewRepository(_context);
    }

    [Fact]
    public async Task GetByMovieIdAsync_WithExistingMovieId_ReturnsReviewsOrderedByCreatedAtDescending()
    {
        // Arrange
        var movie = TestDataBuilder.CreateMovie();
        await _context.Movies.AddAsync(movie);
        await _context.SaveChangesAsync();

        var review1 = TestDataBuilder.CreateReview(movie.Id, 5);
        review1.CreatedAt = DateTime.UtcNow.AddDays(-2);

        var review2 = TestDataBuilder.CreateReview(movie.Id, 4);
        review2.CreatedAt = DateTime.UtcNow.AddDays(-1);

        var review3 = TestDataBuilder.CreateReview(movie.Id, 3);
        review3.CreatedAt = DateTime.UtcNow;

        await _context.Reviews.AddRangeAsync(review1, review2, review3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByMovieIdAsync(movie.Id);

        // Assert
        var reviewList = result.ToList();
        reviewList.Should().HaveCount(3);
        reviewList[0].Id.Should().Be(review3.Id);
        reviewList[1].Id.Should().Be(review2.Id);
        reviewList[2].Id.Should().Be(review1.Id);
    }

    [Fact]
    public async Task GetByMovieIdAsync_WithNonExistentMovieId_ReturnsEmptyCollection()
    {
        // Arrange
        var nonExistentMovieId = Guid.NewGuid();

        // Act
        var result = await _sut.GetByMovieIdAsync(nonExistentMovieId);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByMovieIdAsync_OnlyReturnsReviewsForSpecifiedMovie()
    {
        // Arrange
        var movie1 = TestDataBuilder.CreateMovie("Película 1");
        var movie2 = TestDataBuilder.CreateMovie("Película 2");
        await _context.Movies.AddRangeAsync(movie1, movie2);
        await _context.SaveChangesAsync();

        var review1 = TestDataBuilder.CreateReview(movie1.Id, 5);
        var review2 = TestDataBuilder.CreateReview(movie1.Id, 4);
        var review3 = TestDataBuilder.CreateReview(movie2.Id, 3);

        await _context.Reviews.AddRangeAsync(review1, review2, review3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByMovieIdAsync(movie1.Id);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(r => r.MovieId.Should().Be(movie1.Id));
        result.Should().NotContain(r => r.Id == review3.Id);
    }

    [Fact]
    public async Task GetAverageRatingByMovieIdAsync_WithMultipleReviews_ReturnsCorrectAverage()
    {
        // Arrange
        var movie = TestDataBuilder.CreateMovie();
        await _context.Movies.AddAsync(movie);
        await _context.SaveChangesAsync();

        var review1 = TestDataBuilder.CreateReview(movie.Id, 5);
        var review2 = TestDataBuilder.CreateReview(movie.Id, 4);
        var review3 = TestDataBuilder.CreateReview(movie.Id, 3);

        await _context.Reviews.AddRangeAsync(review1, review2, review3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetAverageRatingByMovieIdAsync(movie.Id);

        // Assert
        result.Should().Be(4.0);
    }

    [Fact]
    public async Task GetAverageRatingByMovieIdAsync_WithNoReviews_ReturnsZero()
    {
        // Arrange
        var movie = TestDataBuilder.CreateMovie();
        await _context.Movies.AddAsync(movie);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetAverageRatingByMovieIdAsync(movie.Id);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task GetAverageRatingByMovieIdAsync_WithSingleReview_ReturnsRating()
    {
        // Arrange
        var movie = TestDataBuilder.CreateMovie();
        await _context.Movies.AddAsync(movie);
        await _context.SaveChangesAsync();

        var review = TestDataBuilder.CreateReview(movie.Id, 5);
        await _context.Reviews.AddAsync(review);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetAverageRatingByMovieIdAsync(movie.Id);

        // Assert
        result.Should().Be(5.0);
    }

    [Fact]
    public async Task GetAverageRatingByMovieIdAsync_OnlyConsidersReviewsFromSpecifiedMovie()
    {
        // Arrange
        var movie1 = TestDataBuilder.CreateMovie("Película 1");
        var movie2 = TestDataBuilder.CreateMovie("Película 2");
        await _context.Movies.AddRangeAsync(movie1, movie2);
        await _context.SaveChangesAsync();

        var review1 = TestDataBuilder.CreateReview(movie1.Id, 5);
        var review2 = TestDataBuilder.CreateReview(movie1.Id, 3);
        var review3 = TestDataBuilder.CreateReview(movie2.Id, 1);

        await _context.Reviews.AddRangeAsync(review1, review2, review3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetAverageRatingByMovieIdAsync(movie1.Id);

        // Assert
        result.Should().Be(4.0);
    }


    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
