using BackendTest.Application.DTOs;
using BackendTest.Application.Validators;
using FluentAssertions;

namespace BackendTest.Tests.Validators;

public class CreateMovieDtoValidatorTests
{
    private readonly CreateMovieDtoValidator _validator;

    public CreateMovieDtoValidatorTests()
    {
        _validator = new CreateMovieDtoValidator();
    }

    [Fact]
    public void Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var dto = new CreateMovieDto
        {
            Title = "Título de Película Válido",
            Description = "Descripción válida",
            ReleaseYear = 2023,
            Genre = "Acción",
            Director = "Director Válido",
            DurationMinutes = 120
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithEmptyTitle_ShouldHaveError()
    {
        // Arrange
        var dto = new CreateMovieDto
        {
            Title = "",
            Description = "Descripción",
            ReleaseYear = 2023,
            Genre = "Acción",
            Director = "Director"
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateMovieDto.Title));
    }

    [Fact]
    public void Validate_WithTitleTooLong_ShouldHaveError()
    {
        // Arrange
        var dto = new CreateMovieDto
        {
            Title = new string('A', 201),
            Description = "Descripción",
            ReleaseYear = 2023,
            Genre = "Acción",
            Director = "Director"
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == nameof(CreateMovieDto.Title) &&
            e.ErrorMessage.Contains("200"));
    }

    [Fact]
    public void Validate_WithDescriptionTooLong_ShouldHaveError()
    {
        // Arrange
        var dto = new CreateMovieDto
        {
            Title = "Título Válido",
            Description = new string('A', 2001),
            ReleaseYear = 2023,
            Genre = "Acción",
            Director = "Director"
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == nameof(CreateMovieDto.Description) &&
            e.ErrorMessage.Contains("2000"));
    }

    [Fact]
    public void Validate_WithReleaseYearTooEarly_ShouldHaveError()
    {
        // Arrange
        var dto = new CreateMovieDto
        {
            Title = "Título Válido",
            Description = "Descripción",
            ReleaseYear = 1887,
            Genre = "Acción",
            Director = "Director"
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == nameof(CreateMovieDto.ReleaseYear));
    }

    [Fact]
    public void Validate_WithReleaseYearInFuture_ShouldHaveError()
    {
        // Arrange
        var dto = new CreateMovieDto
        {
            Title = "Título Válido",
            Description = "Descripción",
            ReleaseYear = DateTime.Now.Year + 2,
            Genre = "Acción",
            Director = "Director"
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == nameof(CreateMovieDto.ReleaseYear));
    }

    [Fact]
    public void Validate_WithEmptyGenre_ShouldHaveError()
    {
        // Arrange
        var dto = new CreateMovieDto
        {
            Title = "Título Válido",
            Description = "Descripción",
            ReleaseYear = 2023,
            Genre = "",
            Director = "Director"
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateMovieDto.Genre));
    }

    [Fact]
    public void Validate_WithEmptyDirector_ShouldHaveError()
    {
        // Arrange
        var dto = new CreateMovieDto
        {
            Title = "Título Válido",
            Description = "Descripción",
            ReleaseYear = 2023,
            Genre = "Acción",
            Director = ""
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateMovieDto.Director));
    }

    [Fact]
    public void Validate_WithNegativeDuration_ShouldHaveError()
    {
        // Arrange
        var dto = new CreateMovieDto
        {
            Title = "Título Válido",
            Description = "Descripción",
            ReleaseYear = 2023,
            Genre = "Acción",
            Director = "Director",
            DurationMinutes = -10
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == nameof(CreateMovieDto.DurationMinutes));
    }

    [Fact]
    public void Validate_WithNullDuration_ShouldNotHaveError()
    {
        // Arrange
        var dto = new CreateMovieDto
        {
            Title = "Título Válido",
            Description = "Descripción",
            ReleaseYear = 2023,
            Genre = "Acción",
            Director = "Director",
            DurationMinutes = null
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
