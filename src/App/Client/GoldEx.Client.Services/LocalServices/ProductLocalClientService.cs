using GoldEx.Client.Abstractions.LocalServices;
using GoldEx.Client.Offline.Domain.ProductAggregate;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.Definitions;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Shared.Application.Services.Abstractions;
using GoldEx.Shared.Domain.Aggregates.ProductAggregate;
using GoldEx.Shared.DTOs.Products;
using MapsterMapper;

namespace GoldEx.Client.Services.LocalServices;

[ScopedService]
public class ProductLocalClientService(IMapper mapper, IProductService<Product> service) : IProductLocalClientService
{
    public async Task<PagedList<GetProductResponse>> GetListAsync(RequestFilter filter, CancellationToken cancellationToken = default)
    {
        var list = await service.GetListAsync(filter, cancellationToken);

        return mapper.Map<PagedList<GetProductResponse>>(list);
    }

    public async Task<GetProductResponse?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await service.GetAsync(new ProductId(id), cancellationToken);

        return item is null ? null : mapper.Map<GetProductResponse>(item);
    }

    public async Task<GetProductResponse?> GetAsync(string barcode, CancellationToken cancellationToken = default)
    {
        var item = await service.GetAsync(barcode, cancellationToken);

        return item is null ? null : mapper.Map<GetProductResponse>(item);
    }

    public async Task CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        var product = new Product(request.Name,
            request.Barcode,
            request.Weight,
            request.Wage,
            request.ProductType,
            request.WageType,
            request.CaratType);

        await service.CreateAsync(product, cancellationToken);
    }

    public async Task UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        var item = await service.GetAsync(new ProductId(id), cancellationToken) ?? throw new NotFoundException();

        item.SetName(request.Name);
        item.SetBarcode(request.Barcode);
        item.SetWeight(request.Weight);
        item.SetWage(request.Wage);
        item.SetProductType(request.ProductType);
        item.SetWageType(request.WageType);
        item.SetCaratType(request.CaratType);
        item.SetLastModifiedDate(DateTime.UtcNow);

        await service.UpdateAsync(item, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, bool deletePermanently = false, CancellationToken cancellationToken = default)
    {
        var item = await service.GetAsync(new ProductId(id), cancellationToken) ?? throw new NotFoundException();

        if (deletePermanently)
        {
            await service.DeleteAsync(item, deletePermanently, cancellationToken);
        }
        else
        {
            item.SetStatus(ModifyStatus.Deleted);
            item.SetLastModifiedDate(DateTime.UtcNow);
            await service.UpdateAsync(item, cancellationToken);
        }   
    }

    public async Task<List<GetPendingProductResponse>> GetPendingsAsync(DateTime checkpointDate,
        CancellationToken cancellationToken = default)
    {
        var items = await service.GetPendingItemsAsync(checkpointDate, cancellationToken);

        return mapper.Map<List<GetPendingProductResponse>>(items);
    }

    public async Task SetSyncedAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await service.GetAsync(new ProductId(id), cancellationToken) ?? throw new NotFoundException();

        item.SetStatus(ModifyStatus.Synced);

        await service.UpdateAsync(item, cancellationToken);
    }

    public async Task CreateAsync(CreateProductRequest request, ModifyStatus status, CancellationToken cancellationToken = default)
    {
        var product = new Product(new ProductId(request.Id),
            request.Name,
            request.Barcode,
            request.Weight,
            request.Wage,
            request.ProductType,
            request.WageType,
            request.CaratType);

        product.SetStatus(status);

        await service.CreateAsync(product, cancellationToken);
    }

    public async Task UpdateAsync(Guid id, UpdateProductRequest request, ModifyStatus status,
        CancellationToken cancellationToken = default)
    {
        var item = await service.GetAsync(new ProductId(id), cancellationToken) ?? throw new NotFoundException();

        item.SetName(request.Name);
        item.SetBarcode(request.Barcode);
        item.SetWeight(request.Weight);
        item.SetWage(request.Wage);
        item.SetProductType(request.ProductType);
        item.SetWageType(request.WageType);
        item.SetCaratType(request.CaratType);
        item.SetLastModifiedDate(DateTime.UtcNow);
        item.SetStatus(status);

        await service.UpdateAsync(item, cancellationToken);
    }
}