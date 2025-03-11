using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.Domain.Aggregates.ProductAggregate;

namespace GoldEx.Shared.Application.Services.Abstractions;

public interface IProductService<T> where T : ProductBase
{
    Task CreateAsync(T product, CancellationToken cancellationToken = default);
    Task UpdateAsync(T product, CancellationToken cancellationToken = default);
    Task DeleteAsync(T product, bool deletePermanently = false, CancellationToken cancellationToken = default);
    Task<T?> GetAsync(ProductId id, CancellationToken cancellationToken = default);
    Task<T?> GetAsync(string barcode, CancellationToken cancellationToken = default);
    Task<PagedList<T>> GetListAsync(RequestFilter filter, CancellationToken cancellationToken = default);
    Task<List<T>> GetPendingItemsAsync(DateTime checkpointDate, CancellationToken cancellationToken = default);
}