using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Sdk.Server.Infrastructure.Specifications;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace GoldEx.Sdk.Server.Infrastructure.Repositories;

public interface IUpdateRepository<TEntity> where TEntity : EntityBase
{
    void Update(TEntity entity);
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    void UpdateRange(IEnumerable<TEntity> entities);
    Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    Task<int> ExecuteUpdateAsync(ISpecification<TEntity> specification, 
        Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setPropertyCalls,
        CancellationToken cancellationToken = default);
}