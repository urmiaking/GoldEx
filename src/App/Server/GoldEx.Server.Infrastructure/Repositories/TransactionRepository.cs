using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.TransactionAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Infrastructure.Repositories;

[ScopedService]
internal class TransactionRepository(GoldExDbContext dbContext)
    : RepositoryBase<Transaction>(dbContext), ITransactionRepository
{
    public Task<long> GetLastTransactionNumberAsync(CancellationToken cancellationToken = default) =>
        Query.OrderByDescending(t => t.Number)
             .Select(t => t.Number)
             .FirstOrDefaultAsync(cancellationToken);
}