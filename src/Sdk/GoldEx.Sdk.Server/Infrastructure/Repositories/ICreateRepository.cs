using GoldEx.Sdk.Server.Domain.Entities;

namespace GoldEx.Sdk.Server.Infrastructure.Repositories;

public interface ICreateRepository<in TEntity> where TEntity : EntityBase
{
    void Create(TEntity entity);
    Task CreateAsync(TEntity entity, CancellationToken cancellationToken = default);
}
