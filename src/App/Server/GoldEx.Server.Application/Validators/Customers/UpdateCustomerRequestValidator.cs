using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Customers;
using GoldEx.Shared.DTOs.Customers;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Validators.Customers;

[ScopedService]
internal class UpdateCustomerRequestValidator : AbstractValidator<(Guid id, UpdateCustomerRequest request)>
{
    private readonly ICustomerRepository _customerRepository;
    public UpdateCustomerRequestValidator(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;

        RuleFor(x => x.request.NationalId)
            .NotEmpty().WithMessage("وارد کردن شناسه یکتا الزامی است")
            .MaximumLength(25).WithMessage("حداکثر طول شناسه یکتا 25 کاراکتر می باشد")
            .MustAsync(BeUniqueNationalId).WithMessage("شناسه یکتا نمی تواند تکراری باشد");

        RuleFor(x => x.request.FullName)
            .NotEmpty().WithMessage("وارد کردن نام الزامی است")
            .MaximumLength(100).WithMessage("حداکثر طول نام 100 کاراکتر می باشد");

        RuleFor(x => x.request.PhoneNumber)
            .MaximumLength(25).WithMessage("حداکثر طول تلفن تماس 25 کاراکتر می باشد");

        RuleFor(x => x.request.Address)
            .MaximumLength(200).WithMessage("حداکثر طول آدرس 200 کاراکتر می باشد");

        RuleFor(x => x.request.CreditLimit)
            .GreaterThanOrEqualTo(0).WithMessage("محدودیت اعتباری نمی تواند منفی باشد");

        RuleFor(x => x.request.CreditLimitUnit)
            .IsInEnum().WithMessage("واحد محدودیت اعتباری باید یکی از مقادیر معتبر باشد");

        RuleFor(x => x.id)
            .NotEmpty().WithMessage("شناسه مشتری نمی تواند خالی باشد")
            .MustAsync(BeValidId)
            .WithMessage("مشتری با این شناسه وجود ندارد");
    }

    private async Task<bool> BeValidId(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await _customerRepository.Get(new CustomersByIdSpecification(new CustomerId(id)))
            .FirstOrDefaultAsync(cancellationToken);

        return item is not null;
    }

    private async Task<bool> BeUniqueNationalId((Guid id, UpdateCustomerRequest request) request, string nationalId, CancellationToken cancellationToken = default)
    {
        var item = await _customerRepository
            .Get(new CustomersByNationalIdSpecification(nationalId))
            .FirstOrDefaultAsync(cancellationToken);

        if (item is null)
            return true;

        return item.Id.Value == request.id;
    }
}