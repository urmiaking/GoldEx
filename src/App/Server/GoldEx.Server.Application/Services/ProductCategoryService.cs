using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Server.Application.Validators.ProductCategories;
using GoldEx.Server.Domain.ProductCategoryAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.ProductCategories;
using GoldEx.Shared.DTOs.ProductCategories;
using GoldEx.Shared.Services;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class ProductCategoryService(
    IProductCategoryRepository repository,
    IMapper mapper,
    CreateProductCategoryRequestValidator createValidator,
    UpdateProductCategoryRequestValidator updateValidator,
    DeleteProductCategoryValidator deleteValidator) : IProductCategoryService
{
    public async Task<List<GetProductCategoryResponse>> GetListAsync(CancellationToken cancellationToken = default)
    {
        var items = await repository.Get(new ProductCategoriesDefaultSpecification())
            .ToListAsync(cancellationToken) ?? throw new NotFoundException();

        return mapper.Map<List<GetProductCategoryResponse>>(items);
    }

    public async Task<GetProductCategoryResponse> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await repository.Get(new ProductCategoriesByIdSpecification(new ProductCategoryId(id)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        return mapper.Map<GetProductCategoryResponse>(item);
    }

    public async Task CreateAsync(CreateProductCategoryRequest request, CancellationToken cancellationToken = default)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);

        var category = ProductCategory.Create(request.Title);

        await repository.CreateAsync(category, cancellationToken);
    }

    public async Task UpdateAsync(Guid id, UpdateProductCategoryRequest request, CancellationToken cancellationToken = default)
    {
        await updateValidator.ValidateAndThrowAsync((id, request), cancellationToken);

        var item = await repository.Get(new ProductCategoriesByIdSpecification(new ProductCategoryId(id)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        item.SetTitle(request.Title);

        await repository.UpdateAsync(item, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await repository.Get(new ProductCategoriesByIdSpecification(new ProductCategoryId(id)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        await deleteValidator.ValidateAndThrowAsync(item, cancellationToken);

        await repository.DeleteAsync(item, cancellationToken);
    }
}