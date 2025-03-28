using FluentValidation;
using GoldEx.Shared.Domain.Aggregates.CustomerAggregate;

namespace GoldEx.Shared.Application.Validators.Customers;

public class DeleteCustomerValidator<TCustomer> : AbstractValidator<TCustomer> where TCustomer : CustomerBase
{
    public DeleteCustomerValidator()
    {
        // TODO: validate if customer has transactions
    }
}