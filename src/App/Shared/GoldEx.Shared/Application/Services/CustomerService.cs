using FluentValidation;
using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.Application.Services.Abstractions;
using GoldEx.Shared.Application.Validators.Customers;
using GoldEx.Shared.Domain.Aggregates.CustomerAggregate;
using GoldEx.Shared.Infrastructure.Repositories.Abstractions;

namespace GoldEx.Shared.Application.Services;

public class CustomerService<TCustomer>(
    ICustomerRepository<TCustomer> repository,
    CreateCustomerValidator<TCustomer> createValidator,
    UpdateCustomerValidator<TCustomer> updateValidator,
    DeleteCustomerValidator<TCustomer> deleteValidator) : ICustomerService<TCustomer> where TCustomer : CustomerBase
{
    public async Task CreateAsync(TCustomer customer, CancellationToken cancellationToken = default)
    {
        await createValidator.ValidateAndThrowAsync(customer, cancellationToken);
        await repository.CreateAsync(customer, cancellationToken);
    }

    public async Task UpdateAsync(TCustomer customer, CancellationToken cancellationToken = default)
    {
        await updateValidator.ValidateAndThrowAsync(customer, cancellationToken);
        await repository.UpdateAsync(customer, cancellationToken);
    }

    public async Task DeleteAsync(TCustomer customer, bool deletePermanently = false, CancellationToken cancellationToken = default)
    {
        await deleteValidator.ValidateAndThrowAsync(customer, cancellationToken);
        await repository.DeleteAsync(customer, deletePermanently, cancellationToken);
    }

    public Task<TCustomer?> GetAsync(CustomerId id, CancellationToken cancellationToken = default)
        => repository.GetAsync(id, cancellationToken);

    public Task<TCustomer?> GetAsync(string nationalId, CancellationToken cancellationToken = default)
        => repository.GetAsync(nationalId, cancellationToken);

    public Task<TCustomer?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default)
        => repository.GetAsync(phoneNumber, cancellationToken);

    public Task<PagedList<TCustomer>> GetListAsync(RequestFilter filter, CancellationToken cancellationToken = default)
        => repository.GetListAsync(filter, cancellationToken);

    public Task<List<TCustomer>> GetPendingItemsAsync(DateTime checkpointDate, CancellationToken cancellationToken = default)
        => repository.GetPendingItemsAsync(checkpointDate, cancellationToken);
}