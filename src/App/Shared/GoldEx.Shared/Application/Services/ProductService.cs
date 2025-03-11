using FluentValidation;
using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.Application.Services.Abstractions;
using GoldEx.Shared.Application.Validators.Products;
using GoldEx.Shared.Domain.Aggregates.ProductAggregate;
using GoldEx.Shared.Infrastructure.Repositories.Abstractions;

namespace GoldEx.Shared.Application.Services;

public class ProductService<T>(
    IProductRepository<T> repository,
    CreateProductValidator<T> createValidator,
    UpdateProductValidator<T> updateValidator,
    DeleteProductValidator<T> deleteValidator)
    : IProductService<T> where T : ProductBase
{ 
    public async Task CreateAsync(T product, CancellationToken cancellationToken = default)
    {
        await createValidator.ValidateAndThrowAsync(product, cancellationToken);
        await repository.CreateAsync(product, cancellationToken);
    }

    public async Task UpdateAsync(T product, CancellationToken cancellationToken = default)
    {
        await updateValidator.ValidateAndThrowAsync(product, cancellationToken);
        await repository.UpdateAsync(product, cancellationToken);
    }

    public async Task DeleteAsync(T product, bool deletePermanently = false, CancellationToken cancellationToken = default)
    {
        await deleteValidator.ValidateAndThrowAsync(product, cancellationToken);
        await repository.DeleteAsync(product, deletePermanently, cancellationToken);
    }

    public Task<T?> GetAsync(ProductId id, CancellationToken cancellationToken = default)
        => repository.GetAsync(id, cancellationToken);

    public Task<T?> GetAsync(string barcode, CancellationToken cancellationToken = default)
        => repository.GetAsync(barcode, cancellationToken);

    public Task<PagedList<T>> GetListAsync(RequestFilter filter, CancellationToken cancellationToken = default)
        => repository.GetListAsync(filter, cancellationToken);

    public Task<List<T>> GetPendingItemsAsync(DateTime checkpointDate, CancellationToken cancellationToken = default)
        => repository.GetPendingItemsAsync(checkpointDate, cancellationToken);
}