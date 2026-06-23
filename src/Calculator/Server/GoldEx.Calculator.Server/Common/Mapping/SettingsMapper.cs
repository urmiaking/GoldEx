using GoldEx.Server.Application.Utilities;
using GoldEx.Server.Domain.SettingAggregate;
using GoldEx.Server.Domain.SettingAggregate.ValueObjects;
using GoldEx.Shared.DTOs.Settings;
using GoldEx.Shared.DTOs.Settings.Barcodes;
using Mapster;

namespace GoldEx.Calculator.Server.Common.Mapping;

internal class SettingsMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Setting, GetSettingResponse>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.UsedGoldFinenessDeductionRate, src => src.UsedGoldFinenessDeductionRate)
            .Map(dest => dest.HasIcon, src => MapContext.Current.GetService<IWebHostEnvironment>().AppIconExists());

        config.NewConfig<BarcodePrintSettings, GetBarcodePrintSettingsResponse>()
            .Map(dest => dest.LabelWidth, src => src.LabelWidth)
            .Map(dest => dest.LabelHeight, src => src.LabelHeight)
            .Map(dest => dest.TailWidth, src => src.TailWidth)
            .Map(dest => dest.MarginTop, src => src.Margin.Top)
            .Map(dest => dest.MarginRight, src => src.Margin.Right)
            .Map(dest => dest.MarginBottom, src => src.Margin.Bottom)
            .Map(dest => dest.MarginLeft, src => src.Margin.Left)
            .Map(dest => dest.PaddingTop, src => src.Padding.Top)
            .Map(dest => dest.PaddingRight, src => src.Padding.Right)
            .Map(dest => dest.PaddingBottom, src => src.Padding.Bottom)
            .Map(dest => dest.PaddingLeft, src => src.Padding.Left)
            .Map(dest => dest.PositionItems, src => src.PositionItems);

        config.NewConfig<BarcodePositionItem, GetBarcodePositionItemResponse>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.Position, src => src.Position)
            .Map(dest => dest.ItemType, src => src.ItemType)
            .Map(dest => dest.Order, src => src.Order)
            .Map(dest => dest.IsVisible, src => src.IsVisible)
            .Map(dest => dest.FontSize, src => src.FontSize)
            .Map(dest => dest.ItemSpacing, src => src.ItemSpacing)
            .Map(dest => dest.BarcodeSettings, src => src.BarcodeSettings);

        config.NewConfig<BarcodeDisplaySettings, BarcodeDisplaySettingsDto>();
    }
}
