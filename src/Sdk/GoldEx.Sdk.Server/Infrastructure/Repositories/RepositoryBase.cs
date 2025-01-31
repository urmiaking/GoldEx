using System.Data.Common;
using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Sdk.Server.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace GoldEx.Sdk.Server.Infrastructure.Repositories;

public abstract class RepositoryBase<TEntity>(DbContext context) :
    IRepository, ICreateRepository<TEntity>, IUpdateRepository<TEntity>, IDeleteRepository<TEntity>
    where TEntity : EntityBase
{
    protected bool Tracking { get; private set; } = true;

    public virtual IQueryable<TEntity> Query => context.Set<TEntity>().SetTracking(Tracking);

    public virtual void AsNoTracking() => Tracking = false;
    public virtual void AsTracking() => Tracking = true;

    public virtual void Create(TEntity entity) => context.Add(entity);
    public virtual void Update(TEntity entity) => context.Update(entity);
    public virtual void Delete(TEntity entity) => context.Remove(entity);

    public virtual Task<int> SaveAsync(CancellationToken cancellationToken = default) => context.SaveChangesAsync(cancellationToken);

    public virtual Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return context.Database.BeginTransactionAsync(cancellationToken);
    }

    public virtual Task<IDbContextTransaction?> UseTransactionAsync(DbTransaction transaction, CancellationToken cancellationToken = default)
    {
        return context.Database.UseTransactionAsync(transaction, cancellationToken);
    }

    public virtual IDbContextTransaction? GetCurrentTransaction()
    {
        return context.Database.CurrentTransaction;
    }

    public virtual async Task CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Create(entity);
        await SaveAsync(cancellationToken);
    }

    public virtual async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Update(entity);
        await SaveAsync(cancellationToken);
    }

    public virtual async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Delete(entity);
        await SaveAsync(cancellationToken);
    }
}
