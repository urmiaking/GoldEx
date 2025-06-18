using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Domain.PaymentMethodAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.PaymentMethods;
using GoldEx.Server.Infrastructure.Specifications.PriceUnits;
using GoldEx.Shared.DTOs.Invoices;

namespace GoldEx.Server.Application.Validators.Invoices;

[ScopedService]
internal class InvoicePaymentDtoValidator : AbstractValidator<InvoicePaymentDto>
{
    private readonly IPriceUnitRepository _priceUnitRepository;
    private readonly IPaymentMethodRepository _paymentMethodRepository;
    public InvoicePaymentDtoValidator(IPriceUnitRepository priceUnitRepository, IPaymentMethodRepository paymentMethodRepository)
    {
        _priceUnitRepository = priceUnitRepository;
        _paymentMethodRepository = paymentMethodRepository;

        RuleFor(x => x.PriceUnitId)
            .MustAsync(BeValidPriceUnit)
            .WithMessage("واحد ارزی پرداخت معتبر نمی باشد");

        RuleFor(x => x.PaymentMethodId)
            .MustAsync(BeValidPaymentMethod)
            .WithMessage("روش پرداخت معتبر نمی باشد");
    }

    private async Task<bool> BeValidPaymentMethod(Guid id, CancellationToken cancellationToken = default)
    {
        return await _paymentMethodRepository.ExistsAsync(new PaymentMethodsByIdSpecification(new PaymentMethodId(id)), cancellationToken);
    }

    private async Task<bool> BeValidPriceUnit(Guid id, CancellationToken cancellationToken = default)
    {
        return await _priceUnitRepository.ExistsAsync(new PriceUnitsByIdSpecification(new PriceUnitId(id)), cancellationToken);
    }
}