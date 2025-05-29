using GoldEx.Sdk.Server.Domain.Entities;

namespace GoldEx.Sdk.Server.Infrastructure.Repositories;

public interface IUpdateRepository<in TEntity> where TEntity : EntityBase
{
    void Update(TEntity entity);
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
}