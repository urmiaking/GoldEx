using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Domain.FinancialAccountAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Invoices;
using GoldEx.Server.Infrastructure.Specifications.PaymentVouchers;

namespace GoldEx.Server.Application.Validators.FinancialAccounts;

[ScopedService]
internal class DeleteFinancialAccountValidator : AbstractValidator<FinancialAccount>
{
    private readonly IPaymentVoucherRepository _paymentVoucherRepository;
    private readonly IInvoiceRepository _invoiceRepository;
    public DeleteFinancialAccountValidator(IPaymentVoucherRepository paymentVoucherRepository, IInvoiceRepository invoiceRepository)
    {
        _paymentVoucherRepository = paymentVoucherRepository;
        _invoiceRepository = invoiceRepository;

        RuleFor(x => x)
            .MustAsync(NotUsedInPaymentVouchers)
            .WithMessage("حساب مالی در اسناد پرداختی استفاده شده است و قابل حذف نیست.")
            .MustAsync(NotUsedInInvoicePayments)
            .WithMessage("حساب مالی در پرداخت فاکتورها استفاده شده است و قابل حذف نیست.");
    }

    private async Task<bool> NotUsedInInvoicePayments(FinancialAccount financialAccount, CancellationToken cancellationToken = default)
    {
        return !await _invoiceRepository.ExistsAsync(new InvoicesByFinancialAccountIdSpecification(financialAccount.Id), cancellationToken);
    }

    private async Task<bool> NotUsedInPaymentVouchers(FinancialAccount financialAccount, CancellationToken cancellationToken = default)
    {
        return !await _paymentVoucherRepository.ExistsAsync(new PaymentVouchersBySourceFinancialAccountIdSpecification(financialAccount.Id),
            cancellationToken);
    }
}