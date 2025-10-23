using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.FinancialAccountAggregate;
using GoldEx.Server.Domain.PaymentVoucherAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Customers;
using GoldEx.Server.Infrastructure.Specifications.FinancialAccounts;
using GoldEx.Server.Infrastructure.Specifications.PaymentVouchers;
using GoldEx.Server.Infrastructure.Specifications.PriceUnits;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Application.Validators.Invoices;

[ScopedService]
internal class InvoicePaymentDtoValidator : AbstractValidator<InvoicePaymentDto>
{
    private readonly IPriceUnitRepository _priceUnitRepository;
    private readonly IFinancialAccountRepository _financialAccountRepository;
    private readonly IPaymentVoucherRepository _paymentVoucherRepository;

    public InvoicePaymentDtoValidator(
        IPriceUnitRepository priceUnitRepository,
        IFinancialAccountRepository financialAccountRepository,
        IPaymentVoucherRepository paymentVoucherRepository,
        ICustomerRepository customerRepository)
    {
        _priceUnitRepository = priceUnitRepository;
        _financialAccountRepository = financialAccountRepository;
        _paymentVoucherRepository = paymentVoucherRepository;

        // PriceUnit همیشه بررسی می‌شود
        RuleFor(x => x.PriceUnitId)
            .MustAsync(BeValidPriceUnit)
            .WithMessage("واحد ارزی پرداخت معتبر نمی باشد");

        // Amount اجباری و مثبت باشد
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("مقدار پرداخت باید بزرگتر از صفر باشد");

        // PaymentDate اجباری
        RuleFor(x => x.PaymentDate)
            .NotEmpty()
            .WithMessage("تاریخ پرداخت باید مشخص شود");

        // VoucherId اگر پر شده باید معتبر باشد
        When(x => x.VoucherId.HasValue, () =>
        {
            RuleFor(x => x.VoucherId)
                .MustAsync(BeValidVoucherId)
                .WithMessage("شناسه سند پرداخت معتبر نمی باشد");
        });

        // PaymentType-based rules
        RuleFor(x => x).CustomAsync(async (payment, context, cancellationToken) =>
        {
            switch (payment.PaymentType)
            {
                case PaymentType.InternalCash:
                    if (!payment.FinancialAccountId.HasValue)
                    {
                        context.AddFailure(nameof(payment.FinancialAccountId),
                            "حساب مالی برای پرداخت نقدی یا بانکی الزامی است.");
                    }
                    else
                    {
                        var exists = await BeValidFinancialAccount(payment.FinancialAccountId, cancellationToken);
                        if (!exists)
                        {
                            context.AddFailure(nameof(payment.FinancialAccountId),
                                "حساب مالی معتبر نمی باشد.");
                        }
                    }
                    break;

                case PaymentType.MoltenGoldInventory:
                case PaymentType.UsedGoldInventory:
                    if (payment.FinancialAccountId.HasValue)
                    {
                        context.AddFailure(nameof(payment.FinancialAccountId),
                            "برای پرداخت با طلای آبشده یا شکسته، حساب مالی نباید انتخاب شود.");
                    }
                    if (payment.CustomerId.HasValue)
                    {
                        context.AddFailure(nameof(payment.CustomerId),
                            "برای پرداخت با طلای آبشده یا شکسته، مشتری نباید انتخاب شود.");
                    }
                    break;

                case PaymentType.CustomerTransfer:
                    if (!payment.CustomerId.HasValue)
                    {
                        context.AddFailure(nameof(payment.CustomerId),
                            "برای حواله‌کرد مشتری، شناسه مشتری الزامی است.");
                    }
                    else
                    {
                        var exists = await customerRepository.ExistsAsync(new CustomersByIdSpecification(new CustomerId(payment.CustomerId.Value)), cancellationToken);
                        if (!exists)
                        {
                            context.AddFailure(nameof(payment.CustomerId),
                                "مشتری انتخاب شده معتبر نمی باشد.");
                        }
                    }

                    // برای حواله‌کرد، FinancialAccountId نباید پر شود
                    if (payment.FinancialAccountId.HasValue)
                    {
                        context.AddFailure(nameof(payment.FinancialAccountId),
                            "برای حواله‌کرد مشتری، حساب مالی نباید انتخاب شود.");
                    }
                    break;

                default:
                    context.AddFailure(nameof(payment.PaymentType), "نوع پرداخت نامعتبر است.");
                    break;
            }
        });
    }

    private async Task<bool> BeValidVoucherId(Guid? voucherId, CancellationToken cancellationToken = default)
    {
        if (!voucherId.HasValue) return true;

        return await _paymentVoucherRepository.ExistsAsync(
            new PaymentVouchersByIdSpecification(new PaymentVoucherId(voucherId.Value)),
            cancellationToken);
    }

    private async Task<bool> BeValidFinancialAccount(Guid? id, CancellationToken cancellationToken = default)
    {
        if (!id.HasValue) return true;

        return await _financialAccountRepository.ExistsAsync(
            new FinancialAccountsByIdSpecification(new FinancialAccountId(id.Value)), cancellationToken);
    }

    private async Task<bool> BeValidPriceUnit(Guid id, CancellationToken cancellationToken = default)
    {
        return await _priceUnitRepository.ExistsAsync(new PriceUnitsByIdSpecification(new PriceUnitId(id)), cancellationToken);
    }
}