using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Server.Application.Validators.ProductAttributes;
using GoldEx.Server.Application.Validators.ProductCategories;
using GoldEx.Server.Domain.ProductAttributeAggregate;
using GoldEx.Server.Domain.ProductCategoryAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.ProductAttributes;
using GoldEx.Server.Infrastructure.Specifications.ProductCategories;
using GoldEx.Shared.DTOs.ProductAttributes;
using GoldEx.Shared.DTOs.ProductCategories;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class ProductAttributeService(
    IProductAttributeRepository attributeRepository,
    IProductCategoryRepository categoryRepository,
    CreateProductAttributeRequestValidator createValidator,
    UpdateProductAttributeRequestValidator updateValidator,
    SetCategoryAttributesRequestValidator setCategoryValidator) : IProductAttributeService
{
    public async Task<List<ProductAttributeDto>> GetListAsync(CancellationToken cancellationToken = default)
    {
        var items = await attributeRepository
            .Get(new ProductAttributesDefaultSpecification())
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return items.Select(x => new ProductAttributeDto(
            x.Id.Value,
            x.Title,
            x.Unit,
            x.DataType,
            x.Options,
            x.Description
        )).ToList();
    }

    public async Task<ProductAttributeDto> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await attributeRepository
            .Get(new ProductAttributesByIdSpecification(new ProductAttributeId(id)))
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException("ویژگی مورد نظر یافت نشد.");

        return new ProductAttributeDto(
            item.Id.Value,
            item.Title,
            item.Unit,
            item.DataType,
            item.Options,
            item.Description);
    }

    public async Task CreateAsync(CreateProductAttributeRequest request, CancellationToken cancellationToken = default)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);

        var attribute = ProductAttribute.Create(
            request.Title,
            request.Unit,
            request.DataType,
            request.Options,
            request.Description);

        await attributeRepository.CreateAsync(attribute, cancellationToken);
    }

    public async Task UpdateAsync(Guid id, UpdateProductAttributeRequest request, CancellationToken cancellationToken = default)
    {
        await updateValidator.ValidateAndThrowAsync(request, cancellationToken);

        var item = await attributeRepository
            .Get(new ProductAttributesByIdSpecification(new ProductAttributeId(id)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException("ویژگی مورد نظر یافت نشد.");

        item.Update(
            request.Title,
            request.Unit,
            request.DataType,
            request.Options,
            request.Description);

        await attributeRepository.UpdateAsync(item, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await attributeRepository
            .Get(new ProductAttributesByIdSpecification(new ProductAttributeId(id)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException("ویژگی مورد نظر یافت نشد.");

        await attributeRepository.DeleteAsync(item, cancellationToken);
    }

    public async Task<List<CategoryAttributeDto>> GetCategoryAttributesAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        var category = await categoryRepository
            .Get(new ProductCategoryWithAttributesByIdSpecification(new ProductCategoryId(categoryId)))
            .Include(x => x.Attributes)
                .ThenInclude(x => x.ProductAttribute)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException("دسته‌بندی مورد نظر یافت نشد.");

        return category.Attributes
            .OrderBy(x => x.DisplayOrder)
            .Where(x => x.ProductAttribute != null)
            .Select(x => new CategoryAttributeDto(
                x.ProductAttributeId.Value,
                x.ProductAttribute!.Title,
                x.ProductAttribute.Unit,
                x.ProductAttribute.DataType,
                x.ProductAttribute.Options,
                x.IsRequired,
                x.DisplayOrder,
                x.ShowInFilter))
            .ToList();
    }

    public async Task SetCategoryAttributesAsync(SetCategoryAttributesRequest request, CancellationToken cancellationToken = default)
    {
        await setCategoryValidator.ValidateAndThrowAsync(request, cancellationToken);

        var category = await categoryRepository
            .Get(new ProductCategoryWithAttributesByIdSpecification(new ProductCategoryId(request.CategoryId)))
            .Include(x => x.Attributes)
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException("دسته‌بندی مورد نظر یافت نشد.");

        var desired = request.Attributes.Select((item, index) =>
            (new ProductAttributeId(item.AttributeId),
             item.IsRequired,
             item.DisplayOrder == 0 ? index + 1 : item.DisplayOrder,
             item.ShowInFilter));

        category.UpdateAttributes(desired, category.StoreId);

        await categoryRepository.UpdateAsync(category, cancellationToken);
    }
}
