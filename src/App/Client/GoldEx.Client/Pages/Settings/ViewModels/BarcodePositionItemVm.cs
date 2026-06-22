using GoldEx.Shared.DTOs.Settings.Barcodes;
using GoldEx.Shared.Enums;
using MudBlazor;

namespace GoldEx.Client.Pages.Settings.ViewModels;

public class BarcodePositionItemVm
{
    public Guid? Id { get; set; }
    public BarcodePosition Position { get; set; }
    public BarcodePrintableItem ItemType { get; set; }  
    public int Order { get; set; }
    public bool IsVisible { get; set; } = true;
    public double FontSize { get; set; } = 7.0;
    public double ItemSpacing { get; set; } = 0.5;
    public bool IsInToolbox { get; set; }

    public double BarcodeWidth { get; set; } = 22.0;
    public double BarcodeHeight { get; set; } = 8.0;
    public bool BarcodeDisplayValue { get; set; } = true;
    public double BarcodeFontSize { get; set; } = 7.0;
    public double BarcodeMargin { get; set; } = 0.0;
    public int BarWidthMultiplier { get; set; } = 2;

    public string UniqueKey => IsInToolbox
        ? $"toolbox-{ItemType}"
        : $"{Position}-{ItemType}-{Id?.ToString() ?? Guid.CreateVersion7().ToString()}";

    public static BarcodePositionItemVm CreateToolboxItem(BarcodePrintableItem itemType)
    {
        return new BarcodePositionItemVm
        {
            ItemType = itemType,
            IsInToolbox = true,
            Order = 0,
            IsVisible = true,
            FontSize = 7.0,
            ItemSpacing = 0.5,
            BarcodeWidth = 22.0,
            BarcodeHeight = 8.0,
            BarcodeDisplayValue = true,
            BarcodeFontSize = 7.0,
            BarcodeMargin = 0.0,
            BarWidthMultiplier = 2
        };
    }


    public static BarcodePositionItemVm CreateFrom(GetBarcodePositionItemResponse response)
    {
        var vm = new BarcodePositionItemVm
        {
            Id = response.Id,
            Position = response.Position,
            ItemType = response.ItemType,
            Order = response.Order,
            IsVisible = response.IsVisible,
            FontSize = response.FontSize,
            ItemSpacing = response.ItemSpacing,
            IsInToolbox = false
        };

        if (response.BarcodeSettings != null)
        {
            vm.BarcodeWidth = response.BarcodeSettings.Width;
            vm.BarcodeHeight = response.BarcodeSettings.Height;
            vm.BarcodeDisplayValue = response.BarcodeSettings.DisplayValue;
            vm.BarcodeFontSize = response.BarcodeSettings.FontSize;
            vm.BarcodeMargin = response.BarcodeSettings.Margin;
            vm.BarWidthMultiplier = response.BarcodeSettings.BarWidthMultiplier;
        }

        return vm;
    }

    public BarcodePositionItemRequest ToRequest()
    {
        BarcodeDisplaySettingsDto? barcodeSettings = null;

        if (ItemType == BarcodePrintableItem.Barcode)
        {
            barcodeSettings = new BarcodeDisplaySettingsDto(
                BarcodeWidth,
                BarcodeHeight,
                BarcodeDisplayValue,
                BarcodeFontSize,
                BarcodeMargin,
                BarWidthMultiplier);
        }

        return new BarcodePositionItemRequest(
            Id,
            Position,
            ItemType,
            Order,
            IsVisible,
            FontSize,
            ItemSpacing,
            barcodeSettings
        );
    }

    public string GetLabel()
    {
        return ItemType switch
        {
            BarcodePrintableItem.Barcode => "بارکد",
            BarcodePrintableItem.ProductName => "عنوان",
            BarcodePrintableItem.Weight => "وزن",
            BarcodePrintableItem.Wage => "اجرت",
            _ => "نامشخص"
        };
    }

    public string GetIcon()
    {
        return ItemType switch
        {
            BarcodePrintableItem.Barcode => Icons.Material.Filled.QrCode,
            BarcodePrintableItem.ProductName => Icons.Material.Filled.Label,
            BarcodePrintableItem.Weight => Icons.Material.Filled.Scale,
            BarcodePrintableItem.Wage => Icons.Material.Filled.AttachMoney,
            _ => Icons.Material.Filled.Help
        };
    }

    public Color GetColor()
    {
        return ItemType switch
        {
            BarcodePrintableItem.Barcode => Color.Primary,
            BarcodePrintableItem.ProductName => Color.Secondary,
            BarcodePrintableItem.Weight => Color.Tertiary,
            BarcodePrintableItem.Wage => Color.Info,
            _ => Color.Default
        };
    }

    // برای equality comparison در List
    public override bool Equals(object? obj)
    {
        if (obj is not BarcodePositionItemVm other) return false;
        return UniqueKey == other.UniqueKey;
    }

    public override int GetHashCode()
    {
        return UniqueKey.GetHashCode();
    }
}