using GoldEx.Client.Abstractions.LocalServices;
using GoldEx.Client.Offline.Domain.CustomerAggregate;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.Definitions;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Shared.Application.Services.Abstractions;
using GoldEx.Shared.Domain.Aggregates.CustomerAggregate;
using GoldEx.Shared.DTOs.Customers;
using MapsterMapper;

namespace GoldEx.Client.Services.LocalServices;

[ScopedService]
public class CustomerLocalClientService(IMapper mapper, ICustomerService<Customer> service) : ICustomerLocalClientService
{
    public async Task<PagedList<GetCustomerResponse>> GetListAsync(RequestFilter filter, CancellationToken cancellationToken = default)
    {
        var list = await service.GetListAsync(filter, cancellationToken);
        return mapper.Map<PagedList<GetCustomerResponse>>(list);
    }

    public async Task<GetCustomerResponse?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await service.GetAsync(new CustomerId(id), cancellationToken);
        return item is null ? null : mapper.Map<GetCustomerResponse>(item);
    }

    public async Task<GetCustomerResponse?> GetAsync(string nationalId, CancellationToken cancellationToken = default)
    {
        var item = await service.GetAsync(nationalId, cancellationToken);
        return item is null ? null : mapper.Map<GetCustomerResponse>(item);
    }

    public async Task<GetCustomerResponse?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default)
    {
        var item = await service.GetByPhoneNumberAsync(phoneNumber, cancellationToken);
        return item is null ? null : mapper.Map<GetCustomerResponse>(item);
    }

    public async Task<bool> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var customer = new Customer(request.CustomerType,
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
        var customer = await service.GetAsync(new CustomerId(id), cancellationToken);

        if (customer is null)
            return false;
        
        customer.SetCustomerType(request.CustomerType);
        customer.SetFullName(request.FullName);
        customer.SetNationalId(request.NationalId);
        customer.SetPhoneNumber(request.PhoneNumber);
        customer.SetAddress(request.Address);
        customer.SetCreditLimit(request.CreditLimit, request.CreditLimitUnit);

        await service.UpdateAsync(customer, cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, bool deletePermanently = false, CancellationToken cancellationToken = default)
    {
        var item = await service.GetAsync(new CustomerId(id), cancellationToken);

        if (item is null)
            return false;

        // In case the item is created locally and is not synced to server, it will be deleted permanently
        if (item.Status == ModifyStatus.Created)
        {
            await service.DeleteAsync(item, true, cancellationToken);
            return true;
        }

        if (deletePermanently)
        {
            await service.DeleteAsync(item, deletePermanently, cancellationToken);
        }
        else
        {
            item.SetStatus(ModifyStatus.Deleted);
            await service.UpdateAsync(item, cancellationToken);
        }

        return true;
    }

    public async Task<List<GetPendingCustomerResponse>> GetPendingsAsync(DateTime checkpointDate, CancellationToken cancellationToken = default)
    {
        var list = await service.GetPendingItemsAsync(checkpointDate, cancellationToken);
        return mapper.Map<List<GetPendingCustomerResponse>>(list);
    }

    public async Task SetSyncedAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await service.GetAsync(new CustomerId(id), cancellationToken)
                   ?? throw new NotFoundException("جنس یافت نشد");

        item.SetStatus(ModifyStatus.Synced);

        await service.UpdateAsync(item, cancellationToken);
    }

    public async Task CreateAsSyncedAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var customer = new Customer(new CustomerId(request.Id),
            request.CustomerType,
            request.FullName,
            request.NationalId,
            request.PhoneNumber,
            request.Address,
            request.CreditLimit,
            request.CreditLimitUnit);

        customer.SetStatus(ModifyStatus.Created);

        await service.CreateAsync(customer, cancellationToken);
    }

    public async Task UpdateAsSyncAsync(Guid id, UpdateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var customer = await service.GetAsync(new CustomerId(id), cancellationToken)
                       ?? throw new NotFoundException("مشتری یافت نشد");

        customer.SetCustomerType(request.CustomerType);
        customer.SetFullName(request.FullName);
        customer.SetNationalId(request.NationalId);
        customer.SetPhoneNumber(request.PhoneNumber);
        customer.SetAddress(request.Address);
        customer.SetCreditLimit(request.CreditLimit, request.CreditLimitUnit);

        customer.SetStatus(ModifyStatus.Synced);

        await service.UpdateAsync(customer, cancellationToken);
    }
}