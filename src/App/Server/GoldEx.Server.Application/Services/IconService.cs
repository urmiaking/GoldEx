using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Application.Utilities;
using GoldEx.Shared.Enums;
using Microsoft.AspNetCore.Hosting;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class IconService(IWebHostEnvironment environment) : IIconService
{
    public async Task<byte[]?> GetIconAsync(IconType iconType, Guid id, CancellationToken cancellationToken = default)
    {
        switch (iconType)
        {
            case IconType.Price:
                if (environment.PriceHistoryIconExists(id))
                {
                    var path = environment.GetPriceHistoryIconPath(id, null);

                    if (File.Exists(path))
                        return await File.ReadAllBytesAsync(path, cancellationToken);
                }
                break;
            case IconType.PriceUnit:
                if (environment.PriceUnitIconExists(id))
                {
                    var path = environment.GetPriceUnitIconPath(id, null);
                    if (File.Exists(path))
                        return await File.ReadAllBytesAsync(path, cancellationToken);
                }
                break;
            case IconType.App:
                if (environment.AppIconExists())
                {
                    var path = environment.GetAppIconPath();
                    if (File.Exists(path))
                        return await File.ReadAllBytesAsync(path, cancellationToken);
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(iconType), iconType, null);
        }

        return null;
    }
}