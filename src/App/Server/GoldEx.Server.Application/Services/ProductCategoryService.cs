using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Application.Validators.ProductCategories;
using GoldEx.Server.Domain.ProductCategoryAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.ProductCategories;
using GoldEx.Shared.DTOs.ProductCategories;
using GoldEx.Shared.Services.Abstractions;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class ProductCategoryService(
    IProductCategoryRepository repository,
    IMapper mapper,
    CreateProductCategoryRequestValidator createValidator,
    UpdateProductCategoryRequestValidator updateValidator,
    DeleteProductCategoryValidator deleteValidator) : IProductCategoryService, IServerProductCategoryService
{
    public async Task<List<GetProductCategoryResponse>> GetListAsync(CancellationToken cancellationToken = default)
    {
        var items = await repository
            .Get(new ProductCategoriesDefaultSpecification())
            .AsNoTracking()
            .ToListAsync(cancellationToken) ?? throw new NotFoundException();

        return mapper.Map<List<GetProductCategoryResponse>>(items);
    }

    public async Task<GetProductCategoryResponse> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await repository
            .Get(new ProductCategoriesByIdSpecification(new ProductCategoryId(id)))
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        return mapper.Map<GetProductCategoryResponse>(item);
    }

    public async Task CreateAsync(CreateProductCategoryRequest request, CancellationToken cancellationToken = default)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);

        var category = ProductCategory.Create(request.Title, request.PrefixCode);

        await repository.CreateAsync(category, cancellationToken);
    }

    public async Task UpdateAsync(Guid id, UpdateProductCategoryRequest request, CancellationToken cancellationToken = default)
    {
        await updateValidator.ValidateAndThrowAsync((id, request), cancellationToken);

        var item = await repository
            .Get(new ProductCategoriesByIdSpecification(new ProductCategoryId(id)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        item.SetTitle(request.Title);
        item.SetPrefixCode(request.PrefixCode);

        await repository.UpdateAsync(item, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await repository.Get(new ProductCategoriesByIdSpecification(new ProductCategoryId(id)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        await deleteValidator.ValidateAndThrowAsync(item, cancellationToken);

        await repository.DeleteAsync(item, cancellationToken);
    }

    public async Task<GetProductCategoryNumberResponse> GetLastCodeAsync(CancellationToken cancellationToken = default)
    {
        var lastCode = await repository.GetLastPrefixCodeAsync(cancellationToken);
        // if the last code is int e.g. 01, convert to int and add 1, otherwise if contains letter for example EH
        // add a letter to last letter and return EI, and if the last letter is Z then increment the first letter and set the last letter to A
        if (int.TryParse(lastCode, out var number))
        {
            number++;
            lastCode = number.ToString("D2");
        }
        else if (lastCode.Length == 2)
        {
            var firstChar = lastCode[0];
            var secondChar = lastCode[1];
            if (secondChar == 'Z')
            {
                if (firstChar == 'Z')
                    throw new InvalidOperationException("Cannot generate new prefix code, maximum limit reached.");

                firstChar++;
                secondChar = 'A';
            }
            else
            {
                secondChar++;
            }
            lastCode = $"{firstChar}{secondChar}";
        }
        else
            throw new InvalidOperationException("Invalid prefix code format.");

        return new GetProductCategoryNumberResponse(lastCode);
    }

    public async Task<ProductCategory> GetOrCreateAsync(
        string? categoryTitle,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(categoryTitle))
            categoryTitle = "سایر";

        var normalized = categoryTitle.NormalizeText();

        var spec = new ProductCategoriesByLooseMatchSpecification(normalized);

        var candidates = await repository
            .Get(spec)
            .ToListAsync(cancellationToken);

        // In-memory strict comparison (only on small candidate list)
        var exact = candidates.FirstOrDefault(c => c.Title.Normalize() == normalized);

        if (exact is not null)
            return exact;

        // Create new category
        var categoryPrefix = await GetLastCodeAsync(cancellationToken);

        var category = ProductCategory.Create(categoryTitle, prefixCode: categoryPrefix.Number);

        await repository.CreateAsync(category, cancellationToken);

        return category;
    }
}