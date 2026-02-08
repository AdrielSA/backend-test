using BackendTest.Application.DTOs;
using FluentValidation;

namespace BackendTest.Application.Validators;

public class GetMoviesQueryValidator : AbstractValidator<GetMoviesQueryDto>
{
    public GetMoviesQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("El número de página debe ser mayor a 0.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("El tamaño de página debe estar entre 1 y 100.");

        RuleFor(x => x.SortBy)
            .IsInEnum()
            .WithMessage("El campo de ordenamiento no es válido.");
    }
}
