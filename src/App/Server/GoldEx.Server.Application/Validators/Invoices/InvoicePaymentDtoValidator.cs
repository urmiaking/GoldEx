using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Domain.PaymentMethodAggregate;
using GoldEx.Server.Domain.PaymentVoucherAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.PaymentMethods;
using GoldEx.Server.Infrastructure.Specifications.PaymentVouchers;
using GoldEx.Server.Infrastructure.Specifications.PriceUnits;
using GoldEx.Shared.DTOs.Invoices;

namespace GoldEx.Server.Application.Validators.Invoices;

[ScopedService]
internal class InvoicePaymentDtoValidator : AbstractValidator<InvoicePaymentDto>
{
    private readonly IPriceUnitRepository _priceUnitRepository;
    private readonly IPaymentMethodRepository _paymentMethodRepository;
    private readonly IPaymentVoucherRepository _paymentVoucherRepository;
    public InvoicePaymentDtoValidator(IPriceUnitRepository priceUnitRepository,
        IPaymentMethodRepository paymentMethodRepository,
        IPaymentVoucherRepository paymentVoucherRepository)
    {
        _priceUnitRepository = priceUnitRepository;
        _paymentMethodRepository = paymentMethodRepository;
        _paymentVoucherRepository = paymentVoucherRepository;

        RuleFor(x => x.PriceUnitId)
            .MustAsync(BeValidPriceUnit)
            .WithMessage("واحد ارزی پرداخت معتبر نمی باشد");

        RuleFor(x => x.PaymentMethodId)
            .MustAsync(BeValidPaymentMethod)
            .When(x => x.PaymentMethodId is not null)
            .WithMessage("روش پرداخت معتبر نمی باشد");

        RuleFor(x => x.VoucherId)
            .MustAsync(BeValidVoucherId)
            .When(x => x.VoucherId is not null)
            .WithMessage("شناسه سند پرداخت معتبر نمی باشد");
    }

    // TODO: add a checking method to check if the voucher is already used in another invoice payment
    private async Task<bool> BeValidVoucherId(Guid? voucherId, CancellationToken cancellationToken = default)
    {
        if (voucherId is null)
            return true;

        return await _paymentVoucherRepository.ExistsAsync(new PaymentVouchersByIdSpecification(new PaymentVoucherId(voucherId.Value)), cancellationToken);
    }

    private async Task<bool> BeValidPaymentMethod(Guid? id, CancellationToken cancellationToken = default)
    {
        if (id is null)
            return true;

        return await _paymentMethodRepository.ExistsAsync(new PaymentMethodsByIdSpecification(new PaymentMethodId(id.Value)), cancellationToken);
    }

    private async Task<bool> BeValidPriceUnit(Guid id, CancellationToken cancellationToken = default)
    {
        return await _priceUnitRepository.ExistsAsync(new PriceUnitsByIdSpecification(new PriceUnitId(id)), cancellationToken);
    }
}