using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.FinancialAccountAggregate;

namespace GoldEx.Server.Infrastructure.Repositories.Abstractions;

public interface IFinancialAccountRepository : IRepository<FinancialAccount>,
    ICreateRepository<FinancialAccount>,
    IUpdateRepository<FinancialAccount>,
    IDeleteRepository<FinancialAccount>;