using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Domain.CoinAggregate;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Coins;
using GoldEx.Server.Infrastructure.Specifications.Customers;
using GoldEx.Shared.DTOs.CoinInstances;
using GoldEx.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Validators.CoinInstances;

[ScopedService]
internal class CoinInstanceDtoValidator : AbstractValidator<CoinInstanceRequestDto>
{
    private readonly ICoinRepository _coinRepository;
    private readonly ICustomerRepository _customerRepository;

    public CoinInstanceDtoValidator(ICoinRepository coinRepository, ICustomerRepository customerRepository)
    {
        _coinRepository = coinRepository;
        _customerRepository = customerRepository;

        RuleFor(x => x.Weight)
            .GreaterThan(0).WithMessage("وزن سکه باید بزرگتر از صفر باشد");

        RuleFor(x => x.Barcode)
            .MaximumLength(20)
            .WithMessage("طول بارکد سکه نمی‌تواند بیشتر از 20 کاراکتر باشد");

        RuleFor(x => x.Fineness)
            .GreaterThan(0).WithMessage("عیار سکه باید بزرگتر از صفر باشد")
            .LessThanOrEqualTo(1000).WithMessage("عیار نمی‌تواند بیشتر از 1000 باشد");

        RuleFor(x => x)
            .MustAsync(BeValidMintYearForCoinAsync)
            .WithMessage("سال ضرب سکه باید در بازه مجاز این سکه باشد");

        When(x => x.PackageType is CoinPackageType.VacuumSealed, () =>
        {
            RuleFor(x => x.CoinPackage)
                .NotNull().WithMessage("مشخصات بسته‌بندی سکه نمی‌تواند خالی باشد");

            RuleFor(x => x.CoinPackage!.VacuumedWeight)
                .GreaterThan(0).WithMessage("وزن با وکیوم سکه باید بزرگتر از صفر باشد");

             RuleFor(x => x.CoinPackage!.StandardCode)
                 .MaximumLength(30).WithMessage("طول کد استاندارد سکه نمی‌تواند بیشتر از 30 کاراکتر باشد");

             RuleFor(x => x.CoinPackage!.CardColor)
                 .MaximumLength(30).WithMessage("رنگ کارت سکه نامعتبر است");

             RuleFor(x => x.CoinPackage!.IssuerId)
                 .MustAsync(BeValidIssuerAsync).WithMessage("صادر کننده کارت برای بسته‌بندی سکه نامعتبر است");
        });

        When(x => x.MintType is CoinMintType.Banking, () =>
        {
            RuleFor(x => x.Weight)
                .MustAsync(EqualToCoinWeight)
                .WithMessage("وزن سکه‌های بانکی باید برابر با وزن استاندارد سکه باشد");

            RuleFor(x => x.Fineness)
                .MustAsync(EqualToCoinFineness)
                .WithMessage("عیار سکه‌های بانکی باید برابر با عیار استاندارد سکه باشد");
        });
    }

    private async Task<bool> BeValidMintYearForCoinAsync(CoinInstanceRequestDto dto, CancellationToken cancellationToken)
    {
        if (!dto.MintYear.HasValue)
            return true;

        var coin = await _coinRepository
            .Get(new CoinsByIdSpecification(new CoinId(dto.CoinId)))
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        if (coin is null)
            return false;

        if (dto.MintYear.Value < coin.StartMintYear)
            return false;

        if (coin.EndMintYear.HasValue &&
            dto.MintYear.Value > coin.EndMintYear.Value)
            return false;

        return true;
    }

    private async Task<bool> BeValidIssuerAsync(Guid? issuerId, CancellationToken cancellationToken)
    {
        if (!issuerId.HasValue)
            return true;

        var issuer = await _customerRepository
            .Get(new CustomersByIdSpecification(new CustomerId(issuerId.Value)))
            .FirstOrDefaultAsync(cancellationToken);

        if (issuer is null || issuer.CustomerType != CustomerType.Workshop)
            return false;

        return true;
    }

    private async Task<bool> EqualToCoinWeight(CoinInstanceRequestDto dto, decimal weight, CancellationToken cancellationToken)
    {
        var coin = await _coinRepository
            .Get(new CoinsByIdSpecification(new CoinId(dto.CoinId)))
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        if (coin is null)
            return false;

        return weight == coin.Weight;
    }

    private async Task<bool> EqualToCoinFineness(CoinInstanceRequestDto dto, decimal fineness, CancellationToken cancellationToken)
    {
        var coin = await _coinRepository
            .Get(new CoinsByIdSpecification(new CoinId(dto.CoinId)))
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        if (coin is null)
            return false;

        return fineness == coin.Fineness;
    }
}