using FluentValidation;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.PriceUnits;
using GoldEx.Shared.DTOs.Invoices;

namespace GoldEx.Server.Application.Validators.Invoices;

internal class InvoiceCurrencyItemDtoValidator : AbstractValidator<InvoiceCurrencyItemDto>
{
    private readonly IPriceUnitRepository _priceUnitRepository;
    public InvoiceCurrencyItemDtoValidator(IPriceUnitRepository priceUnitRepository)
    {
        _priceUnitRepository = priceUnitRepository;

        RuleFor(x => x.ProfitPercent)
            .GreaterThanOrEqualTo(0).WithMessage("درصد سود نمی‌تواند منفی باشد")
            .LessThanOrEqualTo(100).WithMessage("درصد سود نمی‌تواند بیشتر از 100 باشد");

        RuleFor(x => x.TaxPercent)
            .GreaterThanOrEqualTo(0).WithMessage("درصد مالیات نمی‌تواند منفی باشد")
            .LessThanOrEqualTo(100).WithMessage("درصد مالیات نمی‌تواند بیشتر از 100 باشد");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("حجم ارز باید بزرگتر از صفر باشد");

        RuleFor(x => x.CurrencyId)
            .NotEmpty().WithMessage("لطفا ارز را انتخاب کنید")
            .MustAsync(BeValidCurrency).WithMessage("ارز انتخابی نامعتبر است");

        RuleFor(x => x.FinancialAccountId)
            .NotEmpty().WithMessage("لطفا حساب مالی را انتخاب کنید");
    }

    private async Task<bool> BeValidCurrency(Guid priceUnitId, CancellationToken cancellationToken = default)
    {
        return await _priceUnitRepository.ExistsAsync(
            new PriceUnitsByIdSpecification(new PriceUnitId(priceUnitId)), cancellationToken);
    }
}