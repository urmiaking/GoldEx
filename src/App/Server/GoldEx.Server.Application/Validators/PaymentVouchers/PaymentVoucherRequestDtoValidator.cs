using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.PaymentVouchers;
using GoldEx.Shared.DTOs.PaymentVouchers;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Validators.PaymentVouchers;

[ScopedService]
internal class PaymentVoucherRequestDtoValidator : AbstractValidator<PaymentVoucherRequestDto>
{
    private readonly IPaymentVoucherRepository _paymentVoucherRepository;
    public PaymentVoucherRequestDtoValidator(IPaymentVoucherRepository paymentVoucherRepository)
    {
        _paymentVoucherRepository = paymentVoucherRepository;

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("لطفا شرح را وارد کنید")
            .MaximumLength(150)
            .WithMessage("شرح نمی تواند بیشتر از 150 کاراکتر باشد");

        RuleFor(x => x.VoucherNumber)
            .MustAsync(BeUniqueNumber)
            .WithMessage("شماره رسید پرداخت باید یکتا باشد");

        RuleFor(x => x.VoucherType)
            .IsInEnum()
            .WithMessage("نوع رسید پرداخت نامعتبر است");
    }

    private async Task<bool> BeUniqueNumber(PaymentVoucherRequestDto request, long voucherNumber, CancellationToken cancellationToken)
    {
        var item = await _paymentVoucherRepository
            .Get(new PaymentVouchersByNumberSpecification(voucherNumber))
            .FirstOrDefaultAsync(cancellationToken);

        if (item is null)
            return true;

        return item.Id.Value == request.Id;
    }
}