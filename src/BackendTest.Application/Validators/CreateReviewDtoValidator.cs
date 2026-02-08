using BackendTest.Application.DTOs;
using FluentValidation;

namespace BackendTest.Application.Validators;

public class CreateReviewDtoValidator : AbstractValidator<CreateReviewDto>
{
    public CreateReviewDtoValidator()
    {
        RuleFor(x => x.ReviewerName)
            .NotEmpty().WithMessage("El nombre del revisor es requerido.")
            .MaximumLength(100).WithMessage("El nombre no puede exceder los 100 caracteres.");

        RuleFor(x => x.Comment)
            .NotEmpty().WithMessage("El comentario es requerido.")
            .MaximumLength(1000).WithMessage("El comentario no puede exceder los 1000 caracteres.");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5)
            .WithMessage("La calificación debe estar entre 1 y 5.");
    }
}
