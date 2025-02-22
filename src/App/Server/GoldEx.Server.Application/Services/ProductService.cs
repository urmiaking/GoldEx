using FluentValidation;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Application.Validators;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;

namespace GoldEx.Server.Application.Services;

[ScopedService]
public class ProductService(
    IProductRepository repository,
    CreateProductValidator createValidator,
    UpdateProductValidator updateValidator,
    DeleteProductValidator deleteValidator)
    : IProductService
{
    public async Task CreateAsync(Product product, CancellationToken cancellationToken = default)
    {
        await createValidator.ValidateAndThrowAsync(product, cancellationToken);
        await repository.CreateAsync(product, cancellationToken);
    }

    public async Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        await updateValidator.ValidateAndThrowAsync(product, cancellationToken);
        await repository.UpdateAsync(product, cancellationToken);
    }

    public async Task DeleteAsync(Product product, CancellationToken cancellationToken = default)
    {
        await deleteValidator.ValidateAndThrowAsync(product, cancellationToken);
        await repository.DeleteAsync(product, cancellationToken);
    }

    public Task<Product?> GetAsync(ProductId id, CancellationToken cancellationToken = default)
        => repository.GetAsync(id, cancellationToken);

    public Task<Product?> GetAsync(string barcode, CancellationToken cancellationToken = default)
        => repository.GetAsync(barcode, cancellationToken);

    public Task<PagedList<Product>> GetListAsync(RequestFilter filter, CancellationToken cancellationToken = default)
        => repository.GetListAsync(filter, cancellationToken);
}