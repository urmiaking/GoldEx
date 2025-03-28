using FluentValidation;
using GoldEx.Shared.Domain.Aggregates.CustomerAggregate;
using GoldEx.Shared.Infrastructure.Repositories.Abstractions;

namespace GoldEx.Shared.Application.Validators.Customers;

public class UpdateCustomerValidator<TCustomer> : AbstractValidator<TCustomer> where TCustomer : CustomerBase
{
    public UpdateCustomerValidator(ICustomerRepository<TCustomer> repository)
    {
        Include(new CreateCustomerValidator<TCustomer>(repository));

        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("شناسه مشتری نمی تواند خالی باشد");
    }
}