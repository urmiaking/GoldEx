using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Customers;
using GoldEx.Shared.DTOs.Customers;

namespace GoldEx.Server.Application.Validators.Customers;

[ScopedService]
internal class CreateCustomerRequestValidator : AbstractValidator<CreateCustomerRequest>
{
    private readonly ICustomerRepository _repository;
    public CreateCustomerRequestValidator(ICustomerRepository repository)
    {
        _repository = repository;

        RuleFor(x => x.NationalId)
            .NotEmpty().WithMessage("وارد کردن شناسه یکتا الزامی است")
            .MaximumLength(25).WithMessage("حداکثر طول شناسه یکتا 25 کاراکتر می باشد")
            .MustAsync(BeUniqueNationalId).WithMessage("شناسه یکتا نمی تواند تکراری باشد");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("وارد کردن نام الزامی است")
            .MaximumLength(100).WithMessage("حداکثر طول نام 100 کاراکتر می باشد");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(25).WithMessage("حداکثر طول تلفن تماس 25 کاراکتر می باشد");

        RuleFor(x => x.Address)
            .MaximumLength(200).WithMessage("حداکثر طول آدرس 200 کاراکتر می باشد");

        RuleFor(x => x.CreditLimit)
            .GreaterThanOrEqualTo(0).WithMessage("محدودیت اعتباری نمی تواند منفی باشد");

        RuleFor(x => x.CreditLimitUnit)
            .IsInEnum().WithMessage("واحد محدودیت اعتباری باید یکی از مقادیر معتبر باشد");
    }

    private async Task<bool> BeUniqueNationalId(string nationalId, CancellationToken cancellationToken = default)
    {
        return !await _repository.ExistsAsync(new CustomersByNationalIdSpecification(nationalId), cancellationToken);
    }
}