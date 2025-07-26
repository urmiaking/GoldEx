using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Domain.PaymentVoucherAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;

namespace GoldEx.Server.Application.Validators.PaymentVouchers;

[ScopedService]
internal class DeletePaymentVoucherValidator : AbstractValidator<PaymentVoucher>
{
    private readonly IInvoiceRepository _invoiceRepository;
    public DeletePaymentVoucherValidator(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;

        RuleFor(x => x)
            .MustAsync(NotUsedByInvoices)
            .WithMessage("سند پرداخت در فاکتورها استفاده شده است و قابل حذف نمی باشد");
    }

    private Task<bool> NotUsedByInvoices(PaymentVoucher request, CancellationToken cancellationToken = default)
    {
        //TODO: implement this operation
        throw new NotImplementedException();
    }
}