using FluentValidation;
using GoldEx.Shared.Domain.Aggregates.PriceAggregate;
using GoldEx.Shared.Domain.Aggregates.PriceHistoryAggregate;

namespace GoldEx.Shared.Application.Validators;

public class PriceValidator<TPrice, TPriceHistory> : AbstractValidator<TPrice>
    where TPrice : PriceBase<TPriceHistory>
    where TPriceHistory : PriceHistoryBase
{
    public PriceValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title cannot be empty");
    }
}