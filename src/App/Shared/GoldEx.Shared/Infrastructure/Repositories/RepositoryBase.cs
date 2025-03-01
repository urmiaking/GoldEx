using System.Data.Common;
using GoldEx.Shared.Domain.Entities;
using GoldEx.Shared.Infrastructure.Extensions;
using GoldEx.Shared.Infrastructure.Repositories.Abstractions.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace GoldEx.Shared.Infrastructure.Repositories;

public abstract class RepositoryBase<TEntity>(IGoldExDbContextFactory contextFactory) :
    IRepository, ICreateRepository<TEntity>, IUpdateRepository<TEntity>, IDeleteRepository<TEntity>, IDisposable
    where TEntity : EntityBase
{
    protected bool Tracking { get; private set; } = true;

    private DbContext? _context;

    protected DbContext Context => _context ?? throw new InvalidOperationException("DbContext is not initialized.");

    public virtual IQueryable<TEntity> Query => Context.Set<TEntity>().SetTracking(Tracking);

    public virtual void AsNoTracking() => Tracking = false;
    public virtual void AsTracking() => Tracking = true;

    public virtual void Create(TEntity entity) => Context.Add(entity);
    public virtual void Update(TEntity entity) => Context.Update(entity);
    public virtual void Delete(TEntity entity) => Context.Remove(entity);

    public virtual Task<int> SaveAsync(CancellationToken cancellationToken = default) => Context.SaveChangesAsync(cancellationToken);

    public virtual Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return Context.Database.BeginTransactionAsync(cancellationToken);
    }

    public virtual Task<IDbContextTransaction?> UseTransactionAsync(DbTransaction transaction, CancellationToken cancellationToken = default)
    {
        return Context.Database.UseTransactionAsync(transaction, cancellationToken);
    }

    public virtual IDbContextTransaction? GetCurrentTransaction()
    {
        return Context.Database.CurrentTransaction;
    }

    public virtual async Task CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await InitializeDbContextAsync();

        Context.Set<TEntity>().Add(entity);
        await Context.SaveChangesAsync(cancellationToken);

        //Create(entity);
        //await SaveAsync(cancellationToken);
    }

    public virtual async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await InitializeDbContextAsync();

        Update(entity);
        await SaveAsync(cancellationToken);
    }

    public virtual async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await InitializeDbContextAsync();
       
        Delete(entity);
        await SaveAsync(cancellationToken);
    }

    protected async Task InitializeDbContextAsync()
    {
        _context ??= await contextFactory.CreateDbContextAsync();
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
