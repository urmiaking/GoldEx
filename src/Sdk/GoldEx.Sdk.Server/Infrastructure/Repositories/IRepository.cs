using System.Data.Common;
using Microsoft.EntityFrameworkCore.Storage;

namespace GoldEx.Sdk.Server.Infrastructure.Repositories;

public interface IRepository
{
    void AsNoTracking();
    void AsTracking();
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    IDbContextTransaction? GetCurrentTransaction();
    Task<int> SaveAsync(CancellationToken cancellationToken = default);
    Task<IDbContextTransaction?> UseTransactionAsync(DbTransaction transaction, CancellationToken cancellationToken = default);
}
