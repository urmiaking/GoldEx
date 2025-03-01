using System.Data.Common;
using Microsoft.EntityFrameworkCore.Storage;

namespace GoldEx.Shared.Infrastructure.Repositories.Abstractions.Base;

public interface IRepository
{
    void AsNoTracking();
    void AsTracking();
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    IDbContextTransaction? GetCurrentTransaction();
    Task<int> SaveAsync(CancellationToken cancellationToken = default);
    Task<IDbContextTransaction?> UseTransactionAsync(DbTransaction transaction, CancellationToken cancellationToken = default);
}
