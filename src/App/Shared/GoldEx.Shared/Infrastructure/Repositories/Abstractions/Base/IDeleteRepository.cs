using GoldEx.Shared.Domain.Entities;

namespace GoldEx.Shared.Infrastructure.Repositories.Abstractions.Base;

public interface IDeleteRepository<in TEntity> where TEntity : EntityBase
{
    void Delete(TEntity entity);
    Task DeleteAsync(TEntity entity, bool deletePermanently = false, CancellationToken cancellationToken = default);
}
