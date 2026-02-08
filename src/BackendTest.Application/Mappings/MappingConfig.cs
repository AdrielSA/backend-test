using BackendTest.Application.DTOs;
using BackendTest.Core.Entities;
using Mapster;

namespace BackendTest.Application.Mappings;

public static class MappingConfig
{
    public static void RegisterMappings()
    {
        // Movie -> MovieDto
        TypeAdapterConfig<Movie, MovieDto>
            .NewConfig()
            .Map(dest => dest.AverageRating, src => src.CalculateAverageRating());

        // Movie -> MovieDetailDto
        TypeAdapterConfig<Movie, MovieDetailDto>
            .NewConfig()
            .Map(dest => dest.AverageRating, src => src.CalculateAverageRating())
            .Map(dest => dest.Reviews, src => src.Reviews);

        // CreateMovieDto -> Movie
        TypeAdapterConfig<CreateMovieDto, Movie>
            .NewConfig()
            .Map(dest => dest.Id, src => Guid.NewGuid())
            .Map(dest => dest.CreatedAt, src => DateTime.UtcNow)
            .Map(dest => dest.UpdatedAt, src => DateTime.UtcNow)
            .Map(dest => dest.IsActive, src => true);

        // Review -> ReviewDto
        TypeAdapterConfig<Review, ReviewDto>
            .NewConfig();

        // CreateReviewDto -> Review
        TypeAdapterConfig<CreateReviewDto, Review>
            .NewConfig()
            .Map(dest => dest.Id, src => Guid.NewGuid())
            .Map(dest => dest.CreatedAt, src => DateTime.UtcNow);
    }
}
