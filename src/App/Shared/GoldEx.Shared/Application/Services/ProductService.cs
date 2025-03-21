using FluentValidation;
using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.Application.Services.Abstractions;
using GoldEx.Shared.Application.Validators.Products;
using GoldEx.Shared.Domain.Aggregates.ProductAggregate;
using GoldEx.Shared.Domain.Aggregates.ProductCategoryAggregate;
using GoldEx.Shared.Infrastructure.Repositories.Abstractions;

namespace GoldEx.Shared.Application.Services;

public class ProductService<TProduct, TCategory, TGemStone>(
    IProductRepository<TProduct, TCategory, TGemStone> repository,
    CreateProductValidator<TProduct, TCategory, TGemStone> createValidator,
    UpdateProductValidator<TProduct, TCategory, TGemStone> updateValidator,
    DeleteProductValidator<TProduct, TCategory, TGemStone> deleteValidator)
    : IProductService<TProduct, TCategory, TGemStone>
    where TProduct : ProductBase<TCategory, TGemStone>
    where TCategory : ProductCategoryBase
    where TGemStone : GemStoneBase
{ 
    public async Task CreateAsync(TProduct product, CancellationToken cancellationToken = default)
    {
        await createValidator.ValidateAndThrowAsync(product, cancellationToken);
        await repository.CreateAsync(product, cancellationToken);
    }

    public async Task UpdateAsync(TProduct product, CancellationToken cancellationToken = default)
    {
        await updateValidator.ValidateAndThrowAsync(product, cancellationToken);
        await repository.UpdateAsync(product, cancellationToken);
    }

    public async Task DeleteAsync(TProduct product, bool deletePermanently = false, CancellationToken cancellationToken = default)
    {
        await deleteValidator.ValidateAndThrowAsync(product, cancellationToken);
        await repository.DeleteAsync(product, deletePermanently, cancellationToken);
    }

    public Task<TProduct?> GetAsync(ProductId id, CancellationToken cancellationToken = default)
        => repository.GetAsync(id, cancellationToken);

    public Task<TProduct?> GetAsync(string barcode, CancellationToken cancellationToken = default)
        => repository.GetAsync(barcode, cancellationToken);

    public Task<PagedList<TProduct>> GetListAsync(RequestFilter filter, CancellationToken cancellationToken = default)
        => repository.GetListAsync(filter, cancellationToken);

    public Task<List<TProduct>> GetPendingItemsAsync(DateTime checkpointDate, CancellationToken cancellationToken = default)
        => repository.GetPendingItemsAsync(checkpointDate, cancellationToken);
}