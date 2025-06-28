using FluentValidation;
using Application.Commands;

namespace Application.Validators;

/// <summary>
/// Validator for CancelOrderCommand
/// </summary>
public class CancelOrderCommandValidator : AbstractValidator<CancelOrderCommand>
{
    public CancelOrderCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("OrderId is required");
    }
}
