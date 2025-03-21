using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.Domain.Aggregates.ProductAggregate;
using GoldEx.Shared.Domain.Aggregates.ProductCategoryAggregate;
using GoldEx.Shared.Infrastructure.Repositories.Abstractions.Base;

namespace GoldEx.Shared.Infrastructure.Repositories.Abstractions;

public interface IProductRepository<TProduct, TCategory, TGemStone> : IRepository,
    ICreateRepository<TProduct>,
    IUpdateRepository<TProduct>,
    IDeleteRepository<TProduct> 
    where TProduct : ProductBase<TCategory, TGemStone>
    where TCategory : ProductCategoryBase
    where TGemStone : GemStoneBase
{
    Task<TProduct?> GetAsync(ProductId id, CancellationToken cancellationToken = default);
    Task<TProduct?> GetAsync(string barcode, CancellationToken cancellationToken = default);
    Task<PagedList<TProduct>> GetListAsync(RequestFilter filter, CancellationToken cancellationToken = default);
    Task<List<TProduct>> GetPendingItemsAsync(DateTime checkpointDate, CancellationToken cancellationToken = default);
    Task<bool> CheckCategoryUsedAsync(ProductCategoryId categoryId, CancellationToken cancellationToken = default);
}