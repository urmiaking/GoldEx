using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Application.Validators.CoinInstances;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Shared.DTOs.Invoices;

namespace GoldEx.Server.Application.Validators.Invoices;

[ScopedService]
internal class InvoiceCoinItemDtoValidator : AbstractValidator<InvoiceCoinItemDto>
{
    public InvoiceCoinItemDtoValidator(ICoinRepository coinRepository, ICustomerRepository customerRepository)
    {
        RuleFor(x => x.ProfitPercent)
            .GreaterThanOrEqualTo(0).WithMessage("درصد سود نمی‌تواند منفی باشد")
            .LessThanOrEqualTo(100).WithMessage("درصد سود نمی‌تواند بیشتر از 100 باشد");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("تعداد باید بزرگتر از صفر باشد");

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0).WithMessage("نرخ واحد باید بزرگتر از صفر باشد");

        RuleFor(x => x.CoinInstance)
            .SetValidator(new CoinInstanceDtoValidator(coinRepository, customerRepository));
    }
}