using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Application.Utilities;
using GoldEx.Server.Domain.StoreAggregate;
using GoldEx.Server.Infrastructure;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class IconService(
    IWebHostEnvironment environment,
    IStoreContext storeContext,
    GoldExDbContext dbContext,
    IHttpContextAccessor httpContextAccessor) : IIconService
{
    public async Task<byte[]?> GetIconAsync(IconType iconType, Guid id, CancellationToken cancellationToken = default)
    {
        switch (iconType)
        {
            case IconType.Price:
                {
                    var path = environment.GetPriceHistoryIconPath(id, null);

                    if (File.Exists(path))
                        return await File.ReadAllBytesAsync(path, cancellationToken);
                }
                break;
            case IconType.PriceUnit:
                {
                    var path = environment.GetPriceUnitIconPath(id, null);
                    if (File.Exists(path))
                        return await File.ReadAllBytesAsync(path, cancellationToken);
                }
                break;
            case IconType.App:
                {
                    var storeSlug = await GetAppIconSlugAsync(id, cancellationToken);
                    var path = environment.GetAppIconPath(storeSlug);
                    if (File.Exists(path))
                        return await File.ReadAllBytesAsync(path, cancellationToken);
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(iconType), iconType, null);
        }

        return null;
    }

    public string? GetIconPath(IconType iconType, Guid id)
    {
        var path = iconType switch
        {
            IconType.Price => environment.GetPriceHistoryIconPath(id, null),
            IconType.PriceUnit => environment.GetPriceUnitIconPath(id, null),
            IconType.App => environment.GetAppIconPath(GetAppIconSlug(id)),
            _ => throw new ArgumentOutOfRangeException(nameof(iconType), iconType, null)
        };

        return File.Exists(path) ? path : null;
    }

    private async Task<string> GetAppIconSlugAsync(Guid id, CancellationToken cancellationToken)
    {
        if (id != Guid.Empty)
        {
            var slug = await dbContext.Set<Store>()
                .AsNoTracking()
                .IgnoreQueryFilters()
                .Where(x => x.Id == new StoreId(id))
                .Select(x => x.Slug)
                .FirstOrDefaultAsync(cancellationToken);
            if (!string.IsNullOrWhiteSpace(slug))
                return slug;
        }

        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext != null &&
            httpContext.Request.Cookies.TryGetValue("ActiveStoreId", out var cookieValue) &&
            Guid.TryParse(cookieValue, out var cookieStoreId))
        {
            var slug = await dbContext.Set<Store>()
                .AsNoTracking()
                .IgnoreQueryFilters()
                .Where(x => x.Id == new StoreId(cookieStoreId))
                .Select(x => x.Slug)
                .FirstOrDefaultAsync(cancellationToken);
            if (!string.IsNullOrWhiteSpace(slug))
                return slug;
        }

        if (!string.IsNullOrWhiteSpace(storeContext.StoreSlug))
        {
            return storeContext.StoreSlug;
        }

        return "default";
    }

    private string GetAppIconSlug(Guid id)
    {
        if (id != Guid.Empty)
        {
            var slug = dbContext.Set<Store>()
                .AsNoTracking()
                .IgnoreQueryFilters()
                .Where(x => x.Id == new StoreId(id))
                .Select(x => x.Slug)
                .FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(slug))
                return slug;
        }

        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext != null &&
            httpContext.Request.Cookies.TryGetValue("ActiveStoreId", out var cookieValue) &&
            Guid.TryParse(cookieValue, out var cookieStoreId))
        {
            var slug = dbContext.Set<Store>()
                .AsNoTracking()
                .IgnoreQueryFilters()
                .Where(x => x.Id == new StoreId(cookieStoreId))
                .Select(x => x.Slug)
                .FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(slug))
                return slug;
        }

        if (!string.IsNullOrWhiteSpace(storeContext.StoreSlug))
        {
            return storeContext.StoreSlug;
        }

        return "default";
    }
}