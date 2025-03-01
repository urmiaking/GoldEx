using GoldEx.Shared.Domain.Entities;

namespace GoldEx.Shared.Infrastructure.Repositories.Abstractions.Base;

public interface IUpdateRepository<in TEntity> where TEntity : EntityBase
{
    void Update(TEntity entity);
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
}