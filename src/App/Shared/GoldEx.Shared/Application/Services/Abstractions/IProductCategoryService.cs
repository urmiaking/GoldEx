using GoldEx.Shared.Domain.Aggregates.ProductCategoryAggregate;

namespace GoldEx.Shared.Application.Services.Abstractions;

public interface IProductCategoryService<TCategory>
    where TCategory : ProductCategoryBase
{
    Task CreateAsync(TCategory category, CancellationToken cancellationToken = default);
    Task UpdateAsync(TCategory category, CancellationToken cancellationToken = default);
    Task DeleteAsync(TCategory category, bool deletePermanently = false, CancellationToken cancellationToken = default);
    Task<TCategory?> GetAsync(ProductCategoryId id, CancellationToken cancellationToken = default);
    Task<TCategory?> GetAsync(string title, CancellationToken cancellationToken = default);
    Task<List<TCategory>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<TCategory>> GetPendingItemsAsync(DateTime checkpointDate, CancellationToken cancellationToken = default);
}