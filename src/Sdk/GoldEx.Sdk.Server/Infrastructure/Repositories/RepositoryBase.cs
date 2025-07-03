using System.Data;
using System.Data.Common;
using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Sdk.Server.Infrastructure.Extensions;
using GoldEx.Sdk.Server.Infrastructure.Specifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace GoldEx.Sdk.Server.Infrastructure.Repositories;

public abstract class RepositoryBase<TEntity>(DbContext context) :
    IRepository<TEntity>,
    ICreateRepository<TEntity>,
    IUpdateRepository<TEntity>,
    IDeleteRepository<TEntity>
    where TEntity : EntityBase
{
    protected bool Tracking { get; private set; } = true;

    public virtual IQueryable<TEntity> Query => context.Set<TEntity>().SetTracking(Tracking);

    public virtual void AsNoTracking() => Tracking = false;

    public virtual void AsTracking() => Tracking = true;

    public virtual void Create(TEntity entity) => context.Add(entity);

    public virtual void CreateRange(IEnumerable<TEntity> entities) => context.AddRange(entities);

    public virtual void Update(TEntity entity) => context.Update(entity);

    public virtual void UpdateRange(IEnumerable<TEntity> entities) => context.UpdateRange(entities);

    public virtual void Delete(TEntity entity) => context.Remove(entity);

    public virtual void DeleteRange(IEnumerable<TEntity> entities) => context.RemoveRange(entities);

    public virtual Task<int> SaveAsync(CancellationToken cancellationToken = default)
    {
        return context.SaveChangesAsync(cancellationToken);
    }

    public virtual Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel,
      CancellationToken cancellationToken = default)
    {
        return context.Database.BeginTransactionAsync(isolationLevel, cancellationToken);
    }

    public virtual Task<IDbContextTransaction?> UseTransactionAsync(
      DbTransaction transaction,
      CancellationToken cancellationToken = default)
    {
        return context.Database.UseTransactionAsync(transaction, cancellationToken);
    }

    public IQueryable<TEntity> Get(ISpecification<TEntity> specification)
    {
        var query = Query;

        // Apply criteria
        if (specification.Criteria != null)
        {
            query = query.Where(specification.Criteria);
        }

        // Apply includes
        query = specification.Includes
            .Aggregate(query, (current, include) => current.Include(include));

        // Apply ordering
        if (specification.OrderBy != null)
        {
            query = query.OrderBy(specification.OrderBy);
        }
        else if (specification.OrderByDescending != null)
        {
            query = query.OrderByDescending(specification.OrderByDescending);
        }


        // Apply paging
        if (specification.IsPagingEnabled)
        {
            query = query.Skip(specification.Skip ?? 0).Take(specification.Take ?? 100);
        }

        return query;
    }

    public Task<int> CountAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
    {
        var query = Query;

        if (specification.Criteria != null)
        {
            query = query.Where(specification.Criteria);
        }

        return query.CountAsync(cancellationToken);
    }

    public Task<bool> ExistsAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
    {
        var query = Query;

        if (specification.Criteria != null)
        {
            query = query.Where(specification.Criteria);
        }

        return query.AnyAsync(cancellationToken);
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

    public virtual async Task CreateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        CreateRange(entities);
        await SaveAsync(cancellationToken);
    }

    public virtual async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Update(entity);
        await SaveAsync(cancellationToken);
    }

    public virtual async Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        UpdateRange(entities);
        await SaveAsync(cancellationToken);
    }

    public virtual async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Delete(entity);
        await SaveAsync(cancellationToken);
    }

    public virtual async Task DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        DeleteRange(entities);
        await SaveAsync(cancellationToken);
    }
}