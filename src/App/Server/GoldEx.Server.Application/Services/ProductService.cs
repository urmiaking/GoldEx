using FluentValidation;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Server.Application.Validators.Products;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Domain.ProductCategoryAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Products;
using GoldEx.Shared.DTOs.Products;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class ProductService(
    IProductRepository repository,
    IMapper mapper,
    CreateProductRequestValidator createValidator,
    UpdateProductRequestValidator updateValidator,
    DeleteProductValidator deleteValidator) : IProductService
{
    public async Task<PagedList<GetProductResponse>> GetListAsync(RequestFilter filter, CancellationToken cancellationToken = default)
    {
        var skip = filter.Skip ?? 0;
        var take = filter.Take ?? 100;

        var data = await repository.Get(new ProductsByFilterSpecification(filter)).ToListAsync(cancellationToken);
        var totalCount = await repository.CountAsync(new ProductsByFilterSpecification(filter), cancellationToken);

        return new PagedList<GetProductResponse>
        {
            Data = mapper.Map<List<GetProductResponse>>(data),
            Skip = skip,
            Take = take,
            Total = totalCount
        };
    }

    public async Task<GetProductResponse> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await repository.Get(new ProductsByIdSpecification(new ProductId(id)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        return mapper.Map<GetProductResponse>(item);
    }

    public async Task<GetProductResponse> GetAsync(string barcode, CancellationToken cancellationToken = default)
    {
        var item = await repository.Get(new ProductsByBarcodeSpecification(barcode))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        return mapper.Map<GetProductResponse>(item);
    }

    public async Task CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);

        var item = Product.Create(
            request.Name,
            request.Barcode,
            request.Weight,
            request.Wage,
            request.ProductType,
            request.CaratType,
            request.WageType,
            new ProductCategoryId(request.ProductCategoryId));

        if (request.ProductType == ProductType.Jewelry)
        {
            item.SetGemStones(request.GemStones?.Select(s => GemStone.Create(s.Code,
                s.Type,
                s.Color,
                s.Cut,
                s.Carat,
                s.Purity,
                item.Id)));
        }

        await repository.CreateAsync(item, cancellationToken);
    }

    public async Task UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        await updateValidator.ValidateAndThrowAsync((id, request), cancellationToken);

        var item = await repository.Get(new ProductsByIdSpecification(new ProductId(id)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        item.SetName(request.Name);
        item.SetBarcode(request.Barcode);
        item.SetWeight(request.Weight);
        item.SetWage(request.Wage);
        item.SetWageType(request.WageType);
        item.SetProductType(request.ProductType);
        item.SetCaratType(request.CaratType);

        if (request.ProductCategoryId.HasValue)
            item.SetProductCategory(new ProductCategoryId(request.ProductCategoryId.Value));
        else
            item.SetProductCategory(null);

        if (request.ProductType == ProductType.Jewelry)
        {
            item.SetGemStones(request.GemStones?.Select(s => GemStone.Create(s.Code,
                s.Type,
                s.Color,
                s.Cut,
                s.Carat,
                s.Purity,
                item.Id)));
        }
        else
        {
            item.ClearGemStones();
        }
        await repository.UpdateAsync(item, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await repository.Get(new ProductsByIdSpecification(new ProductId(id)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        await deleteValidator.ValidateAndThrowAsync(item, cancellationToken);
        await repository.DeleteAsync(item, cancellationToken);
    }
}