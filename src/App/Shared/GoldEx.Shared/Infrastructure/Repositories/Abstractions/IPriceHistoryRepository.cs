using GoldEx.Shared.Domain.Entities;
using GoldEx.Shared.Infrastructure.Repositories.Abstractions.Base;

namespace GoldEx.Shared.Infrastructure.Repositories.Abstractions;

public interface IPriceHistoryRepository<in T> : IRepository,
    ICreateRepository<T>,
    IDeleteRepository<T> where T : EntityBase
{
    Task<int> CleanupAsync(CancellationToken cancellationToken = default);
}