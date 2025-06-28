using FluentValidation;
using Application.Queries;

namespace Application.Validators;

/// <summary>
/// Validator for GetOrderByIdQuery
/// </summary>
public class GetOrderByIdQueryValidator : AbstractValidator<GetOrderByIdQuery>
{
    public GetOrderByIdQueryValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("OrderId is required");
    }
}

/// <summary>
/// Validator for GetOrdersQuery
/// </summary>
public class GetOrdersQueryValidator : AbstractValidator<GetOrdersQuery>
{
    public GetOrdersQueryValidator()
    {
        RuleFor(x => x.Skip)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Skip must be greater than or equal to 0");

        RuleFor(x => x.Take)
            .GreaterThan(0)
            .LessThanOrEqualTo(1000)
            .WithMessage("Take must be between 1 and 1000");
    }
}
