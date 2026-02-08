using BackendTest.Application.DTOs;
using FluentValidation;

namespace BackendTest.Application.Validators;

public class CreateMovieDtoValidator : AbstractValidator<CreateMovieDto>
{
    public CreateMovieDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("El título es requerido.")
            .MaximumLength(200).WithMessage("El título no puede exceder los 200 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("La descripción no puede exceder los 2000 caracteres.");

        RuleFor(x => x.ReleaseYear)
            .InclusiveBetween(1888, DateTime.Now.Year)
            .WithMessage($"El año de lanzamiento debe estar entre 1888 y {DateTime.Now.Year}.");

        RuleFor(x => x.Genre)
            .NotEmpty().WithMessage("El género es requerido.");

        RuleFor(x => x.Director)
            .NotEmpty().WithMessage("El director es requerido.");

        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0).When(x => x.DurationMinutes.HasValue)
            .WithMessage("La duración debe ser mayor a 0 minutos.");
    }
}
