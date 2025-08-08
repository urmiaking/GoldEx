using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Domain.PriceAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Prices;
using GoldEx.Shared.DTOs.Coins;

namespace GoldEx.Server.Application.Validators.Coins;

[ScopedService]
internal class CoinRequestDtoValidator : AbstractValidator<CoinRequestDto>
{
    private readonly IPriceRepository _priceRepository;
    public CoinRequestDtoValidator(IPriceRepository priceRepository)
    {
        _priceRepository = priceRepository;

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("عنوان سکه الزامی است")
            .MaximumLength(100)
            .WithMessage("عنوان سکه باید کمتر از 100 کاراکتر باشد");

        RuleFor(x => x.CoinType)
            .IsInEnum()
            .WithMessage("نوع سکه معتبر نمی باشد");

        RuleFor(x => x.PriceId)
            .MustAsync(BeValidPrice)
            .WithMessage("شناسه قیمت وابسته نامعتبر است");
    }

    private async Task<bool> BeValidPrice(Guid? priceId, CancellationToken cancellationToken = default)
    {
        if (!priceId.HasValue)
            return true;

        return await _priceRepository.ExistsAsync(new PricesByIdSpecification(new PriceId(priceId.Value)), cancellationToken);
    }
}