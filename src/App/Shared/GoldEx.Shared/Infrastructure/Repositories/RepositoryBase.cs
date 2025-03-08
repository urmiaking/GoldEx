using System.Data.Common;
using GoldEx.Shared.Domain.Entities;
using GoldEx.Shared.Infrastructure.Extensions;
using GoldEx.Shared.Infrastructure.Repositories.Abstractions.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Dynamic.Core;
using GoldEx.Sdk.Common.Definitions;
using System.Reflection;

namespace GoldEx.Shared.Infrastructure.Repositories;

public abstract class RepositoryBase<TEntity>(IGoldExDbContextFactory contextFactory) :
    IRepository, ICreateRepository<TEntity>, IUpdateRepository<TEntity>, IDeleteRepository<TEntity>, IDisposable
    where TEntity : EntityBase
{
    protected bool Tracking { get; private set; } = true;

    private DbContext? _context;

    protected DbContext Context => _context ?? throw new InvalidOperationException("DbContext is not initialized.");

    public virtual IQueryable<TEntity> NonDeletedQuery
    {
        get
        {
            var query = Context.Set<TEntity>().SetTracking(Tracking);

            if (typeof(TEntity).IsAssignableTo(typeof(ISoftDeleteEntity)))
            {
                return query.Where($"{nameof(ISoftDeleteEntity.IsDeleted)}={false}");
            }

            if (typeof(TEntity).IsAssignableTo(typeof(ITrackableEntity)))
            {
                return query.Where($"{nameof(ITrackableEntity)}<>{ModifyStatus.Deleted}");
            }

            return query;
        }
    }

    public virtual IQueryable<TEntity> AllQuery => Context.Set<TEntity>().SetTracking(Tracking);

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

        if (entity is ISyncableEntity syncableEntity)
            syncableEntity.SetLastModifiedDate();

        var properties = typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var property in properties)
        {
            if (typeof(ISyncableEntity).IsAssignableFrom(property.PropertyType))
            {
                if (property.GetValue(entity) is ISyncableEntity navigationProperty)
                {
                    navigationProperty.SetLastModifiedDate();
                }
            }
        }

        Create(entity);
        await SaveAsync(cancellationToken);
    }

    public virtual async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await InitializeDbContextAsync();

        if (entity is ISyncableEntity syncableEntity)
            syncableEntity.SetLastModifiedDate();

        var properties = typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var property in properties)
        {
            if (typeof(ISyncableEntity).IsAssignableFrom(property.PropertyType))
            {
                if (property.GetValue(entity) is ISyncableEntity navigationProperty)
                {
                    navigationProperty.SetLastModifiedDate();
                }
            }
        }

        Update(entity);
        await SaveAsync(cancellationToken);
    }

    public virtual async Task DeleteAsync(TEntity entity, bool deletePermanently = false,
        CancellationToken cancellationToken = default)
    {
        await InitializeDbContextAsync();

        if (!deletePermanently && entity is ISoftDeleteEntity softDeleteEntity)
        {
            softDeleteEntity.SetDeleted();

            if (entity is ISyncableEntity syncableEntity)
                syncableEntity.SetLastModifiedDate();

            var properties = typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                if (typeof(ISyncableEntity).IsAssignableFrom(property.PropertyType))
                {
                    if (property.GetValue(entity) is ISyncableEntity navigationProperty)
                    {
                        navigationProperty.SetLastModifiedDate();
                    }
                }
            }

            await UpdateAsync(entity, cancellationToken);
        }
        else
        {
            Delete(entity);
            await SaveAsync(cancellationToken);
        }
    }

    protected async Task InitializeDbContextAsync()
    {
        _context ??= await contextFactory.CreateDbContextAsync();

        Context.Set<TEntity>();
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
