using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Domain.CoinAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Coins;
using GoldEx.Shared.DTOs.Invoices;

namespace GoldEx.Server.Application.Validators.Invoices;

[ScopedService]
internal class InvoiceCoinItemDtoValidator : AbstractValidator<InvoiceCoinItemDto>
{
    private readonly ICoinRepository _coinRepository;
    public InvoiceCoinItemDtoValidator(ICoinRepository coinRepository)
    {
        _coinRepository = coinRepository;

        RuleFor(x => x.ProfitPercent)
            .GreaterThanOrEqualTo(0).WithMessage("درصد سود نمی‌تواند منفی باشد")
            .LessThanOrEqualTo(100).WithMessage("درصد سود نمی‌تواند بیشتر از 100 باشد");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("تعداد باید بزرگتر از صفر باشد");

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0).WithMessage("نرخ واحد باید بزرگتر از صفر باشد");

        RuleFor(x => x.CoinId)
            .NotEmpty().WithMessage("لطفا نوع سکه را انتخاب کنید")
            .MustAsync(BeValidCoin).WithMessage("شناسه سکه نامعتبر است");
    }

    private async Task<bool> BeValidCoin(Guid coinId, CancellationToken cancellationToken = default) =>
        await _coinRepository.ExistsAsync(new CoinsByIdSpecification(new CoinId(coinId)), cancellationToken);
}