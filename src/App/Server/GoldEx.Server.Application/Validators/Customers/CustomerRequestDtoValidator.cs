using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Customers;
using GoldEx.Server.Infrastructure.Specifications.PriceUnits;
using GoldEx.Shared.DTOs.Customers;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Validators.Customers;

[ScopedService]
internal class CustomerRequestDtoValidator : AbstractValidator<CustomerRequestDto>
{
    private readonly ICustomerRepository _repository;
    private readonly IPriceUnitRepository _priceUnitRepository;
    public CustomerRequestDtoValidator(ICustomerRepository repository, IPriceUnitRepository priceUnitRepository)
    {
        _repository = repository;
        _priceUnitRepository = priceUnitRepository;

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

        RuleFor(x => x.CreditLimitPriceUnitId)
            .MustAsync(BeValidPriceUnitId)
            .WithMessage("واحد محدودیت اعتباری نامعتبر است");

        RuleFor(x => x)
            .Custom((dto, context) =>
            {
                var creditLimitHasValue = dto.CreditLimit.HasValue && dto.CreditLimit.Value > 0;
                var priceUnitHasValue = dto.CreditLimitPriceUnitId.HasValue;

                if (creditLimitHasValue != priceUnitHasValue) 
                    context.AddFailure("در صورت وارد کردن محدودیت اعتباری، وارد کردن واحد آن نیز الزامی است (و بالعکس)");
            });
    }

    private async Task<bool> BeValidPriceUnitId(CustomerRequestDto request, Guid? priceUnitId, CancellationToken cancellationToken = default)
    {
        if (!priceUnitId.HasValue)
            return true;

        return await _priceUnitRepository.ExistsAsync(new PriceUnitsByIdSpecification(new PriceUnitId(priceUnitId.Value)), cancellationToken);
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