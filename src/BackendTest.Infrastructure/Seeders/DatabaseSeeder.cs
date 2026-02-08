using BackendTest.Core.Entities;
using BackendTest.Infrastructure.Data;
using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BackendTest.Infrastructure.Seeders;

public class DatabaseSeeder(ApplicationDbContext context, ILogger<DatabaseSeeder> logger)
{
    private readonly ApplicationDbContext _context = context;
    private readonly ILogger<DatabaseSeeder> _logger = logger;

    public async Task SeedAsync()
    {
        if (await _context.Movies.AnyAsync())
        {
            _logger.LogInformation("La base de datos ya contiene películas. Omitiendo seeder.");
            return;
        }

        _logger.LogInformation("Iniciando seed de datos...");

        var genres = new[] { "Acción", "Drama", "Comedia", "Suspenso", "Ciencia Ficción", "Romance", "Terror", "Animación" };

        var movieTitles = new[]
        {
            "El Secreto de", "La Última", "Los Guardianes de", "El Misterio del", "La Noche de",
            "El Destino de", "La Sombra de", "Los Héroes de", "El Regreso de", "La Leyenda de",
            "El Laberinto de", "La Casa de", "Los Secretos de", "El Viaje de", "La Historia de",
            "El Reino de", "La Ciudad de", "Los Mundos de", "El Cazador de", "La Búsqueda de"
        };

        var movieSuffixes = new[]
        {
            "la Eternidad", "los Perdidos", "las Sombras", "la Esperanza", "los Olvidados",
            "la Verdad", "los Sueños", "las Estrellas", "la Libertad", "los Caídos",
            "la Redención", "los Valientes", "las Mentiras", "la Justicia", "los Inocentes"
        };

        var movieFaker = new Faker<Movie>("es")
            .RuleFor(m => m.Id, f => Guid.NewGuid())
            .RuleFor(m => m.Title, f => $"{f.PickRandom(movieTitles)} {f.PickRandom(movieSuffixes)}")
            .RuleFor(m => m.Description, f => f.Lorem.Paragraph(3))
            .RuleFor(m => m.ReleaseYear, f => f.Random.Int(1980, 2024))
            .RuleFor(m => m.Genre, f => f.PickRandom(genres))
            .RuleFor(m => m.Director, f => f.Name.FullName())
            .RuleFor(m => m.DurationMinutes, f => f.Random.Int(80, 180))
            .RuleFor(m => m.IsActive, f => true)
            .RuleFor(m => m.CreatedAt, f => f.Date.Past(2))
            .RuleFor(m => m.UpdatedAt, f => DateTime.UtcNow);

        var movies = movieFaker.Generate(500);

        var batchSize = 100;
        for (int i = 0; i < movies.Count; i += batchSize)
        {
            var batch = movies.Skip(i).Take(batchSize).ToList();
            await _context.Movies.AddRangeAsync(batch);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Guardadas {Count} películas (batch {Batch})", batch.Count, i / batchSize + 1);
        }

        var moviesWithReviews = movies.Take((int)(movies.Count * 0.8)).ToList();
        var reviews = new List<Review>();

        var reviewComments = new[]
        {
            "Excelente película, muy recomendada",
            "Me encantó, una obra maestra",
            "Buena trama pero le faltó desarrollo",
            "Entretenida de principio a fin",
            "No cumplió mis expectativas",
            "Una joya del cine moderno",
            "Actuaciones increíbles",
            "La mejor película que he visto este año",
            "Predecible pero disfrutable",
            "Impresionante dirección y fotografía"
        };

        var reviewFaker = new Faker<Review>("es")
            .RuleFor(r => r.Id, f => Guid.NewGuid())
            .RuleFor(r => r.ReviewerName, f => f.Name.FullName())
            .RuleFor(r => r.Comment, f => f.PickRandom(reviewComments))
            .RuleFor(r => r.Rating, f => f.Random.Int(1, 5))
            .RuleFor(r => r.CreatedAt, f => f.Date.Past(1));

        foreach (var movie in moviesWithReviews)
        {
            var reviewCount = new Faker().Random.Int(2, 10);
            var movieReviews = reviewFaker.Generate(reviewCount);
            foreach (var review in movieReviews)
            {
                review.MovieId = movie.Id;
                reviews.Add(review);
            }
        }

        for (int i = 0; i < reviews.Count; i += batchSize)
        {
            var batch = reviews.Skip(i).Take(batchSize).ToList();
            await _context.Reviews.AddRangeAsync(batch);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Guardadas {Count} reseñas (batch {Batch})", batch.Count, i / batchSize + 1);
        }

        _logger.LogInformation("Seed completado: {MovieCount} películas y {ReviewCount} reseñas", movies.Count, reviews.Count);
    }
}
