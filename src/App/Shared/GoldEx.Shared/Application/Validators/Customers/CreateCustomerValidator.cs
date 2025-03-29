using FluentValidation;
using GoldEx.Shared.Domain.Aggregates.CustomerAggregate;
using GoldEx.Shared.Infrastructure.Repositories.Abstractions;

namespace GoldEx.Shared.Application.Validators.Customers;

public class CreateCustomerValidator<TCustomer> : AbstractValidator<TCustomer> where TCustomer : CustomerBase
{
    private readonly ICustomerRepository<TCustomer> _repository;
    public CreateCustomerValidator(ICustomerRepository<TCustomer> repository)
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
    }

    private async Task<bool> BeUniqueNationalId(TCustomer customer, string nationalId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(nationalId))
            return true; // Let the NotEmpty rule handle this.

        var existingCustomer = await _repository.GetAsync(nationalId, cancellationToken);

        if (existingCustomer == null)
            return true; // No existing customer with this nationalId, so it's unique.

        // Check if it's the same customer (update scenario)
        return customer.Id.Value != Guid.Empty && customer.Id == existingCustomer.Id;
    }
}