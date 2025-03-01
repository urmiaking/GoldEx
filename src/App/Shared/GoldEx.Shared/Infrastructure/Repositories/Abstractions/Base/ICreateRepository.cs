using GoldEx.Shared.Domain.Entities;

namespace GoldEx.Shared.Infrastructure.Repositories.Abstractions.Base;

public interface ICreateRepository<in TEntity> where TEntity : EntityBase
{
    void Create(TEntity entity);
    Task CreateAsync(TEntity entity, CancellationToken cancellationToken = default);
}
