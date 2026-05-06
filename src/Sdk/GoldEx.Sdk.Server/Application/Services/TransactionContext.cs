using GoldEx.Sdk.Server.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace GoldEx.Sdk.Server.Application.Services;

public class TransactionContext<T>(T dbContext) : ITransactionContext where T : DbContext
{
    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.Database.BeginTransactionAsync(cancellationToken);
    }
}