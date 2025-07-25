using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Domain.FinancialAccountAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.PaymentVouchers;

namespace GoldEx.Server.Application.Validators.FinancialAccounts;

[ScopedService]
internal class DeleteFinancialAccountValidator : AbstractValidator<FinancialAccount>
{
    private readonly IPaymentVoucherRepository _paymentVoucherRepository;
    public DeleteFinancialAccountValidator(IPaymentVoucherRepository paymentVoucherRepository)
    {
        _paymentVoucherRepository = paymentVoucherRepository;

        RuleFor(x => x)
            .MustAsync(NotUsedInPaymentVouchers)
            .WithMessage("حساب مالی در پرداخت ها استفاده شده است و قابل حذف نیست.");
    }

    private Task<bool> NotUsedInPaymentVouchers(FinancialAccount financialAccount, CancellationToken cancellationToken = default)
    {
        return _paymentVoucherRepository.ExistsAsync(new PaymentVouchersByFinancialAccountIdSpecification(financialAccount.Id),
            cancellationToken);
    }
}