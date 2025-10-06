using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.PriceUnits;
using GoldEx.Shared.DTOs.PriceUnits;

namespace GoldEx.Server.Application.Validators.PriceUnits;

[ScopedService]
internal class CreatePriceUnitRequestValidator : AbstractValidator<CreatePriceUnitRequest>
{
    private readonly IPriceUnitRepository _priceUnitRepository;
    public CreatePriceUnitRequestValidator(IPriceUnitRepository priceUnitRepository)
    {
        _priceUnitRepository = priceUnitRepository;

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("عنوان نمی تواند خالی باشد")
            .MaximumLength(100)
            .WithMessage("عنوان نمی تواند بیشتر از 100 کاراکتر باشد")
            .MustAsync(BeUniqueTitle)
            .WithMessage("عنوان واحد ارزی باید یکتا باشد");
    }

    private async Task<bool> BeUniqueTitle(string title, CancellationToken cancellationToken = default)
    {
        return !await _priceUnitRepository.ExistsAsync(new PriceUnitsByTitleSpecification(title), cancellationToken);
    }
}