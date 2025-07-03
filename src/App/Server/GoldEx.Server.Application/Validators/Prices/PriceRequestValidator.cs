using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Server.Infrastructure.DTOs;

namespace GoldEx.Server.Application.Validators.Prices;

[ScopedService]
internal class PriceRequestValidator : AbstractValidator<List<PriceResponse>>
{
    public PriceRequestValidator()
    {
        RuleFor(x => x)
            .Must(HasValidValues)
            .WithMessage("Price list must contain at least one valid price with a non-empty title and a positive current value.");
    }

    private bool HasValidValues(List<PriceResponse> list)
    {
        return list.Any() && list.All(price => !string.IsNullOrWhiteSpace(price.Title) && price.CurrentValue > 0);
    }
}