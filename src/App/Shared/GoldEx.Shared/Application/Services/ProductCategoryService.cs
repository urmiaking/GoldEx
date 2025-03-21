using GoldEx.Shared.Application.Services.Abstractions;
using GoldEx.Shared.Application.Validators.Categories;
using GoldEx.Shared.Domain.Aggregates.ProductAggregate;
using GoldEx.Shared.Domain.Aggregates.ProductCategoryAggregate;
using GoldEx.Shared.Infrastructure.Repositories.Abstractions;

namespace GoldEx.Shared.Application.Services;

public class ProductCategoryService<TCategory, TProduct, TGemStone>(
    IProductCategoryRepository<TCategory> repository,
    CreateProductCategoryValidator<TCategory> createValidator,
    UpdateProductCategoryValidator<TCategory> updateValidator,
    DeleteProductCategoryValidator<TCategory, TProduct, TGemStone> deleteValidator) : IProductCategoryService<TCategory>
    where TCategory : ProductCategoryBase
    where TProduct : ProductBase<TCategory, TGemStone>
    where TGemStone : GemStoneBase
{
    public async Task CreateAsync(TCategory category, CancellationToken cancellationToken = default)
    {
        await createValidator.ValidateAsync(category, cancellationToken);
        await repository.CreateAsync(category, cancellationToken);
    }

    public async Task UpdateAsync(TCategory category, CancellationToken cancellationToken = default)
    {
        await updateValidator.ValidateAsync(category, cancellationToken);
        await repository.UpdateAsync(category, cancellationToken);
    }

    public async Task DeleteAsync(TCategory category, bool deletePermanently = false, CancellationToken cancellationToken = default)
    {
        await deleteValidator.ValidateAsync(category, cancellationToken);
        await repository.DeleteAsync(category, deletePermanently, cancellationToken);
    }

    public Task<TCategory?> GetAsync(ProductCategoryId id, CancellationToken cancellationToken = default)
        => repository.GetAsync(id, cancellationToken);

    public Task<TCategory?> GetAsync(string title, CancellationToken cancellationToken = default)
        => repository.GetAsync(title, cancellationToken);

    public Task<List<TCategory>> GetAllAsync(CancellationToken cancellationToken = default)
        => repository.GetAllAsync(cancellationToken);

    public Task<List<TCategory>> GetPendingItemsAsync(DateTime checkpointDate, CancellationToken cancellationToken = default)
        => repository.GetPendingItemsAsync(checkpointDate, cancellationToken);
}