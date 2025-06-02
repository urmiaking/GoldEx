using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Customers;
using GoldEx.Shared.DTOs.Customers;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Validators.Customers;

[ScopedService]
internal class CustomerRequestDtoValidator : AbstractValidator<CustomerRequestDto>
{
    private readonly ICustomerRepository _repository;
    public CustomerRequestDtoValidator(ICustomerRepository repository)
    {
        _repository = repository;

        RuleFor(x => x.Id)
            .MustAsync(BeValidId).WithMessage("شناسه مشتری نامعتبر است")
            .When(x => x.Id.HasValue);

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

    private async Task<bool> BeValidId(Guid? id, CancellationToken cancellationToken = default)
    {
        if (!id.HasValue)
            return true;

        var item = await _repository
            .Get(new CustomersByIdSpecification(new CustomerId(id.Value)))
            .FirstOrDefaultAsync(cancellationToken);

        return item is not null;
    }

    private async Task<bool> BeUniqueNationalId(CustomerRequestDto request, string nationalId, CancellationToken cancellationToken = default)
    {
        var item = await _repository
            .Get(new CustomersByNationalIdSpecification(nationalId))
            .FirstOrDefaultAsync(cancellationToken);

        if (item is null)
            return true;

        return item.Id.Value == request.Id;
    }
}