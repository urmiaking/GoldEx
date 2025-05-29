using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.TransactionAggregate;

namespace GoldEx.Server.Infrastructure.Repositories.Abstractions;

public interface ITransactionRepository : IRepository<Transaction>,
    ICreateRepository<Transaction>,
    IUpdateRepository<Transaction>,
    IDeleteRepository<Transaction>
{
    Task<long> GetLastTransactionNumberAsync(CancellationToken cancellationToken = default);
}