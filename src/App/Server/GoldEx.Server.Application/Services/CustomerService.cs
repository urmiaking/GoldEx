using FluentValidation;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Server.Application.Validators.Customers;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Customers;
using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.Services;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class CustomerService(ICustomerRepository repository, IMapper mapper, CustomerRequestDtoValidator validator) : ICustomerService
{
    public async Task<PagedList<GetCustomerResponse>> GetListAsync(RequestFilter filter, CancellationToken cancellationToken = default)
    {
        var skip = filter.Skip ?? 0;
        var take = filter.Take ?? 100;

        var data = await repository
            .Get(new CustomersByFilterSpecification(filter))
            .Include(x => x.CreditLimitPriceUnit)
            .ToListAsync(cancellationToken);

        var totalCount = await repository.CountAsync(new CustomersByFilterSpecification(filter), cancellationToken);

        return new PagedList<GetCustomerResponse>
        {
            Data = mapper.Map<List<GetCustomerResponse>>(data),
            Skip = skip,
            Take = take,
            Total = totalCount
        };
    }

    public async Task<GetCustomerResponse> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await repository
            .Get(new CustomersByIdSpecification(new CustomerId(id)))
            .Include(x => x.CreditLimitPriceUnit)
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        return mapper.Map<GetCustomerResponse>(item);
    }

    public async Task<GetCustomerResponse?> GetAsync(string nationalId, CancellationToken cancellationToken = default)
    {
        var item = await repository
            .Get(new CustomersByNationalIdSpecification(nationalId))
            .Include(x => x.CreditLimitPriceUnit)
            .FirstOrDefaultAsync(cancellationToken);

        return item is not null ? mapper.Map<GetCustomerResponse>(item) : null;
    }

    public async Task<GetCustomerResponse> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default)
    {
        var item = await repository
            .Get(new CustomersByPhoneNumberSpecification(phoneNumber))
            .Include(x => x.CreditLimitPriceUnit)
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        return mapper.Map<GetCustomerResponse>(item);
    }

    public async Task<Guid> CreateAsync(CustomerRequestDto request, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var item = Customer.Create(request.CustomerType,
            request.FullName,
            request.NationalId,
            request.PhoneNumber,
            request.Address,
            request.CreditLimit,
            request.CreditLimitPriceUnitId.HasValue ? new PriceUnitId(request.CreditLimitPriceUnitId.Value) : null);

        await repository.CreateAsync(item, cancellationToken);

        return item.Id.Value;
    }

    public async Task UpdateAsync(Guid id, CustomerRequestDto request, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var item = await repository.Get(new CustomersByIdSpecification(new CustomerId(id)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        item.SetFullName(request.FullName);
        item.SetNationalId(request.NationalId);
        item.SetPhoneNumber(request.PhoneNumber);
        item.SetAddress(request.Address);
        item.SetCustomerType(request.CustomerType);

        if (request.CreditLimitPriceUnitId.HasValue) 
            item.SetCreditLimit(request.CreditLimit, new PriceUnitId(request.CreditLimitPriceUnitId.Value));

        await repository.UpdateAsync(item, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await repository.Get(new CustomersByIdSpecification(new CustomerId(id)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        await repository.DeleteAsync(item, cancellationToken);
    }
}