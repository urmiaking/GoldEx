using Microsoft.EntityFrameworkCore.Storage;

namespace GoldEx.Sdk.Server.Application.Abstractions;

public interface ITransactionContext
{
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}