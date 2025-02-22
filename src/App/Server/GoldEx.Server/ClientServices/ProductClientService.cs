using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Shared.DTOs.Products;
using GoldEx.Shared.Services;
using MapsterMapper;
using System.Security.Claims;
using GoldEx.Sdk.Server.Application.Exceptions;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.ClientServices;

[ScopedService]
internal class ProductClientService(IProductService service, IHttpContextAccessor httpContextAccessor, IMapper mapper) : IProductClientService
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
            throw new InvalidOperationException("User ID not found or invalid.");

        var product = new Product(request.Name, request.Barcode, request.Weight, request.Wage, request.ProductType,
            request.WageType, request.CaratType, userId);

        product.SetCreatedUserId(userId);

        await service.CreateAsync(product, cancellationToken);
    }

    public async Task UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        var product = await service.GetAsync(new ProductId(id), cancellationToken);

        if (product is null)
            throw new NotFoundException("کالا یافت نشد");

        product.SetName(request.Name);
        product.SetBarcode(request.Barcode);
        product.SetWeight(request.Weight);
        product.SetWage(request.Wage);
        product.SetWageType(request.WageType);
        product.SetProductType(request.ProductType);
        product.SetCaratType(request.CaratType);

        await service.UpdateAsync(product, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await service.GetAsync(new ProductId(id), cancellationToken);

        if (product is null)
            throw new NotFoundException("کالا یافت نشد");

        await service.DeleteAsync(product, cancellationToken);
    }
}