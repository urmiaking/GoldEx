using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.BankAccountAggregate;

namespace GoldEx.Server.Infrastructure.Repositories.Abstractions;

public interface IBankAccountRepository : IRepository<BankAccount>,
    ICreateRepository<BankAccount>,
    IUpdateRepository<BankAccount>,
    IDeleteRepository<BankAccount>;