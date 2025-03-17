using GoldEx.Shared.Domain.Aggregates.ProductCategoryAggregate;
using GoldEx.Shared.Infrastructure.Repositories.Abstractions.Base;

namespace GoldEx.Shared.Infrastructure.Repositories.Abstractions;

public interface IProductCategoryRepository<T> : IRepository,
    ICreateRepository<T>,
    IUpdateRepository<T>,
    IDeleteRepository<T> where T : ProductCategoryBase
{
    Task<T?> GetAsync(ProductCategoryId id, CancellationToken cancellationToken = default);
    Task<T?> GetAsync(string title, CancellationToken cancellationToken = default);
    Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<T>> GetPendingItemsAsync(DateTime checkpointDate, CancellationToken cancellationToken = default);
}