using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Shared.Application.Services.Abstractions;
using GoldEx.Shared.Domain.Aggregates.CustomerAggregate;
using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.Services;
using MapsterMapper;

namespace GoldEx.Server.ClientServices;

[ScopedService]
internal class CustomerClientService(ICustomerService<Customer> service, IMapper mapper) : ICustomerClientService
{
    public async Task<PagedList<GetCustomerResponse>> GetListAsync(RequestFilter filter, CancellationToken cancellationToken = default)
    {
        var list = await service.GetListAsync(filter, cancellationToken);

        return mapper.Map<PagedList<GetCustomerResponse>>(list);
    }

    public async Task<GetCustomerResponse?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await service.GetAsync(new CustomerId(id), cancellationToken) 
                   ?? throw new NotFoundException("مشتری پیدا نشد");

        return mapper.Map<GetCustomerResponse>(item);
    }

    public async Task<GetCustomerResponse?> GetAsync(string nationalId, CancellationToken cancellationToken = default)
    {
        var item = await service.GetAsync(nationalId, cancellationToken)
                   ?? throw new NotFoundException("مشتری پیدا نشد");

        return mapper.Map<GetCustomerResponse>(item);
    }

    public async Task<GetCustomerResponse?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default)
    {
        var item = await service.GetByPhoneNumberAsync(phoneNumber, cancellationToken)
                   ?? throw new NotFoundException("مشتری پیدا نشد");

        return mapper.Map<GetCustomerResponse>(item);
    }

    public async Task<bool> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var customer = new Customer(new CustomerId(request.Id),
            request.CustomerType,
            request.FullName,
            request.NationalId,
            request.PhoneNumber,
            request.Address,
            request.CreditLimit,
            request.CreditLimitUnit);

        await service.CreateAsync(customer, cancellationToken);

        return true;
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var customer = await service.GetAsync(new CustomerId(id), cancellationToken) ??
                       throw new NotFoundException("مشتری پیدا نشد");

        customer.SetFullName(request.FullName);
        customer.SetNationalId(request.NationalId);
        customer.SetPhoneNumber(request.PhoneNumber);
        customer.SetAddress(request.Address);
        customer.SetCreditLimit(request.CreditLimit, request.CreditLimitUnit);
        customer.SetCustomerType(request.CustomerType);

        await service.UpdateAsync(customer, cancellationToken);

        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, bool deletePermanently = false, CancellationToken cancellationToken = default)
    {
        var customer = await service.GetAsync(new CustomerId(id), cancellationToken) ??
                       throw new NotFoundException("مشتری پیدا نشد");

        await service.DeleteAsync(customer, deletePermanently, cancellationToken);

        return true;
    }

    public async Task<List<GetPendingCustomerResponse>> GetPendingsAsync(DateTime checkpointDate, CancellationToken cancellationToken = default)
    {
        var list = await service.GetPendingItemsAsync(checkpointDate, cancellationToken);

        return mapper.Map<List<GetPendingCustomerResponse>>(list);
    }
}