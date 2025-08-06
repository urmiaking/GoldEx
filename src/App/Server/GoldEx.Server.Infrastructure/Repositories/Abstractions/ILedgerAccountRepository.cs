using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.LedgerAccountAggregate;

namespace GoldEx.Server.Infrastructure.Repositories.Abstractions;

public interface ILedgerAccountRepository : IRepository<LedgerAccount>,
    ICreateRepository<LedgerAccount>,
    IUpdateRepository<LedgerAccount>,
    IDeleteRepository<LedgerAccount>;