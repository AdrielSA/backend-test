using BackendTest.Core.Entities;

namespace BackendTest.Tests.Helpers;

public static class TestDataBuilder
{
    public static Movie CreateMovie(
        string title = "Película de Prueba",
        int releaseYear = 2020,
        string genre = "Acción",
        bool isActive = true)
    {
        return new Movie
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = "Descripción de prueba para la película. Esta es una historia fascinante que cautivará a la audiencia.",
            ReleaseYear = releaseYear,
            Genre = genre,
            Director = "Director de Prueba",
            DurationMinutes = 120,
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Reviews = []
        };
    }

    public static Review CreateReview(Guid movieId, int rating = 5)
    {
        return new Review
        {
            Id = Guid.NewGuid(),
            MovieId = movieId,
            ReviewerName = "Revisor de Prueba",
            Comment = "Excelente película, muy recomendada. La historia es cautivadora y las actuaciones son sobresalientes.",
            Rating = rating,
            CreatedAt = DateTime.UtcNow
        };
    }
}
