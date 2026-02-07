using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.LicensePayments;
using GoldEx.Shared.DTOs.LicensePayments;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Application.Validators.Licenses;

[ScopedService]
internal class LicensePaymentRequestValidator : AbstractValidator<LicensePaymentRequest>
{
    private readonly int[] _validMonths = [1, 3, 6, 12];
    private readonly ILicensePaymentRepository _repository;

    public LicensePaymentRequestValidator(ILicensePaymentRepository repository)
    {
        _repository = repository;

        RuleFor(x => x.RequestedMonths)
            .Must(x => _validMonths.Contains(x))
            .WithMessage("مدت اشتراک نامعتبر است");

        RuleFor(x => x.PaymentReference)
            .NotEmpty().WithMessage("وارد کردن شماره مرجع پرداخت الزامی است")
            .MaximumLength(200).WithMessage("حداکثر طول شماره مرجع پرداخت 200 کاراکتر است");

        RuleFor(x => x.PaymentDescription)
            .MaximumLength(1000).WithMessage("حداکثر طول توضیحات 1000 کاراکتر است");

        RuleFor(x => x)
            .MustAsync(HasNoPendingRequest)
            .WithMessage("درخواست شما از قبل ثبت شده است. لطفا تا رسیدگی به درخواست شکیبا باشید");
    }

    private async Task<bool> HasNoPendingRequest(LicensePaymentRequest request, CancellationToken cancellationToken = default)
    {
        return !await _repository.ExistsAsync(new LicensePaymentsByStatusSpecification(LicensePaymentStatus.Pending),
            cancellationToken);
    }
}