using BackendTest.Application.DTOs;
using BackendTest.Application.Validators;
using FluentAssertions;

namespace BackendTest.Tests.Validators;

public class CreateReviewDtoValidatorTests
{
    private readonly CreateReviewDtoValidator _validator;

    public CreateReviewDtoValidatorTests()
    {
        _validator = new CreateReviewDtoValidator();
    }

    [Fact]
    public void Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var dto = new CreateReviewDto
        {
            ReviewerName = "Juan Pérez",
            Comment = "¡Excelente película!",
            Rating = 5
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithEmptyReviewerName_ShouldHaveError()
    {
        // Arrange
        var dto = new CreateReviewDto
        {
            ReviewerName = "",
            Comment = "¡Excelente película!",
            Rating = 5
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateReviewDto.ReviewerName));
    }

    [Fact]
    public void Validate_WithReviewerNameTooLong_ShouldHaveError()
    {
        // Arrange
        var dto = new CreateReviewDto
        {
            ReviewerName = new string('A', 101),
            Comment = "¡Excelente película!",
            Rating = 5
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == nameof(CreateReviewDto.ReviewerName) &&
            e.ErrorMessage.Contains("100"));
    }

    [Fact]
    public void Validate_WithEmptyComment_ShouldHaveError()
    {
        // Arrange
        var dto = new CreateReviewDto
        {
            ReviewerName = "Juan Pérez",
            Comment = "",
            Rating = 5
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateReviewDto.Comment));
    }

    [Fact]
    public void Validate_WithCommentTooLong_ShouldHaveError()
    {
        // Arrange
        var dto = new CreateReviewDto
        {
            ReviewerName = "Juan Pérez",
            Comment = new string('A', 1001),
            Rating = 5
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == nameof(CreateReviewDto.Comment) &&
            e.ErrorMessage.Contains("1000"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithRatingBelowMinimum_ShouldHaveError(int rating)
    {
        // Arrange
        var dto = new CreateReviewDto
        {
            ReviewerName = "Juan Pérez",
            Comment = "Comentario de prueba",
            Rating = rating
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == nameof(CreateReviewDto.Rating));
    }

    [Theory]
    [InlineData(6)]
    [InlineData(10)]
    public void Validate_WithRatingAboveMaximum_ShouldHaveError(int rating)
    {
        // Arrange
        var dto = new CreateReviewDto
        {
            ReviewerName = "Juan Pérez",
            Comment = "Comentario de prueba",
            Rating = rating
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == nameof(CreateReviewDto.Rating));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void Validate_WithValidRatings_ShouldNotHaveErrors(int rating)
    {
        // Arrange
        var dto = new CreateReviewDto
        {
            ReviewerName = "Juan Pérez",
            Comment = "Comentario de prueba",
            Rating = rating
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
