using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.CustomerAggregate;

namespace GoldEx.Server.Infrastructure.Repositories.Abstractions;

public interface ICustomerRepository : IRepository<Customer>,
    ICreateRepository<Customer>,
    IUpdateRepository<Customer>,
    IDeleteRepository<Customer>;