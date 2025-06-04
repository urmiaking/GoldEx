using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Shared.DTOs.PriceUnits;

namespace GoldEx.Server.Application.Validators.PriceUnits;

[ScopedService]
internal class CreatePriceUnitRequestValidator : AbstractValidator<CreatePriceUnitRequest>
{
    public CreatePriceUnitRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("عنوان نمی تواند خالی باشد")
            .MaximumLength(100)
            .WithMessage("عنوان نمی تواند بیشتر از 100 کاراکتر باشد");
    }
}