using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Shared.DTOs.Customers;

namespace GoldEx.Server.Infrastructure.Repositories.Abstractions;

public interface ICustomerRepository : IRepository<Customer>,
    ICreateRepository<Customer>,
    IUpdateRepository<Customer>,
    IDeleteRepository<Customer>
{
    Task<PagedList<Customer>> GetListAsync(CustomerFilter customerFilter, RequestFilter filter,
        CancellationToken cancellationToken = default);
}