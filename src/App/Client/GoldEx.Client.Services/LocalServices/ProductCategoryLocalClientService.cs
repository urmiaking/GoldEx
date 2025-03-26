using GoldEx.Client.Abstractions.LocalServices;
using GoldEx.Client.Offline.Domain.ProductCategoryAggregate;
using GoldEx.Sdk.Common.Definitions;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Shared.Application.Services.Abstractions;
using GoldEx.Shared.Domain.Aggregates.ProductCategoryAggregate;
using GoldEx.Shared.DTOs.Categories;
using MapsterMapper;

namespace GoldEx.Client.Services.LocalServices;

[ScopedService]
public class ProductCategoryLocalClientService(IMapper mapper,
    IProductCategoryService<ProductCategory> service) : IProductCategoryLocalClientService
{
    public async Task<List<GetCategoryResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var list = await service.GetAllAsync(cancellationToken);

        return mapper.Map<List<GetCategoryResponse>>(list);
    }

    public async Task<GetCategoryResponse?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await service.GetAsync(new ProductCategoryId(id), cancellationToken);

        return item is null ? null : mapper.Map<GetCategoryResponse>(item);
    }

    public async Task<bool> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var category = new ProductCategory(request.Title);

        await service.CreateAsync(category, cancellationToken);

        return true;
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var item = await service.GetAsync(new ProductCategoryId(id), cancellationToken);

        if (item is null)
            return false;
        
        item.SetTitle(request.Title);

        // In case the item is synced, status changes to updated otherwise the previous status remains. e,g. Created
        if (item.Status == ModifyStatus.Synced)
            item.SetStatus(ModifyStatus.Updated);

        await service.UpdateAsync(item, cancellationToken);

        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, bool deletePermanently = false,
        CancellationToken cancellationToken = default)
    {
        var item = await service.GetAsync(new ProductCategoryId(id), cancellationToken);

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

    public async Task<List<GetPendingCategoryResponse>> GetPendingsAsync(DateTime checkpointDate, CancellationToken cancellationToken = default)
    {
        var items = await service.GetPendingItemsAsync(checkpointDate, cancellationToken);

        return mapper.Map<List<GetPendingCategoryResponse>>(items);
    }

    public async Task SetSyncedAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await service.GetAsync(new ProductCategoryId(id), cancellationToken)
                   ?? throw new NotFoundException("دسته بندی مورد نظر یافت نشد");

        item.SetStatus(ModifyStatus.Synced);

        await service.UpdateAsync(item, cancellationToken);
    }

    public async Task CreateAsSyncedAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var category = new ProductCategory(new ProductCategoryId(request.Id), request.Title);

        category.SetStatus(ModifyStatus.Synced);

        await service.CreateAsync(category, cancellationToken);
    }

    public async Task UpdateAsSyncAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var item = await service.GetAsync(new ProductCategoryId(id), cancellationToken);

        if (item is null)
            return;

        item.SetTitle(request.Title);
        item.SetStatus(ModifyStatus.Synced);

        await service.UpdateAsync(item, cancellationToken);
    }
}