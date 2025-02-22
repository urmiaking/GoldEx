using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Domain.PriceAggregate;

namespace GoldEx.Server.Application.Validators;

[ScopedService<IValidator<Price>>]
public class PriceValidator : AbstractValidator<Price>
{
    public PriceValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title cannot be empty");
    }
}