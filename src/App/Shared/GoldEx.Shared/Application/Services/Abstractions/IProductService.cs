using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.Domain.Aggregates.ProductAggregate;
using GoldEx.Shared.Domain.Aggregates.ProductCategoryAggregate;

namespace GoldEx.Shared.Application.Services.Abstractions;

public interface IProductService<TProduct, TCategory> 
    where TProduct : ProductBase<TCategory>
    where TCategory : ProductCategoryBase
{
    Task CreateAsync(TProduct product, CancellationToken cancellationToken = default);
    Task UpdateAsync(TProduct product, CancellationToken cancellationToken = default);
    Task DeleteAsync(TProduct product, bool deletePermanently = false, CancellationToken cancellationToken = default);
    Task<TProduct?> GetAsync(ProductId id, CancellationToken cancellationToken = default);
    Task<TProduct?> GetAsync(string barcode, CancellationToken cancellationToken = default);
    Task<PagedList<TProduct>> GetListAsync(RequestFilter filter, CancellationToken cancellationToken = default);
    Task<List<TProduct>> GetPendingItemsAsync(DateTime checkpointDate, CancellationToken cancellationToken = default);
}