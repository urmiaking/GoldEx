using FluentValidation;
using FluentValidation.Results;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Server.Application.Validators.Products;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Domain.ProductCategoryAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Products;
using GoldEx.Shared.DTOs.Products;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class ProductService(
    IProductRepository repository,
    IMapper mapper,
    ProductRequestDtoValidator validator,
    DeleteProductValidator deleteValidator) : IProductService
{
    public async Task<PagedList<GetProductResponse>> GetListAsync(RequestFilter filter, ProductFilter productFilter,
        CancellationToken cancellationToken = default)
    {
        var skip = filter.Skip ?? 0;
        var take = filter.Take ?? 100;

        var spec = new ProductsByFilterSpecification(filter, productFilter);

        var data = await repository
            .Get(spec)
            .ToListAsync(cancellationToken);

        var totalCount = await repository.CountAsync(spec, cancellationToken);

        return new PagedList<GetProductResponse>
        {
            Data = mapper.Map<List<GetProductResponse>>(data),
            Skip = skip,
            Take = take,
            Total = totalCount
        };
    }

    public async Task<List<GetProductResponse>> GetListAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(name))
            return [];

        var products = await repository
            .Get(new ProductsByNameSpecification(name))
            .Include(x => x.ProductCategory)
            .Include(x => x.WagePriceUnit)
            .GroupBy(x => x.Name)
            .Select(x => x.First())
            .ToListAsync(cancellationToken);

        return mapper.Map<List<GetProductResponse>>(products);
    }

    public async Task<GetProductResponse> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await repository.Get(new ProductsByIdSpecification(new ProductId(id)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        return mapper.Map<GetProductResponse>(item);
    }

    public async Task<GetProductResponse?> GetAsync(string barcode, bool? forCalculation = true, CancellationToken cancellationToken = default)
    {
        var item = await repository
            .Get(new ProductsByBarcodeSpecification(barcode))
            .Include(x => x.ProductCategory)
            .FirstOrDefaultAsync(cancellationToken);

        if (item is not null && forCalculation is false && item.ProductStatus is ProductStatus.Sold)
            throw new ValidationException(new List<ValidationFailure>
            {
                new(nameof(barcode), "این جنس قبلا فروخته شده است", barcode)
            });

        return item is null ? null : mapper.Map<GetProductResponse>(item);
    }

    public async Task CreateAsync(ProductRequestDto request, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var item = Product.Create(
            request.Name,
            request.Barcode,
            request.Weight,
            request.Wage,
            request.ProductType,
            request.CaratType,
            request.GoldUnitType,
            request.WageType,
            request.WagePriceUnitId.HasValue ? new PriceUnitId(request.WagePriceUnitId.Value) : null,
            request.ProductCategoryId.HasValue ? new ProductCategoryId(request.ProductCategoryId.Value) : null);

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

    public async Task UpdateAsync(Guid id, ProductRequestDto request, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var item = await repository.Get(new ProductsByIdSpecification(new ProductId(id)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        item.SetName(request.Name);
        item.SetBarcode(request.Barcode);
        item.SetWeight(request.Weight);
        item.SetWage(request.Wage);
        item.SetWageType(request.WageType);
        item.SetProductType(request.ProductType);
        item.SetCaratType(request.CaratType);
        item.SetGoldUnitType(request.GoldUnitType);

        if (request.ProductCategoryId.HasValue)
            item.SetProductCategory(new ProductCategoryId(request.ProductCategoryId.Value));
        else
            item.SetProductCategory(null);

        if (request.WagePriceUnitId.HasValue)
            item.SetWagePriceUnitId(new PriceUnitId(request.WagePriceUnitId.Value)); 
        else
            item.SetWagePriceUnitId(null);

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
            item.ClearGemStones();

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