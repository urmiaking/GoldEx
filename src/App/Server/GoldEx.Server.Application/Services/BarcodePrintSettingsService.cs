using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Server.Domain.SettingAggregate;
using GoldEx.Server.Domain.SettingAggregate.ValueObjects;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Settings;
using GoldEx.Shared.DTOs.Settings.Barcodes;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class BarcodePrintSettingsService(
    ISettingRepository repository,
    IMapper mapper) : IBarcodePrintSettingsService
{
    public async Task<GetBarcodePrintSettingsResponse> GetAsync(CancellationToken cancellationToken = default)
    {
        var settings = await repository
            .Get(new SettingsDefaultSpecification())
            .FirstOrDefaultAsync(cancellationToken);

        if (settings is null)
            throw new NotFoundException("تنظیمات یافت نشد");

        if (settings.BarcodePrintSettings is null)
        {
            var barcodeSettings = BarcodePrintSettings.CreateDefault();
            settings.UpdateBarcodePrintSettings(barcodeSettings);
            await repository.UpdateAsync(settings, cancellationToken);
        }

        var bps = settings.BarcodePrintSettings;

        return mapper.Map<GetBarcodePrintSettingsResponse>(bps!);
    }

    public async Task UpdateAsync(UpdateBarcodePrintSettingsRequest request, CancellationToken cancellationToken = default)
    {
        var settings = await repository
            .Get(new SettingsDefaultSpecification())
            .FirstOrDefaultAsync(cancellationToken);

        if (settings is null)
            throw new NotFoundException("تنظیمات یافت نشد");

        var barcodePrintSettings = BarcodePrintSettings.CreateDefault();

        barcodePrintSettings.UpdateLabelDimensions(request.LabelWidth, request.LabelHeight, request.TailWidth);

        barcodePrintSettings.UpdateMargin(BarcodeMargin.Create(
            request.MarginTop,
            request.MarginRight,
            request.MarginBottom,
            request.MarginLeft));

        barcodePrintSettings.UpdatePadding(BarcodePadding.Create(
            request.PaddingTop,
            request.PaddingRight,
            request.PaddingBottom,
            request.PaddingLeft));

        barcodePrintSettings.ClearAll();

        foreach (var item in request.PositionItems)
        {
            BarcodeDisplaySettings? barcodeSettings = null;

            if (item is { ItemType: BarcodePrintableItem.Barcode, BarcodeSettings: not null })
            {
                barcodeSettings = BarcodeDisplaySettings.Create(
                    item.BarcodeSettings.Width,
                    item.BarcodeSettings.Height,
                    item.BarcodeSettings.DisplayValue,
                    item.BarcodeSettings.FontSize,
                    item.BarcodeSettings.Margin,
                    item.BarcodeSettings.BarWidthMultiplier);
            }

            barcodePrintSettings.AddPositionItem(
                item.Position,
                item.ItemType,
                item.Order,
                item.IsVisible,
                item.FontSize,
                item.ItemSpacing,
                barcodeSettings);
        }

        settings.UpdateBarcodePrintSettings(barcodePrintSettings);

        await repository.UpdateAsync(settings, cancellationToken);
    }
}