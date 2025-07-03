using GoldEx.Sdk.Server.Domain.Entities;

namespace GoldEx.Sdk.Server.Infrastructure.Repositories;

public interface IDeleteRepository<in TEntity> where TEntity : EntityBase
{
    void Delete(TEntity entity);
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);
    void DeleteRange(IEnumerable<TEntity> entities);
    Task DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
}
