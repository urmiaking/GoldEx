using GoldEx.Shared.Domain.Entities;

namespace GoldEx.Shared.Application.Services.Abstractions;

public interface IPriceHistoryService<in T> where T : EntityBase
{
    Task CreateAsync(T priceHistory, CancellationToken cancellationToken = default);
    Task<int> CleanupAsync(CancellationToken cancellationToken = default);
}