using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Customers;
using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.Services;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class CustomerService(ICustomerRepository repository, IMapper mapper) : ICustomerService
{
    public async Task<PagedList<GetCustomerResponse>> GetListAsync(RequestFilter filter, CancellationToken cancellationToken = default)
    {
        var list = await repository.Get(new CustomersByFilterSpecification(filter)).ToListAsync(cancellationToken);

        return mapper.Map<PagedList<GetCustomerResponse>>(list);
    }

    public async Task<GetCustomerResponse> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await repository.Get(new CustomersByIdSpecification(new CustomerId(id)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        return mapper.Map<GetCustomerResponse>(item);
    }

    public async Task<GetCustomerResponse> GetAsync(string nationalId, CancellationToken cancellationToken = default)
    {
        var item = await repository.Get(new CustomersByNationalIdSpecification(nationalId))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        return mapper.Map<GetCustomerResponse>(item);
    }

    public async Task<GetCustomerResponse> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default)
    {
        var item = await repository.Get(new CustomersByPhoneNumberSpecification(phoneNumber))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        return mapper.Map<GetCustomerResponse>(item);
    }

    public async Task CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var item = Customer.Create(request.CustomerType,
            request.FullName,
            request.NationalId,
            request.PhoneNumber,
            request.Address,
            request.CreditLimit,
            request.CreditLimitUnit);

        await repository.CreateAsync(item, cancellationToken);
    }

    public async Task UpdateAsync(Guid id, UpdateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var item = await repository.Get(new CustomersByIdSpecification(new CustomerId(id)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        item.SetFullName(request.FullName);
        item.SetNationalId(request.NationalId);
        item.SetPhoneNumber(request.PhoneNumber);
        item.SetAddress(request.Address);
        item.SetCreditLimit(request.CreditLimit, request.CreditLimitUnit);
        item.SetCustomerType(request.CustomerType);

        await repository.UpdateAsync(item, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await repository.Get(new CustomersByIdSpecification(new CustomerId(id)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        await repository.DeleteAsync(item, cancellationToken);
    }
}