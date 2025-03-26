using GoldEx.Client.Abstractions.LocalServices;
using GoldEx.Client.Offline.Domain.ProductAggregate;
using GoldEx.Client.Offline.Domain.ProductCategoryAggregate;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.Definitions;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Shared.Application.Services.Abstractions;
using GoldEx.Shared.Domain.Aggregates.ProductAggregate;
using GoldEx.Shared.Domain.Aggregates.ProductCategoryAggregate;
using GoldEx.Shared.DTOs.Products;
using GoldEx.Shared.Enums;
using MapsterMapper;

namespace GoldEx.Client.Services.LocalServices;

[ScopedService]
public class ProductLocalClientService(IMapper mapper, IProductService<Product, ProductCategory, GemStone> service) : IProductLocalClientService
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

    public async Task<bool> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        var product = new Product(request.Name,
            request.Barcode,
            request.Weight,
            request.Wage,
            request.ProductType,
            request.WageType,
            request.CaratType,
            new ProductCategoryId(request.ProductCategoryId));

        if (request.ProductType is ProductType.Jewelry && request.GemStones is not null)
        {
            product.SetGemStones(request.GemStones.Select(x => new GemStone(
                    x.Code,
                    x.Type,
                    x.Color,
                    x.Cut,
                    x.Carat,
                    x.Purity))
                .ToList());
        }

        await service.CreateAsync(product, cancellationToken);

        return true;
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var item = await service.GetAsync(new ProductId(id), cancellationToken);

        if (item is null)
            return false;
        
        item.SetName(request.Name);
        item.SetBarcode(request.Barcode);
        item.SetWeight(request.Weight);
        item.SetWage(request.Wage);
        item.SetProductType(request.ProductType);
        item.SetWageType(request.WageType);
        item.SetCaratType(request.CaratType);
        item.SetProductCategory(new ProductCategoryId(request.ProductCategoryId));

        item.ClearGemStones();

        if (request.ProductType is ProductType.Jewelry && request.GemStones is not null)
        {
            item.SetGemStones(request.GemStones.Select(x => new GemStone(
                    x.Code,
                    x.Type,
                    x.Color,
                    x.Cut,
                    x.Carat,
                    x.Purity))
                .ToList());
        }

        // In case the item is synced, status changes to updated otherwise the previous status remains. e,g. Created
        if (item.Status == ModifyStatus.Synced)
            item.SetStatus(ModifyStatus.Updated);
        
        await service.UpdateAsync(item, cancellationToken);

        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, bool deletePermanently = false,
        CancellationToken cancellationToken = default)
    {
        var item = await service.GetAsync(new ProductId(id), cancellationToken);

        if (item is null)
            return false;

        // In case the item is created locally and is not synced to server, it will be deleted permanently
        if (item.Status == ModifyStatus.Created)
        {
            await service.DeleteAsync(item, true, cancellationToken);
            return true;
        }

        if (deletePermanently)
        {
            await service.DeleteAsync(item, deletePermanently, cancellationToken);
        }
        else
        {
            item.SetStatus(ModifyStatus.Deleted);
            await service.UpdateAsync(item, cancellationToken);
        }

        return true;
    }

    public async Task<List<GetPendingProductResponse>> GetPendingsAsync(DateTime checkpointDate,
        CancellationToken cancellationToken = default)
    {
        var items = await service.GetPendingItemsAsync(checkpointDate, cancellationToken);

        return mapper.Map<List<GetPendingProductResponse>>(items);
    }

    public async Task SetSyncedAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await service.GetAsync(new ProductId(id), cancellationToken) 
                   ?? throw new NotFoundException("جنس مورد نظر یافت نشد");

        item.SetStatus(ModifyStatus.Synced);

        await service.UpdateAsync(item, cancellationToken);
    }

    public async Task CreateAsSyncedAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        var product = new Product(new ProductId(request.Id),
            request.Name,
            request.Barcode,
            request.Weight,
            request.Wage,
            request.ProductType,
            request.WageType,
            request.CaratType,
            new ProductCategoryId(request.ProductCategoryId));

        if (request.ProductType is ProductType.Jewelry && request.GemStones is not null)
        {
            product.SetGemStones(request.GemStones.Select(x => new GemStone(
                    x.Code,
                    x.Type,
                    x.Color,
                    x.Cut,
                    x.Carat,
                    x.Purity))
                .ToList());
        }

        product.SetStatus(ModifyStatus.Synced);

        await service.CreateAsync(product, cancellationToken);
    }

    public async Task UpdateAsSyncAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        var item = await service.GetAsync(new ProductId(id), cancellationToken);

        if (item is null)
            return;
        
        item.SetName(request.Name);
        item.SetBarcode(request.Barcode);
        item.SetWeight(request.Weight);
        item.SetWage(request.Wage);
        item.SetProductType(request.ProductType);
        item.SetWageType(request.WageType);
        item.SetCaratType(request.CaratType);
        item.ClearGemStones();

        if (request.ProductType is ProductType.Jewelry && request.GemStones is not null)
        {
            item.SetGemStones(request.GemStones.Select(x => new GemStone(
                    x.Code,
                    x.Type,
                    x.Color,
                    x.Cut,
                    x.Carat,
                    x.Purity))
                .ToList());
        }

        item.SetStatus(ModifyStatus.Synced);

        await service.UpdateAsync(item, cancellationToken);
    }
}