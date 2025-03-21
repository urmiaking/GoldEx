using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Shared.DTOs.Products;
using GoldEx.Shared.Services;
using MapsterMapper;
using System.Security.Claims;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Server.Domain.ProductCategoryAggregate;
using GoldEx.Shared.Application.Services.Abstractions;
using GoldEx.Shared.Domain.Aggregates.ProductAggregate;
using GoldEx.Shared.Domain.Aggregates.ProductCategoryAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.ClientServices;

[ScopedService]
public class ProductClientService(
    IProductService<Product, ProductCategory, GemStone> service,
    IHttpContextAccessor httpContextAccessor,
    IMapper mapper) : IProductClientService
{
    public async Task<PagedList<GetProductResponse>> GetListAsync(RequestFilter filter, CancellationToken cancellationToken = default)
    {
        var list = await service.GetListAsync(filter, cancellationToken);

        return mapper.Map<PagedList<GetProductResponse>>(list);
    }

    public async Task<GetProductResponse?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await service.GetAsync(new ProductId(id), cancellationToken);

        return product is null ? null : mapper.Map<GetProductResponse>(product);
    }

    public async Task<GetProductResponse?> GetAsync(string barcode, CancellationToken cancellationToken = default)
    {
        var product = await service.GetAsync(barcode, cancellationToken);

        return product is null ? null : mapper.Map<GetProductResponse>(product);
    }

    public async Task CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        var userIdClaim = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            throw new InvalidOperationException("Invalid User ID.");

        var product = new Product(new ProductId(request.Id),
            request.Name,
            request.Barcode,
            request.Weight,
            request.Wage,
            request.ProductType,
            request.WageType,
            request.CaratType,
            userId,
            new ProductCategoryId(request.ProductCategoryId));

        product.SetCreatedUserId(userId);

        if (request.ProductType is ProductType.Jewelry && request.GemStones is not null)
        {
            product.SetGemStones(request.GemStones.Select(x => new GemStone(x.Type,
                    x.Color,
                    x.Cut,
                    x.Carat,
                    x.Purity))
                .ToList());
        }

        await service.CreateAsync(product, cancellationToken);
    }

    public async Task UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        var product = await service.GetAsync(new ProductId(id), cancellationToken);

        if (product is null)
            throw new NotFoundException("جنس یافت نشد");

        product.SetName(request.Name);
        product.SetBarcode(request.Barcode);
        product.SetWeight(request.Weight);
        product.SetWage(request.Wage);
        product.SetWageType(request.WageType);
        product.SetProductType(request.ProductType);
        product.SetCaratType(request.CaratType);
        product.SetProductCategory(new ProductCategoryId(request.ProductCategoryId));
        product.ClearGemStones();

        if (request.ProductType is ProductType.Jewelry && request.GemStones is not null)
        {
            product.SetGemStones(request.GemStones.Select(x => new GemStone(x.Type,
                    x.Color,
                    x.Cut,
                    x.Carat,
                    x.Purity))
                .ToList());
        }

        await service.UpdateAsync(product, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, bool deletePermanently = false, CancellationToken cancellationToken = default)
    {
        var product = await service.GetAsync(new ProductId(id), cancellationToken);

        if (product is null)
            throw new NotFoundException("جنس یافت نشد");

        await service.DeleteAsync(product, deletePermanently, cancellationToken);
    }

    public async Task<List<GetPendingProductResponse>> GetPendingsAsync(DateTime checkpointDate,
        CancellationToken cancellationToken = default)
    {
        var items = await service.GetPendingItemsAsync(checkpointDate, cancellationToken);

        return mapper.Map<List<GetPendingProductResponse>>(items);
    }
}